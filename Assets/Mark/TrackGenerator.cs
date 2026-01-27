using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    public enum PieceType
    {
        Straight,
        Left,
        Right
    }

    [System.Serializable]
    public class PieceOption
    {
        public TrackPiece Prefab;
        [Range(0.01f, 1f)] public float Weight = 1f;
        public PieceType Type = PieceType.Straight;
    }

    [Header("Pieces")]
    public TrackPiece StartPiece;
    public List<PieceOption> Options = new();

    [Header("Generation")]
    [Min(1)] public int PieceCount = 50;
    [Min(1)] public int MaxAttemptsPerPiece = 25;
    [Tooltip("Maximum consecutive turns in the same direction before forcing a different piece type.")]
    [Min(1)] public int MaxConsecutiveTurns = 2;

    [Tooltip("ONLY the layer of your OverlapBounds triggers (e.g. TrackBounds).")]
    public LayerMask OverlapMask;

    [Range(0.5f, 1f)]
    public float BoundsShrink = 0.9f;

    [Header("Output")]
    public Transform TrackParent;

    [Header("Checkpoint / AI Nav")]
    public bool GenerateNodes = true;

    [Header("Debug")]
    public bool DrawLastCheckedBox = true;
    public bool LogStopReason = true;

    private readonly List<TrackPiece> _spawned = new();
    private readonly List<PieceType> _recentPieceTypes = new();

    // Debug info for Gizmos
    private BoxCollider _lastCheckedCollider;
    private bool _lastCheckWasOverlapping;

    private void Start()
    {
        Generate();

        if (GenerateNodes)
            GenerateCheckpointNodes();

    }

    [ContextMenu("Generate Track")]
    public void Generate()
    {
        if (StartPiece == null)
        {
            Debug.LogError("TrackGenerator: StartPiece is not assigned.");
            return;
        }

        if (Options == null || Options.Count == 0)
        {
            Debug.LogError("TrackGenerator: Options list is empty (add Straight/Left/Right).");
            return;
        }

        ClearOld();

        if (TrackParent == null)
            TrackParent = new GameObject("Track").transform;

        // 1) Spawn start
        TrackPiece first = Instantiate(StartPiece, Vector3.zero, Quaternion.identity, TrackParent);
        _spawned.Add(first);

        Transform attachSocket = first.EndSocket;
        if (attachSocket == null)
        {
            Debug.LogError("StartPiece has no EndSocket assigned.");
            return;
        }

        // 2) Spawn chain
        for (int i = 0; i < PieceCount; i++)
        {
            bool placed = false;

            for (int attempt = 0; attempt < MaxAttemptsPerPiece; attempt++)
            {
                TrackPiece prefab = PickWeighted();
                TrackPiece candidate = Instantiate(prefab, Vector3.zero, Quaternion.identity, TrackParent);

                if (candidate.StartSocket == null || candidate.EndSocket == null)
                {
                    Debug.LogError($"{candidate.name} is missing StartSocket or EndSocket reference in TrackPiece.");
                    Destroy(candidate.gameObject);
                    continue;
                }

                // Snap it
                SnapToSocket(candidate, attachSocket);

                // Disable candidate temporarily so it won't accidentally be counted weirdly
                candidate.gameObject.SetActive(false);
                bool overlapping = IsOverlapping(candidate);
                candidate.gameObject.SetActive(true);

                if (!overlapping)
                {
                    _spawned.Add(candidate);
                    attachSocket = candidate.EndSocket;
                    
                    // Track the piece type that was placed
                    PieceType placedType = GetPieceType(prefab);
                    _recentPieceTypes.Add(placedType);
                    
                    // Keep only the last few types for checking consecutive turns
                    if (_recentPieceTypes.Count > MaxConsecutiveTurns + 1)
                        _recentPieceTypes.RemoveAt(0);
                    
                    placed = true;
                    break;
                }

                Destroy(candidate.gameObject);
            }

            if (!placed)
            {
                if (LogStopReason)
                    Debug.LogWarning($"Stopped early at piece {i}: no valid placement found.");
                break;
            }
        }
    }

    private TrackPiece PickWeighted()
    {
        // Check for consecutive turns in the same direction
        PieceType? restrictedType = GetRestrictedPieceType();
        
        // Build filtered list with adjusted weights
        List<(PieceOption option, float weight)> validOptions = new();
        float total = 0f;
        
        foreach (var o in Options)
        {
            if (o.Prefab == null) continue;
            
            // If this piece type is restricted (too many consecutive), reduce its weight significantly
            float weight = o.Weight;
            if (restrictedType.HasValue && o.Type == restrictedType.Value)
            {
                // Heavily penalize this type, but don't completely exclude it
                weight *= 0.01f;
            }
            
            weight = Mathf.Max(0.0001f, weight);
            validOptions.Add((o, weight));
            total += weight;
        }
        
        if (validOptions.Count == 0)
            return Options[0].Prefab;
        
        float r = Random.value * total;
        float acc = 0f;

        foreach (var (option, weight) in validOptions)
        {
            acc += weight;
            if (r <= acc)
                return option.Prefab;
        }

        return validOptions[0].option.Prefab;
    }
    
    private PieceType? GetRestrictedPieceType()
    {
        if (_recentPieceTypes.Count < MaxConsecutiveTurns)
            return null;
        
        // Check if the last N pieces are all the same turn type
        PieceType lastType = _recentPieceTypes[_recentPieceTypes.Count - 1];
        
        // Only restrict turn types (Left/Right), not straights
        if (lastType == PieceType.Straight)
            return null;
        
        // Count how many consecutive pieces of this type we have
        int consecutiveCount = 0;
        for (int i = _recentPieceTypes.Count - 1; i >= 0; i--)
        {
            if (_recentPieceTypes[i] == lastType)
                consecutiveCount++;
            else
                break;
        }
        
        // If we've hit the max consecutive turns, restrict this type
        if (consecutiveCount >= MaxConsecutiveTurns)
            return lastType;
        
        return null;
    }
    
    private PieceType GetPieceType(TrackPiece piece)
    {
        if (piece == null) return PieceType.Straight;
        
        // Try to find the piece type from the Options list by matching the prefab
        // Unity instantiated objects have "(Clone)" appended, so we compare the base prefab
        foreach (var option in Options)
        {
            if (option.Prefab != null)
            {
                // Check if this piece was instantiated from this prefab
                // We can do this by comparing names (removing "(Clone)" suffix)
                string pieceName = piece.name.Replace("(Clone)", "").Trim();
                string prefabName = option.Prefab.name.Trim();
                
                if (pieceName == prefabName)
                {
                    return option.Type;
                }
            }
        }
        
        // Fallback: try to determine from name
        string name = piece.name.ToLower();
        if (name.Contains("left"))
            return PieceType.Left;
        if (name.Contains("right"))
            return PieceType.Right;
        
        return PieceType.Straight;
    }

    private void SnapToSocket(TrackPiece piece, Transform targetSocket)
    {
        Transform startSocket = piece.StartSocket;

        Quaternion rotDelta = Quaternion.FromToRotation(startSocket.forward, targetSocket.forward);
        piece.transform.rotation = rotDelta * piece.transform.rotation;

        piece.transform.rotation = Quaternion.Euler(0f, piece.transform.rotation.eulerAngles.y, 0f);


        Vector3 offset = targetSocket.position - startSocket.position;
        piece.transform.position += offset;
    }

    private bool IsOverlapping(TrackPiece piece)
    {
        BoxCollider box = piece.OverlapBounds;
        _lastCheckedCollider = box;

        if (box == null)
        {
            _lastCheckWasOverlapping = false;
            return false;
        }

        Vector3 worldCenter = box.transform.TransformPoint(box.center);
        Vector3 worldHalfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale) * BoundsShrink;
        Quaternion worldRotation = box.transform.rotation;

        Collider[] hits = Physics.OverlapBox(
            worldCenter,
            worldHalfExtents,
            worldRotation,
            OverlapMask,
            QueryTriggerInteraction.Collide
        );

        foreach (var h in hits)
        {
            if (h == null) continue;

            // Ignore collisions with itself
            if (h.transform.IsChildOf(piece.transform)) continue;

            _lastCheckWasOverlapping = true;
            return true;
        }

        _lastCheckWasOverlapping = false;
        return false;
    }

    private void ClearOld()
    {
        if (TrackParent == null) return;

        for (int i = TrackParent.childCount - 1; i >= 0; i--)
            Destroy(TrackParent.GetChild(i).gameObject);

        _spawned.Clear();
        _recentPieceTypes.Clear();
        _lastCheckedCollider = null;
        _lastCheckWasOverlapping = false;
    }

    [ContextMenu("Generate Checkpoints")]
    private void GenerateCheckpointNodes()
    {
        Transform[] trackPieces = new Transform[_spawned.Count];
        for (int i = 0; i < trackPieces.Length; i++)
        {
            trackPieces[i] = _spawned[i].transform;
        }
        if(CheckpointManager.instance != null)
            CheckpointManager.instance.GenerateCheckpoints(trackPieces);
    }


    private void OnDrawGizmos()
    {
        if (!DrawLastCheckedBox) return;
        if (_lastCheckedCollider == null) return;

        DrawDebugBox(_lastCheckedCollider, _lastCheckWasOverlapping);
    }

    private void DrawDebugBox(BoxCollider box, bool wasOverlapping)
    {
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, box.transform.lossyScale) * BoundsShrink;

        Gizmos.color = wasOverlapping ? Color.red : Color.magenta;

        Matrix4x4 m = Matrix4x4.TRS(center, box.transform.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        Gizmos.matrix = Matrix4x4.identity;
    }
}