using UnityEngine;

public class ForceDriveTest : MonoBehaviour
{
    ICarInputs car;

    void Awake()
    {
        car = GetComponent<ICarInputs>();
        Debug.Log("ForceDriveTest Awake. car is " + (car == null ? "NULL" : "OK"));
    }

    void FixedUpdate()
    {
        if (car == null) return;

        car.SetInputs(0f, 1f, 0f);

        // This will spam logs a lot; just for testing
        Debug.Log("ForceDriveTest FixedUpdate running");
    }
}
