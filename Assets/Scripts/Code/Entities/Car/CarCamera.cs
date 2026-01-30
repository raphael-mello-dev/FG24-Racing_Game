using UnityEngine;

public class CarCamera : MonoBehaviour
{
    Rigidbody _rb;
    float _maxSpeed;
    Camera _cam;

    Transform _target;
    Vector3 _posOffset;
    Quaternion _rotOffset;

    public bool DEBUG_SLERP;
    public float speed = 0.1f;

    public Vector2 fovZoom = new Vector2(70, 110);

    private void Start()
    {
        _target = transform.parent;
        _posOffset = transform.localPosition;
        _rotOffset = transform.localRotation;

        _rb = GetComponentInParent<Rigidbody>();
        _maxSpeed = GetComponentInParent<CarController>().MaxSpeed;

        _cam = GetComponentInChildren<Camera>();

        transform.SetParent(null);
    }

    void Update()
    {
        float t = 1.0f - Mathf.Pow(2, -Time.unscaledDeltaTime * speed);
        transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, t);

        _cam.fieldOfView = Mathf.Lerp(fovZoom.x, fovZoom.y, _rb.linearVelocity.magnitude / _maxSpeed);
    }

    private void LateUpdate()
    {
        transform.position = _target.position + _posOffset;
    }
}
