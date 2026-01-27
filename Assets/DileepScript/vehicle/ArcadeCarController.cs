using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcadeCarController : MonoBehaviour, ICarInputs
{
    [Header("Forces")]
    public float engineForce = 8000f;
    public float brakeForce = 12000f;
    public float maxSpeedMps = 45f;

    [Header("Steering")]
    public float steerTorque = 2200f;
    public float steerAssist = 2.0f;

    [Header("Grip / Stability")]
    public float lateralDamping = 6.0f;
    public float angularDamping = 3.0f;

    Rigidbody rb;

    float steer;
    float throttle;
    float brake;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetInputs(float steer, float throttle, float brake)
    {
        this.steer = Mathf.Clamp(steer, -1f, 1f);
        this.throttle = Mathf.Clamp01(throttle);
        this.brake = Mathf.Clamp01(brake);
    }

    void FixedUpdate()
    {
        Vector3 vel = rb.linearVelocity;
        float speed = vel.magnitude;

        Vector3 fwd = transform.forward;
        float forwardSpeed = Vector3.Dot(vel, fwd);

        if (Time.frameCount % 20 == 0)
            Debug.Log($"STEER_IN={steer:F2} THR={throttle:F2} speed={rb.linearVelocity.magnitude:F2} angY={rb.angularVelocity.y:F2}");


        if (speed < maxSpeedMps)
            rb.AddForce(fwd * (throttle * engineForce), ForceMode.Force);

        if (brake > 0.001f && speed > 0.5f)
        {
            Vector3 brakeDir = (forwardSpeed >= 0) ? -fwd : fwd;
            rb.AddForce(brakeDir * (brake * brakeForce), ForceMode.Force);
        }

        float steerScale = Mathf.Clamp01(speed / 5f);
        steerScale = Mathf.Max(0.35f, steerScale);

       
        float yawRateDeg = 140f * steerScale;               
        float yawThisStep = steer * yawRateDeg * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, yawThisStep, 0f));

        Vector3 right = transform.right;
        float lateralSpeed = Vector3.Dot(vel, right);
        rb.AddForce(-right * (lateralSpeed * lateralDamping), ForceMode.Force);

       rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, angularDamping * Time.fixedDeltaTime);

        if (speed > 5f && throttle > 0.1f && steerAssist > 0f)
      {
           Vector3 desiredVelDir = Vector3.Lerp(vel.normalized, fwd, steerAssist * Time.fixedDeltaTime).normalized;
            rb.linearVelocity = desiredVelDir * speed;
        }

    }
}
