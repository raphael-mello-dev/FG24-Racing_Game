using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(SplineContainer), typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralTrackGenerator : MonoBehaviour
{
    [Header("Track Shape")]
    [SerializeField, Range(8, 25), Tooltip("Number of corners/sections")]
    private int cornerCount = 15;

    [SerializeField, Tooltip("Overall track size")]
    private float trackSize = 150f;

    [SerializeField, Range(0f, 1f), Tooltip("Track shape randomness")]
    private float shapeRandomness = 0.5f;

    [SerializeField, Tooltip("Seed (0 = random each time)")]
    private int seed = 0;

    [Header("Circuit Features")]
    [SerializeField, Range(0f, 1f), Tooltip("Long straight frequency")]
    private float straights = 1f;

    [SerializeField, Range(0f, 1f), Tooltip("Hairpin turn frequency")]
    private float hairpins = 0f;

    [SerializeField, Range(0f, 1f), Tooltip("Chicane (S-curve) frequency")]
    private float chicanes = 0f;

    [SerializeField, Range(0f, 1f), Tooltip("How tight corners can be")]
    private float cornerTightness = 0f;

    [Header("Road Settings")]
    [SerializeField]
    private float width = 20f;

    [SerializeField]
    private int resolution = 5;

    [Header("Collision")]
    [SerializeField]
    private bool generateCollider = true;

    [Header("UV Settings")]
    [SerializeField]
    private float textureScale = 10f;

    [Header("Auto Generation")]
    [SerializeField]
    private bool autoGenerateOnStart = true;

    [SerializeField]
    private bool autoRegenerate = true;

    [SerializeField]
    private bool autoGenerateCheckpoints = true;

    private SplineContainer splineContainer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;
    private bool isInitialized;
    private System.Random rng;

    [NonSerialized] public bool hasGenerated = false;

    public float Width
    {
        get => width;
        set { width = value; RegenerateMesh(); }
    }

    private void Awake() => Initialize();
    private void Start()
    {
        if (autoGenerateOnStart || meshFilter.sharedMesh == null)
            Generate();
        else
            RegenerateMesh();

        hasGenerated = true;
    }
    private void OnEnable() => Initialize();

    private void Initialize()
    {
        if (isInitialized) return;
        splineContainer = GetComponent<SplineContainer>();
        meshFilter = GetComponent<MeshFilter>();
        if (generateCollider)
        {
            meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        isInitialized = true;
    }

    private void Reset()
    {
        Initialize();
        Generate();
    }

    private void OnValidate()
    {
        cornerCount = Mathf.Clamp(cornerCount, 8, 25);
        resolution = Mathf.Max(1, resolution);
        width = Mathf.Max(0.1f, width);
        trackSize = Mathf.Max(20f, trackSize);

#if UNITY_EDITOR
        if (autoRegenerate && isInitialized && splineContainer != null)
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) Generate();
            };
        }
#endif
    }

    [ContextMenu("Generate New Track")]
    public void Generate()
    {
        if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        int actualSeed = seed == 0 ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : seed;
        rng = new System.Random(actualSeed);
        
        Debug.Log($"[ProceduralTrackGenerator] Generated track with seed: {actualSeed}");

        GenerateCircuit();
        RegenerateMesh();
    }

    [ContextMenu("Regenerate Mesh Only")]
    public void RegenerateMesh()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;
        GenerateRoadMesh();
        UpdateCollider();

        if (Application.isPlaying == true && autoGenerateCheckpoints)
        {
            var knots = splineContainer.Spline.Knots;
            List<Vector3> knotPositions = new();
            foreach(var knot in knots)
            {
                knotPositions.Add(knot.Position);
            }
            CheckpointManager.instance.GenerateCheckpoints(knotPositions.ToArray());
        }
    }

    private float NextFloat() => (float)rng.NextDouble();
    private float NextFloat(float min, float max) => min + (float)rng.NextDouble() * (max - min);

    private void GenerateCircuit()
    {
        Spline spline = splineContainer.Spline;
        spline.Clear();

        List<float3> points = GenerateTrackPoints();

        foreach (float3 point in points)
        {
            BezierKnot knot = new BezierKnot(point);
            spline.Add(knot, TangentMode.AutoSmooth);
        }

        spline.Closed = true;
    }

    private List<float3> GenerateTrackPoints()
    {
        List<float3> points = new List<float3>();
        
        // Generate a random convex hull-ish shape first
        List<float2> baseShape = GenerateRandomBaseShape();
        
        // Now add features (straights, hairpins, chicanes) along this shape
        for (int i = 0; i < baseShape.Count; i++)
        {
            float2 current = baseShape[i];
            float2 next = baseShape[(i + 1) % baseShape.Count];
            float2 prev = baseShape[(i - 1 + baseShape.Count) % baseShape.Count];
            
            float roll = NextFloat();
            float totalChance = straights + hairpins + chicanes;
            
            if (roll < straights / Mathf.Max(0.01f, totalChance) * 0.5f)
            {
                // Straight section - just add the point
                points.Add(new float3(current.x, 0, current.y));
            }
            else if (roll < (straights + hairpins) / Mathf.Max(0.01f, totalChance) * 0.5f)
            {
                // Hairpin - create a tight turn
                float2 toNext = math.normalize(next - current);
                float2 toPrev = math.normalize(prev - current);
                float2 inward = math.normalize(toNext + toPrev);
                
                float depth = trackSize * NextFloat(0.1f, 0.25f) * cornerTightness;
                
                // Entry point
                points.Add(new float3(current.x, 0, current.y));
                
                // Hairpin apex
                float2 apex = current + inward * depth;
                points.Add(new float3(apex.x, 0, apex.y));
            }
            else if (roll < (straights + hairpins + chicanes) / Mathf.Max(0.01f, totalChance) * 0.5f)
            {
                // Chicane - S-curve
                float2 dir = math.normalize(next - current);
                float2 perp = new float2(-dir.y, dir.x);
                
                float offset = trackSize * NextFloat(0.05f, 0.15f);
                float sign = NextFloat() > 0.5f ? 1f : -1f;
                
                // First kink
                float2 p1 = current + dir * 0.3f + perp * offset * sign;
                points.Add(new float3(p1.x, 0, p1.y));
                
                // Second kink (opposite direction)
                float2 p2 = current + dir * 0.7f - perp * offset * sign;
                points.Add(new float3(p2.x, 0, p2.y));
            }
            else
            {
                // Regular corner with random tightness
                float2 toCenter = -math.normalize(current);
                float randomOffset = NextFloat(-1f, 1f) * shapeRandomness * trackSize * 0.15f;
                
                float2 adjusted = current + toCenter * randomOffset;
                points.Add(new float3(adjusted.x, 0, adjusted.y));
                
                // Sometimes add an extra point for variety
                if (NextFloat() < 0.3f)
                {
                    float2 midpoint = (current + next) * 0.5f;
                    float2 midPerp = new float2(-(next.y - current.y), next.x - current.x);
                    midPerp = math.normalize(midPerp);
                    
                    float bulge = NextFloat(-1f, 1f) * trackSize * 0.1f * shapeRandomness;
                    float2 extraPoint = midpoint + midPerp * bulge;
                    points.Add(new float3(extraPoint.x, 0, extraPoint.y));
                }
            }
        }

        return points;
    }

    private List<float2> GenerateRandomBaseShape()
    {
        List<float2> points = new List<float2>();
        
        // Method: Generate random angles, sort them, then place points at varying radii
        List<float> angles = new List<float>();
        
        for (int i = 0; i < cornerCount; i++)
        {
            // Random angle with some structure
            float baseAngle = (i / (float)cornerCount) * 360f;
            float randomOffset = NextFloat(-20f, 20f) * shapeRandomness;
            angles.Add(baseAngle + randomOffset);
        }
        
        // Sort to ensure we go around clockwise/counter-clockwise
        angles.Sort();
        
        // Generate points at these angles with random radii
        float baseRadius = trackSize * 0.5f;
        
        for (int i = 0; i < angles.Count; i++)
        {
            float angle = angles[i] * Mathf.Deg2Rad;
            
            // Vary radius significantly for more interesting shapes
            float radiusVariation = NextFloat(0.5f, 1.5f);
            // Add some correlation with neighbors for smoother shapes
            if (i > 0)
            {
                float prevRadius = math.length(points[i - 1]) / baseRadius;
                radiusVariation = Mathf.Lerp(radiusVariation, prevRadius, 0.3f);
            }
            
            float radius = baseRadius * radiusVariation;
            
            // Add elongation in a random direction for non-circular tracks
            float elongationAngle = NextFloat(0f, Mathf.PI * 2f);
            float elongationAmount = 1f + shapeRandomness * 0.5f * Mathf.Cos(angle - elongationAngle);
            radius *= elongationAmount;
            
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            
            points.Add(new float2(x, z));
        }
        
        // Add some random displacement to break up regularity
        for (int i = 0; i < points.Count; i++)
        {
            float2 p = points[i];
            float2 randomDisplacement = new float2(
                NextFloat(-1f, 1f),
                NextFloat(-1f, 1f)
            ) * trackSize * 0.1f * shapeRandomness;
            
            points[i] = p + randomDisplacement;
        }
        
        return points;
    }

    private void GenerateRoadMesh()
    {
        Spline spline = splineContainer.Spline;
        float splineLength = spline.GetLength();
        int totalSegments = Mathf.Max(4, Mathf.CeilToInt(splineLength * resolution));

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        float halfWidth = width * 0.5f;
        float accumulatedDistance = 0f;

        for (int i = 0; i <= totalSegments; i++)
        {
            float t = i / (float)totalSegments;
            spline.Evaluate(t, out float3 position, out float3 tangent, out float3 up);

            float3 right = math.normalize(math.cross(up, tangent));

            vertices.Add((Vector3)position - (Vector3)right * halfWidth);
            vertices.Add((Vector3)position + (Vector3)right * halfWidth);

            float uvY = accumulatedDistance / textureScale;
            uvs.Add(new Vector2(0f, uvY));
            uvs.Add(new Vector2(1f, uvY));

            if (i < totalSegments)
            {
                float nextT = (i + 1) / (float)totalSegments;
                spline.Evaluate(nextT, out float3 nextPos, out _, out _);
                accumulatedDistance += math.distance(position, nextPos);
            }
        }

        for (int i = 0; i < totalSegments; i++)
        {
            int baseIndex = i * 2;
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
        }

        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Procedural Track";
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    private void UpdateCollider()
    {
        if (!generateCollider || mesh == null) return;

        if (meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, trackSize * 0.5f);
    }
}
