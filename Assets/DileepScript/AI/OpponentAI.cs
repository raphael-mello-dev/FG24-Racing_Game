using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OpponentAI : MonoBehaviour
{
    public enum AIState { FollowLine, Overtake, Defend, Recover }

    [Header("References")]
    public WaypointPath path;
    public AISettings settings;
    public LayerMask carLayer;
    float logTimer;
    Rigidbody rb;
    ICarInputs car;

    public AIState state = AIState.FollowLine;
    public int currentIndex;
    public float currentLaneOffset;

    float stuckTimer;
    float steerSmoothed;
    float desiredSpeedJitter;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        car = GetComponent<ICarInputs>();
        if (car == null) Debug.LogError($"{name}: Missing ICarInputs component.");

        desiredSpeedJitter = Random.Range(-settings.speedJitter, settings.speedJitter);
        InvokeRepeating(nameof(ChangeJitter), 0.5f, settings.jitterChangeRate);

        if(path == null)
            path = FindFirstObjectByType<WaypointPath>();
    }

    void ChangeJitter()
    {
        desiredSpeedJitter = Random.Range(-settings.speedJitter, settings.speedJitter);
    }

    void FixedUpdate()
    {
        if (path == null || settings == null || path.Count < 2 || car == null) return;

        UpdateWaypointProgress();
        UpdateState();

        var wp = path.GetPoint(currentIndex);
        Debug.DrawLine(transform.position + Vector3.up, wp + Vector3.up, Color.green);
        logTimer += Time.fixedDeltaTime;
        if (logTimer > 0.25f)
        {
            logTimer = 0f;
            Debug.Log($"Dist to WP[{currentIndex}] = {Vector3.Distance(transform.position, wp):F2}");
        }

        float targetOffset = ChooseLaneOffsetByState();
        targetOffset += AvoidanceOffset();
        currentLaneOffset = Mathf.Lerp(currentLaneOffset, targetOffset, settings.avoidLerp);

        Vector3 target = GetLookaheadTarget(settings.lookaheadSteps, currentLaneOffset);

        float steer01 = ComputeSteerNormalized(target);
        steer01 = Mathf.Clamp(steer01, -settings.maxSteer, settings.maxSteer);
        steerSmoothed = Mathf.Lerp(steerSmoothed, steer01, settings.steerResponse * Time.fixedDeltaTime);

        float desiredSpeed = ComputeDesiredSpeed() + desiredSpeedJitter;

        float throttle, brake;
        SpeedControl(desiredSpeed, out throttle, out brake);

        RecoveryOverride(ref throttle, ref brake);

        car.SetInputs(steerSmoothed, throttle, brake);

        Debug.Log($"pos={transform.position} speed={rb.linearVelocity.magnitude:F2}");

    }

    void UpdateWaypointProgress()
    {
        Vector3 wp = path.GetPoint(currentIndex);
        if (Vector3.Distance(transform.position, wp) < settings.reachDist)
            currentIndex = (currentIndex + 1) % path.Count;
    }

    void UpdateState()
    {
        if (IsStuckOrWrongWay())
        {
            state = AIState.Recover;
            return;
        }

        bool carAhead = Physics.Raycast(SensorOrigin(0f), transform.forward, settings.sensorDistance, carLayer);
        bool carBehind = Physics.Raycast(transform.position + Vector3.up * 0.5f, -transform.forward, 8f, carLayer);

        if (carAhead) state = AIState.Overtake;
        else if (carBehind) state = AIState.Defend;
        else state = AIState.FollowLine;
    }

    float ChooseLaneOffsetByState()
    {
        switch (state)
        {
            case AIState.Overtake:
                return (Mathf.PerlinNoise(Time.time * 0.2f, transform.position.x) > 0.5f)
                    ? settings.laneOffsetMeters
                    : -settings.laneOffsetMeters;

            case AIState.Defend:
                return Mathf.Sign(currentLaneOffset == 0 ? 1 : currentLaneOffset) * (settings.laneOffsetMeters * 0.6f);

            case AIState.Recover:
            default:
                return 0f;
        }
    }

    Vector3 GetLookaheadTarget(int steps, float lateralOffsetMeters)
    {
        Vector3 p0 = path.GetPoint(currentIndex + steps);
        Vector3 p1 = path.GetPoint(currentIndex + steps + 1);

        Vector3 dir = (p1 - p0);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;

        Vector3 right = Vector3.Cross(Vector3.up, dir.normalized);
        return p0 + right * lateralOffsetMeters;
    }


    float ComputeSteerNormalized(Vector3 worldTarget)
    {
        Vector3 to = worldTarget - transform.position;
        to.y = 0;

        Vector3 fwd = transform.forward;
        fwd.y = 0;

        float angle = Vector3.SignedAngle(fwd.normalized, to.normalized, Vector3.up);
        return Mathf.Clamp(angle / 45f, -1f, 1f);
    }

    float ComputeDesiredSpeed()
    {
        Vector3 ahead = path.GetPoint(currentIndex + settings.speedLookaheadSteps);
        Vector3 toAhead = ahead - transform.position;
        toAhead.y = 0;

        Vector3 fwd = transform.forward; fwd.y = 0;

        float angle = Vector3.Angle(fwd.normalized, toAhead.normalized);
        return (angle > settings.cornerAngleDeg) ? settings.cornerSpeed : settings.straightSpeed;
    }

    void SpeedControl(float desiredSpeed, out float throttle, out float brake)
    {
        float speed = rb.linearVelocity.magnitude;
        float error = desiredSpeed - speed;

        if (error > 1.0f)
        {
            throttle = Mathf.Clamp01(error / 6f);
            brake = 0f;
        }
        else if (error < -1.0f)
        {
            throttle = 0f;
            brake = Mathf.Clamp01((-error) / 6f);
        }
        else
        {
            throttle = 0.25f;
            brake = 0f;
        }
    }

    bool IsStuckOrWrongWay()
    {
        float speed = rb.linearVelocity.magnitude;
        float forwardVel = Vector3.Dot(rb.linearVelocity, transform.forward);

        bool wrongWay = forwardVel < -0.5f && speed > 2f;
        bool slow = speed < settings.stuckSpeed;

        if (slow || wrongWay) stuckTimer += Time.fixedDeltaTime;
        else stuckTimer = 0f;

        return stuckTimer > settings.stuckTime;
    }

    void RecoveryOverride(ref float throttle, ref float brake)
    {
        if (state != AIState.Recover) return;

        brake = 0f;
        throttle = 1f;
        currentLaneOffset = Mathf.Lerp(currentLaneOffset, 0f, 0.25f);
    }

    float AvoidanceOffset()
    {
        Vector3 origin = SensorOrigin(0f);
        Vector3 leftOrigin = SensorOrigin(-settings.sensorSideOffset);
        Vector3 rightOrigin = SensorOrigin(settings.sensorSideOffset);

        bool hitCenter = Physics.Raycast(origin, transform.forward, settings.sensorDistance, carLayer);
        bool hitLeft = Physics.Raycast(leftOrigin, transform.forward, settings.sensorDistance, carLayer);
        bool hitRight = Physics.Raycast(rightOrigin, transform.forward, settings.sensorDistance, carLayer);

        if (!hitCenter && !hitLeft && !hitRight) return 0f;

        if (hitCenter)
        {
            if (!hitLeft && hitRight) return -settings.laneOffsetMeters;
            if (hitLeft && !hitRight) return settings.laneOffsetMeters;
            return (Random.value < 0.5f) ? -settings.laneOffsetMeters : settings.laneOffsetMeters;
        }

        float push = 0f;
        if (hitLeft) push += settings.laneOffsetMeters * 0.6f;
        if (hitRight) push -= settings.laneOffsetMeters * 0.6f;
        return push;
    }

    Vector3 SensorOrigin(float side)
    {
        return transform.position + Vector3.up * 0.5f + transform.forward * 1.5f + transform.right * side;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (path == null || settings == null || path.Count < 2) return;

        Vector3 target = GetLookaheadTarget(settings.lookaheadSteps, currentLaneOffset);
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(target + Vector3.up * 0.2f, 0.35f);
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f, target + Vector3.up * 0.2f);

        Gizmos.DrawLine(SensorOrigin(0f), SensorOrigin(0f) + transform.forward * settings.sensorDistance);
        Gizmos.DrawLine(SensorOrigin(-settings.sensorSideOffset), SensorOrigin(-settings.sensorSideOffset) + transform.forward * settings.sensorDistance);
        Gizmos.DrawLine(SensorOrigin(settings.sensorSideOffset), SensorOrigin(settings.sensorSideOffset) + transform.forward * settings.sensorDistance);
    }
#endif
}
