using UnityEngine;

public class RaceRespawner : MonoBehaviour
{
    Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Respawn();
        }
    }

    
    public void Respawn()
    {
        CheckpointManager.Racer racer = CheckpointManager.GetRacerInfo(transform);

        transform.position = racer.GetCheckpoint().transform.position;
        _rb.linearVelocity = Vector3.zero;
    }
}
