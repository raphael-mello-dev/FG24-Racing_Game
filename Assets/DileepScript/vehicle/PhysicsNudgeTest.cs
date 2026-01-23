using UnityEngine;

public class PhysicsNudgeTest : MonoBehaviour
{
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        rb.AddForce(Vector3.forward * 5000f, ForceMode.Force);
    }
}
