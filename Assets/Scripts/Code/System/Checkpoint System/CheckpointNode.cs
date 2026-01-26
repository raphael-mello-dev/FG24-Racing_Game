using UnityEngine;


//------------------------------------------------------------------------
//
//  This script was created by Milo. If you have questions or problems, ask her. 
//
//------------------------------------------------------------------------


public class CheckpointNode : MonoBehaviour
{
    public CheckpointNode prev;
    public CheckpointNode next;
    public bool isLapFlag = false;
    public bool setForwardAuto = false;

    [HideInInspector] public int index;

    [HideInInspector]
    public CheckpointManager.Racer DEBUG_TRANSFORM;

    protected Mesh _displayPlane;
    //protected Plane _plane;

    public bool HasVehiclePassed(Transform vehicle)
    {
        Vector3 direction = transform.position - vehicle.position;
        return Vector3.Dot(direction.normalized, GetForwardVector()) < 0;
    }
    public float GetVehicleProgress(Transform vehicle)
    {
        Plane _plane = new Plane(GetForwardVector(), transform.position);
        Vector3 planePoint = _plane.ClosestPointOnPlane(vehicle.position);
        float progress = Vector3.Distance(planePoint, vehicle.position);
        
        return progress;
    }

    public Quaternion GetRotation()
    {

        if(setForwardAuto && prev != null && next != null)
        {
            Vector3 dir1 = transform.position - prev.transform.position;
            Vector3 dir2 = transform.position - next.transform.position;

            Vector3 fwd = (dir1 - dir2).normalized;

            return Quaternion.LookRotation(fwd, transform.up);

        }
        else return transform.rotation;
            
    }
    public Vector3 GetForwardVector()
    {
        if (setForwardAuto && prev != null && next != null)
        {
            Vector3 dir1 = transform.position - prev.transform.position;
            Vector3 dir2 = transform.position - next.transform.position;

            Vector3 fwd = (dir1 - dir2).normalized;

            return fwd;

        }
        else return transform.forward;

    }


    //Debug
    protected void DEBUG_CreateDisplayPlane()
    {
        _displayPlane = new Mesh();
        _displayPlane.vertices = 
            new Vector3[4]
            {
                new Vector3(-1,-1,0),
                new Vector3(1,-1,0),
                new Vector3(-1f,1,0),
                new Vector3(1f,1,0)
            };
        _displayPlane.normals =
            new Vector3[4]
            {
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(0,0,1),
                new Vector3(0,0,1)
            };
        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        _displayPlane.triangles = tris;

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (_displayPlane == null)
            DEBUG_CreateDisplayPlane();

        if(next != null)
            Gizmos.DrawLine(transform.position, next.transform.position);

        Gizmos.DrawLine(transform.position, transform.position + GetForwardVector() * 3f);//Draw forward direction

        if(!Application.isPlaying)

        {
            if (isLapFlag)
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawWireMesh(_displayPlane, 0, transform.position, GetRotation(), transform.localScale);
        }

        if (DEBUG_TRANSFORM != null)
        {
            int check = DEBUG_TRANSFORM.checkpointPosition;
            bool current = index == CheckpointManager.CheckpointIndex(check);
            bool next = index == CheckpointManager.CheckpointIndex(check+1);
            if (current || next)
            { 
                Gizmos.color = current ? Color.green : Color.red;
                Gizmos.DrawWireMesh(_displayPlane, 0, transform.position, GetRotation(), transform.localScale);
                //Gizmos.DrawSphere(transform.position, 1f);

                Plane _plane = new Plane(GetForwardVector(), transform.position);
                Vector3 planePoint = _plane.ClosestPointOnPlane(DEBUG_TRANSFORM.transform.position);

                /*
                Gizmos.DrawSphere(planePoint, 1);
                Gizmos.DrawLine(planePoint, DEBUG_TRANSFORM.transform.position);
                */
            }


            

        }

        if (isLapFlag)
        {
            Vector3 pos = transform.position + new Vector3(0, transform.localScale.y+1);
            Gizmos.DrawIcon(pos, "Flag",true);
        }

        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        
    }
}
