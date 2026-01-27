using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    [Header("Sockets")]
    public Transform StartSocket;
    public Transform EndSocket;

    [Header("AI (Optional)")]
    public Transform[] Waypoints;

    [Header("Placement (Required)")]
    public BoxCollider OverlapBounds; 

    private void OnDrawGizmos()
    {
        if (StartSocket != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(StartSocket.position, StartSocket.forward * 2f);
        }

        if (EndSocket != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(EndSocket.position, EndSocket.forward * 2f);
        }
    }
}
