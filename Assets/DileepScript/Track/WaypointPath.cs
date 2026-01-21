using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public Transform[] Points { get; private set; }

    void Awake() => Cache();

    public void Cache()
    {
        int n = transform.childCount;
        Points = new Transform[n];
        for (int i = 0; i < n; i++)
            Points[i] = transform.GetChild(i);
    }

    public Vector3 GetPoint(int index)
    {
        if (Points == null || Points.Length == 0) return transform.position;
        int i = Mod(index, Points.Length);
        return Points[i].position;
    }

    public int Count => (Points == null) ? 0 : Points.Length;

    static int Mod(int a, int b) => (a % b + b) % b;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (transform.childCount < 2) return;
        Gizmos.color = Color.white;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform a = transform.GetChild(i);
            Transform b = transform.GetChild((i + 1) % transform.childCount);
            Gizmos.DrawLine(a.position + Vector3.up * 0.2f, b.position + Vector3.up * 0.2f);
        }
    }
#endif
}
