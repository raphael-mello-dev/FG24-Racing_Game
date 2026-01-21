using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform[] wheels;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float springRestLength;
    [SerializeField] private float springMaxOffset;
    [SerializeField] private float wheelRadius;

    private int[] wheelsAreGrounded = new int[4];
    private bool isGrounded = false;

    [Header("Input")]
    private float moveInput = 0f;
    private float steerInput = 0f;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStregth = 15f;
    [SerializeField] private AnimationCurve turningCurve;
    [SerializeField] private float dragCoefficient = 1f;

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0f;

    #region Unity Functions

    private void Start()
    {
        if (carRigidbody == null)
        {
            carRigidbody = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Move();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    #endregion

    #region Car Status Check

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < wheelsAreGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsAreGrounded[i];
        }

        if (tempGroundedWheels > 0)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRigidbody.linearVelocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    #endregion

    #region Input Handling

    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    #endregion

    #region Car Physics Functions

    private void Suspension()
    {
        int i = 0;

        foreach (Transform wheel in wheels)
        {
            RaycastHit hit;
            float maxLength = springRestLength + springMaxOffset;

            if (Physics.Raycast(wheel.position, -wheel.up, out hit, maxLength + wheelRadius, drivable))
            {
                wheelsAreGrounded[i++] = 1;

                float springLength = hit.distance - wheelRadius;
                float springCompression = (springRestLength - springLength) / springMaxOffset;

                float springVelocity = Vector3.Dot(carRigidbody.GetPointVelocity(wheel.position), wheel.up);
                float damperForce = damperStiffness * springVelocity;

                float springForce = springStiffness * springCompression;
                float totalForce = springForce - damperForce;
                carRigidbody.AddForceAtPosition(totalForce * wheel.up, wheel.position);

                Debug.DrawLine(wheel.position, hit.point, Color.red);
            }
            else
            {
                wheelsAreGrounded[i++] = 0;

                Debug.DrawLine(wheel.position, wheel.position + (wheelRadius + maxLength) * -wheel.up, Color.green);
            }
        }
    }

    private void Move()
    {
        if (isGrounded)
        {
            Accelerate();
            Decelerate();
            Steer();
            ApplyDrag();
        }
    }

    private void Accelerate()
    {
        carRigidbody.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Decelerate()
    {
        carRigidbody.AddForceAtPosition(deceleration * moveInput * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Steer()
    {
        carRigidbody.AddTorque(steerStregth * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * transform.up, ForceMode.Acceleration);
    }

    private void ApplyDrag()
    {
        float sidewaysVelocity = currentCarLocalVelocity.x;

        float dragMagnitude = dragCoefficient * -sidewaysVelocity;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRigidbody.AddForceAtPosition(dragForce, carRigidbody.worldCenterOfMass, ForceMode.Acceleration);
    }

    #endregion
}
