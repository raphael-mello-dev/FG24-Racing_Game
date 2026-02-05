using Unity.VisualScripting;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Transform[] wheels;
    [SerializeField] private Transform[] frontWheels;
    [SerializeField] private Transform[] wheelMeshes;
    [SerializeField] private LayerMask drivable;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness = 40000f;
    [SerializeField] private float damperStiffness = 4000f;
    [SerializeField] private float springRestLength = 0.75f;
    [SerializeField] private float springMaxOffset = 0.15f;
    [SerializeField] private float wheelRadius = 0.33f;

    [Header("Input")]
    private float moveInput = 0f;
    private float steerInput = 0f;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 5000f;
    [SerializeField] private float deceleration = 2500f;
    [SerializeField] private float maxSpeed = 25f;
    public float MaxSpeed { get { return maxSpeed; } private set { maxSpeed = value; } }
    [SerializeField] private AnimationCurve powerCurve;

    [SerializeField] private AnimationCurve frontWheelGripCurve;
    [SerializeField] private AnimationCurve rearWheelGripCurve;
    [SerializeField] private float wheelMass = 20f;

    [SerializeField] private float wheelBase = 2.5f;
    [SerializeField] private float rearTrack = 1.5f;
    [SerializeField] private float turnRadius = 5f;


    private float springLength;
    private float ackermannAngleLeft;
    private float ackermannAngleRight;
    private float wheelGripFactor = 1f; // Value between 0 and 1 representing wheel grip

    private Vector3 wheelWorldVelocity = Vector3.zero;
    private Vector3 springDir = Vector3.zero;
    private Vector3 steeringDir = Vector3.zero;
    private Vector3 accelDir = Vector3.zero;
    private Vector3 wheelPos = Vector3.zero;

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
        if (GameManager.Instance.StateManager.currentState is not GameplayState) return;

        int i = 0;

        foreach (Transform wheel in wheels)
        {
            RaycastHit hit;
            float maxLength = springRestLength + springMaxOffset;
            if (Physics.Raycast(wheel.position, -wheel.up, out hit, maxLength + wheelRadius, drivable))
            {
                springDir = wheel.up;
                steeringDir = wheel.right;
                accelDir = wheel.forward;
                wheelPos = wheel.position;
                wheelWorldVelocity = carRigidbody.GetPointVelocity(wheel.position);

                Debug.DrawLine(wheel.position, hit.point, Color.red);

                Suspension(hit, i);
                //if (wheel == wheels[0] || wheel == wheels[1])
                //{
                //    Move(); 
                //}
                Move();
                ApplyDrag();
            }
            else
            {
                wheelMeshes[i].localPosition = Vector3.down * maxLength + (wheelRadius * Vector3.up);
                Debug.DrawLine(wheel.position, wheel.position + (wheelRadius + maxLength) * -wheel.up, Color.green);
            }
            i++;
        }
    }

    private void Update()
    {
        GetPlayerInput();
        Steer();
    }

    #endregion

    #region Input Handling

    private void GetPlayerInput()
    {
        //moveInput = Input.GetAxis("Vertical");
        //steerInput = Input.GetAxis("Horizontal");
        moveInput = GameManager.Instance.InputManager.Move;
        steerInput = GameManager.Instance.InputManager.Steer;
    }

    #endregion

    #region Car Physics Functions

    private void Suspension(RaycastHit hit, int i)
    {
        float offset = springRestLength - hit.distance;

        float velocity = Vector3.Dot(wheelWorldVelocity, springDir);

        float springForce = (offset * springStiffness) - (velocity * damperStiffness);
        carRigidbody.AddForceAtPosition(springForce * springDir, wheelPos);
        wheelMeshes[i].position = hit.point + (wheelRadius * Vector3.up);

    }

    private void Move()
    {
        float speed = Vector3.Dot(carRigidbody.linearVelocity, transform.forward);

        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(speed) / maxSpeed);

        float availableTorque;

        if (moveInput > 0)
        {
            availableTorque = powerCurve.Evaluate(normalizedSpeed) * moveInput * acceleration; 
        }
        else
        {
            availableTorque = powerCurve.Evaluate(normalizedSpeed) * moveInput * deceleration;
        }

        carRigidbody.AddForceAtPosition(availableTorque * accelDir, wheelPos);
    }

    private void Steer()
    {
        if (steerInput > 0) // Turning Right
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2f))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2f))) * steerInput;
        }
        else if (steerInput < 0) // Turning Left
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2f))) * steerInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2f))) * steerInput;
        }
        else // Going Straight
        {
            ackermannAngleLeft = 0f;
            ackermannAngleRight = 0f;
        }

        foreach (Transform frontWheel in frontWheels)
        {
            if (frontWheel == frontWheels[0]) // Left Front Wheel
            {
                frontWheel.localRotation = Quaternion.Euler(frontWheel.localRotation.x, frontWheel.localRotation.y + ackermannAngleLeft, frontWheel.localRotation.z);
            }
            else if (frontWheel == frontWheels[1]) // Right Front Wheel
            {
                frontWheel.localRotation = Quaternion.Euler(frontWheel.localRotation.x, frontWheel.localRotation.y + ackermannAngleRight, frontWheel.localRotation.z);
            }
        }
    }

    private void ApplyDrag()
    {
        float steeringVelocity = Vector3.Dot(steeringDir, wheelWorldVelocity);

        if (wheelPos == wheels[0].position || wheelPos == wheels[1].position)
        {
            wheelGripFactor = rearWheelGripCurve.Evaluate(steeringVelocity); 
        }
        else
        {
            wheelGripFactor = frontWheelGripCurve.Evaluate(steeringVelocity);
        }

        float desiredVelChange = -steeringVelocity * wheelGripFactor;

        float desiredAcceleration = desiredVelChange / Time.fixedDeltaTime;

        carRigidbody.AddForceAtPosition(steeringDir * desiredAcceleration * wheelMass, wheelPos);



        float forwardVelocity = Vector3.Dot(accelDir, wheelWorldVelocity);

        float desiredForwardVelChange = -forwardVelocity * 0.1f;

        float desiredForwardAcceleration = desiredForwardVelChange / Time.fixedDeltaTime;

        carRigidbody.AddForceAtPosition(accelDir * desiredForwardAcceleration * wheelMass, wheelPos);
    }

    #endregion
}
