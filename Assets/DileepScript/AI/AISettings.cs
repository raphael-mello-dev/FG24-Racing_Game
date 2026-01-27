using UnityEngine;

[CreateAssetMenu(menuName = "Racing/AI Settings")]
public class AISettings : ScriptableObject
{
    public float maxSteer = 1.0f;
    public float steerResponse = 6.0f;

    public int lookaheadSteps = 3;
    public int speedLookaheadSteps = 8;
    public float reachDist = 6f;

    public float straightSpeed = 28f;
    public float cornerSpeed = 15f;
    public float cornerAngleDeg = 25f;

    public float sensorDistance = 12f;
    public float sensorSideOffset = 0.9f;
    public float laneOffsetMeters = 1.6f;
    public float avoidLerp = 0.15f;

    public float stuckSpeed = 1.0f;
    public float stuckTime = 2.0f;

    public float speedJitter = 0.6f;
    public float jitterChangeRate = 0.7f;
}
