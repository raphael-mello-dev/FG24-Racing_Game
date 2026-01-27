using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LoftRoadBehaviour : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField]
    private SplineContainer spline;

    [SerializeField]
    private int segmentsPerMeter = 3;

    [Header("Road Settings")]
    [SerializeField]
    private float width = 4f;

    [SerializeField]
    private Mesh mesh;

    [SerializeField]
    private float textureScale = 250f;

    [Header("Skirt Settings")]
    [SerializeField]
    private float skirtWidthRatio = 0.1f;

    [SerializeField]
    private float skirtHeight = -0.1f;

    private MeshFilter meshFilter;
    private bool needsRebuild = true;

    // Cached lists to avoid GC allocations
    private readonly System.Collections.Generic.List<Vector3> vertices = new();
    private readonly System.Collections.Generic.List<Vector2> uvs = new();
    private readonly System.Collections.Generic.List<int> triangles = new();

    public SplineContainer Spline
    {
        get => spline;
        set
        {
            spline = value;
            needsRebuild = true;
        }
    }

    public int SegmentsPerMeter
    {
        get => segmentsPerMeter;
        set
        {
            segmentsPerMeter = value;
            needsRebuild = true;
        }
    }

    public float Width
    {
        get => width;
        set
        {
            width = value;
            needsRebuild = true;
        }
    }

    public Mesh Mesh
    {
        get => mesh;
        set => mesh = value;
    }

    public float TextureScale
    {
        get => textureScale;
        set
        {
            textureScale = value;
            needsRebuild = true;
        }
    }

    public float SkirtWidthRatio
    {
        get => skirtWidthRatio;
        set
        {
            skirtWidthRatio = value;
            needsRebuild = true;
        }
    }

    public float SkirtHeight
    {
        get => skirtHeight;
        set
        {
            skirtHeight = value;
            needsRebuild = true;
        }
    }

    private void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        UnityEngine.Splines.Spline.Changed += OnSplineChanged;
        needsRebuild = true;
    }

    private void OnDisable()
    {
        UnityEngine.Splines.Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(UnityEngine.Splines.Spline splineChanged, int index, SplineModification modification)
    {
        if (spline != null && spline.Spline == splineChanged)
        {
            needsRebuild = true;
        }
    }

    private void Update()
    {
        if (needsRebuild)
        {
            GenerateRoadMesh();
            needsRebuild = false;
        }
    }

    public void GenerateRoadMesh()
    {
        if (spline == null || spline.Spline == null)
            return;

        float splineLength = spline.Spline.GetLength();
        int totalSegments = Mathf.Max(2, Mathf.CeilToInt(splineLength * segmentsPerMeter));

        vertices.Clear();
        uvs.Clear();
        triangles.Clear();

        float currentDistance = 0f;

        for (int i = 0; i < totalSegments; i++)
        {
            float t = i / (float)(totalSegments - 1);
            
            spline.Spline.Evaluate(t, out float3 position, out float3 tangent, out float3 up);
            
            float3 right = math.normalize(math.cross(up, tangent));
            
            float halfWidth = width * 0.5f;
            
            float skirtOffset = halfWidth * skirtWidthRatio;

            Vector3 leftSkirt = (Vector3)position + (Vector3)right * (-halfWidth - skirtOffset) + Vector3.up * skirtHeight;
            Vector3 leftEdge = (Vector3)position + (Vector3)right * -halfWidth;
            Vector3 rightEdge = (Vector3)position + (Vector3)right * halfWidth;
            Vector3 rightSkirt = (Vector3)position + (Vector3)right * (halfWidth + skirtOffset) + Vector3.up * skirtHeight;

            vertices.Add(leftSkirt);
            vertices.Add(leftEdge);
            vertices.Add(rightEdge);
            vertices.Add(rightSkirt);

            float uvY = currentDistance / textureScale;
            uvs.Add(new Vector2(0f, uvY));
            uvs.Add(new Vector2(0.25f, uvY));
            uvs.Add(new Vector2(0.75f, uvY));
            uvs.Add(new Vector2(1f, uvY));

            if (i < totalSegments - 1)
            {
                int baseIndex = i * 4;
                
                triangles.Add(baseIndex + 0);
                triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 5);
                
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 5);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 5);
                triangles.Add(baseIndex + 6);
                
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 6);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 6);
                triangles.Add(baseIndex + 7);
            }

            if (i < totalSegments - 1)
            {
                float nextT = (i + 1) / (float)(totalSegments - 1);
                spline.Spline.Evaluate(nextT, out float3 nextPos, out _, out _);
                currentDistance += math.distance(position, nextPos);
            }
        }

        if (mesh == null || !mesh.name.Equals("Road Mesh"))
        {
            mesh = new Mesh();
            mesh.name = "Road Mesh";
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (meshFilter != null)
        {
            meshFilter.sharedMesh = mesh;
        }
    }

    private void OnValidate()
    {
        needsRebuild = true;
    }
}
