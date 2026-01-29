using UnityEngine;
using UnityEngine.InputSystem;

public class RaceRespawner : MonoBehaviour
{
    public bool isPlayer = false;
    public float resetPlane = -20;
    Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(isPlayer && Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }
        if (transform.position.y < resetPlane)
            Respawn();
    }

    
    public void Respawn()
    {
        CheckpointManager.Racer racer = CheckpointManager.GetRacerInfo(transform);

        CheckpointNode current = racer.GetCheckpoint();
        CheckpointNode next = current.next;

        Vector3 pos = current.transform.position;
        RaycastHit info;
        LayerMask mask = LayerMask.GetMask("Drivable");
        Physics.Raycast(pos, Vector3.down, out info, 100, mask);
        pos = info.point + Vector3.up*3;


        transform.position = current.transform.position;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.LookRotation(next.transform.position - current.transform.position);
    }
}
