using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class SmoothSplineBoxColliderGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer spline;

    [Header("Collider Shape")]
    [SerializeField] private float width = 12f;
    [SerializeField] private float height = 1f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);

    [Header("Smoothness")]
    [Tooltip("How many colliders per meter of spline length (higher = smoother, more cost)")]
    [SerializeField] private int segmentsPerMeter = 2;

    [Tooltip("Extra length added to each segment (0.1 = 10% overlap). Helps remove bumps.")]
    [Range(0f, 0.5f)]
    [SerializeField] private float lengthOverlap = 0.15f;

    [Tooltip("Minimum segments (safety clamp)")]
    [SerializeField] private int minSegments = 32;

    [Tooltip("Maximum segments (performance clamp)")]
    [SerializeField] private int maxSegments = 2048;

    [Header("Auto")]
    [SerializeField] private bool autoRebuild = true;
    [SerializeField] private Transform collidersRoot;

    private readonly List<BoxCollider> _colliders = new();
    private bool _dirty = true;

    private void OnEnable()
    {
        if (spline == null) spline = GetComponent<SplineContainer>();
        UnityEngine.Splines.Spline.Changed += OnSplineChanged;
        
        // Rebuild collider list from existing children to handle script reloads
        RebuildColliderListFromChildren();
        _dirty = true;
    }

    private void OnDestroy()
    {
        // Clean up generated colliders when component is destroyed
        if (collidersRoot != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(collidersRoot.gameObject);
            else Destroy(collidersRoot.gameObject);
#else
            Destroy(collidersRoot.gameObject);
#endif
        }
        _colliders.Clear();
    }

    private void RebuildColliderListFromChildren()
    {
        _colliders.Clear();
        if (collidersRoot == null)
        {
            var existing = transform.Find("GeneratedSplineColliders");
            if (existing != null) collidersRoot = existing;
        }
        
        if (collidersRoot != null)
        {
            foreach (Transform child in collidersRoot)
            {
                var bc = child.GetComponent<BoxCollider>();
                if (bc != null) _colliders.Add(bc);
            }
        }
    }


    private void OnDisable()
    {
        UnityEngine.Splines.Spline.Changed -= OnSplineChanged;
    }

    private void OnValidate()
    {
        width = Mathf.Max(0.01f, width);
        height = Mathf.Max(0.01f, height);
        segmentsPerMeter = Mathf.Clamp(segmentsPerMeter, 1, 50);
        minSegments = Mathf.Clamp(minSegments, 4, 100000);
        maxSegments = Mathf.Clamp(maxSegments, minSegments, 100000);

        if (autoRebuild) _dirty = true;
    }

    private void Update()
    {
        if (!_dirty) return;
        _dirty = false;
        Rebuild();
    }

    private void OnSplineChanged(UnityEngine.Splines.Spline changed, int knotIndex, SplineModification mod)
    {
        if (!autoRebuild) return;
        if (spline != null && spline.Spline == changed)
            _dirty = true;
    }

    [ContextMenu("Rebuild Colliders")]
    public void Rebuild()
    {
        if (spline == null) spline = GetComponent<SplineContainer>();
        if (spline == null || spline.Spline == null) return;

        EnsureRoot();

        var sp = spline.Spline;
        float length = sp.GetLength();

        int segments = Mathf.CeilToInt(length * segmentsPerMeter);
        segments = Mathf.Clamp(segments, minSegments, maxSegments);

        EnsureColliderCount(segments);

        // Build each collider between t0 and t1, centered in between
        for (int i = 0; i < segments; i++)
        {
            float t0 = i / (float)segments;
            float t1 = (i + 1) / (float)segments;

            sp.Evaluate(t0, out float3 p0, out float3 tan0, out _);
            sp.Evaluate(t1, out float3 p1, out float3 tan1, out _);

            float3 center = (p0 + p1) * 0.5f;

            float3 forward3 = math.normalize(tan0 + tan1);
            if (math.lengthsq(forward3) < 1e-6f) forward3 = new float3(0, 0, 1);

            // Stable frame: keep "up" global to avoid twisting bumps
            Vector3 up = Vector3.up;
            Vector3 forward = new Vector3(forward3.x, forward3.y, forward3.z);
            forward = Vector3.ProjectOnPlane(forward, up).normalized;
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;

            Quaternion rot = Quaternion.LookRotation(forward, up);

            float segLen = math.distance(p0, p1);
            float zLen = Mathf.Max(0.01f, segLen * (1f + lengthOverlap));

            var bc = _colliders[i];
            var tr = bc.transform;

            tr.position = (Vector3)center;
            tr.rotation = rot;
            tr.position += tr.TransformDirection(offset);

            bc.center = Vector3.zero;
            bc.size = new Vector3(width, height, zLen);
        }
    }

    private void EnsureRoot()
    {
        if (collidersRoot != null) return;

        var existing = transform.Find("GeneratedSplineColliders");
        if (existing != null)
        {
            collidersRoot = existing;
            return;
        }

        var go = new GameObject("GeneratedSplineColliders");
        go.transform.SetParent(transform, false);
        collidersRoot = go.transform;
    }

    private void EnsureColliderCount(int count)
    {
        while (_colliders.Count < count)
        {
            int idx = _colliders.Count;
            var go = new GameObject($"Col_{idx:0000}");
            go.transform.SetParent(collidersRoot, false);
            _colliders.Add(go.AddComponent<BoxCollider>());
        }

        for (int i = _colliders.Count - 1; i >= count; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(_colliders[i].gameObject);
            else Destroy(_colliders[i].gameObject);
#else
            Destroy(_colliders[i].gameObject);
#endif
            _colliders.RemoveAt(i);
        }
    }
}
