using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using static CheckpointManager.Spline;

//------------------------------------------------------------------------
//
//  This script was created by Milo. If you have questions or problems, ask her. 
//
//------------------------------------------------------------------------


public class CheckpointManager : SceneOnlySingleton<CheckpointManager>
{

    public GameObject checkpointPrefab;
    public Transform checkpointParent;
    public CheckpointNode[] checkpoints;

    public Transform[] DEBUG_VEHICLES;

    public Racer[] vehicles;
    public List<Racer> winners = new();
    private Dictionary<Transform, Racer> _racerTransformDictonary = new Dictionary<Transform, Racer>();

    public Transform DEBUG_FOCUSED_RACER;

    public TextMeshProUGUI DEBUG_DRAW_RACERS_STATUS;

    public Racer DEBUG_FOCUSED_RACER_INFO;

    


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Init()
    {
        if (hasInit)
            return;
        base.Init();

        _ = SpawnRacers();        
    }

    public async Awaitable SpawnRacers()
    {
        ProceduralTrackGenerator gen = 
            FindFirstObjectByType<ProceduralTrackGenerator>();


        while(!gen.hasGenerated)
        {
            await Awaitable.EndOfFrameAsync();
        }

        if(checkpoints.Length == 0)
            FindAllNodes(); 
        AssignRacers(FindFirstObjectByType<RaceManager>().SpawnRacers());

        if (DEBUG_FOCUSED_RACER == null)
            DEBUG_FOCUSED_RACER = FindFirstObjectByType<CarController>().transform;
        DEBUG_FOCUSED_RACER_INFO = _racerTransformDictonary[DEBUG_FOCUSED_RACER];
    }


    [ContextMenu("Find All Nodes")]
    public void FindAllNodes()
    {
        CheckpointNode[] nodes = 
            FindObjectsByType<CheckpointNode>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        if (!Application.isPlaying)
        {
            foreach (var node in nodes)
            {
                Undo.RecordObject(node.gameObject, "pre");
            }
        }

        

        Array.Sort(nodes, (a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

        AssignCheckpoints(nodes);

        if(!Application.isPlaying)
        {
            foreach(var node in  nodes)
            {
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(node.gameObject);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public void GenerateCheckpoints(Transform[] transforms, int interpolation = 2)
    {
        Vector3[] positions = new Vector3[transforms.Length];
        for(int i = 0; i < transforms.Length; i++)
        {
            positions[i] = transforms[i].position;
        }
        GenerateCheckpoints(positions, interpolation);

    }
    public void GenerateCheckpoints(Vector3[] positions, int interpolation = 2)
    {
        foreach (var c in checkpoints)
        {
            Destroy(c.gameObject);
        }

            Spline spline = new Spline(positions, InterpolationType.Catmull);
        List<CheckpointNode> checkpointList = new List<CheckpointNode>();
        for (int i = 0; i < positions.Length; i++)
        {
            for (int j = 0; j < interpolation; j++)
            {
                Vector3 position = spline.GetPointAtIndex(i + (float)j / (float)interpolation);
                checkpointList.Add(MakeCheckpoint(position + Vector3.up * 10));
            }
        }
        for(int i = 0; i < checkpointList.Count; i++)
        {
            checkpointList[i].gameObject.name = "Checkpoint - " + i;
        }

        checkpointList.First().isLapFlag = true;

        AssignCheckpoints(checkpointList.ToArray());
    }

    private CheckpointNode MakeCheckpoint(Vector3 position)
    {
        var g = Instantiate(checkpointPrefab, checkpointParent);
        g.transform.position = position; 
        return g.GetComponent<CheckpointNode>();
    }

    public void AssignCheckpoints(CheckpointNode[] nodes, bool autoSetNeighbors = true)
    {
        this.checkpoints = (CheckpointNode[])nodes.Clone();

        if (autoSetNeighbors)
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {

                int prev = (int)Mathf.Repeat(i-1, checkpoints.Length);
                int next = (int)Mathf.Repeat(i+1, checkpoints.Length);    

                checkpoints[i].prev = checkpoints[prev];
                checkpoints[i].next = checkpoints[next];
                checkpoints[i].index = i;
            }
        }
    }
    public void AssignRacers(Transform[] transforms)
    {
        vehicles = new Racer[transforms.Length];
        _racerTransformDictonary = new Dictionary<Transform, Racer>();

        for(int i = 0;i < transforms.Length;i++)
        {
            vehicles[i] = new Racer(transforms[i], i);
            _racerTransformDictonary.Add(transforms[i], vehicles[i]);
        }
    }


    private void Update()
    {

        if(DEBUG_FOCUSED_RACER != null)
            foreach(var point in checkpoints)
            {
                point.DEBUG_TRANSFORM = _racerTransformDictonary[DEBUG_FOCUSED_RACER];
            }

        

        //Possibly add a timer so this isn't checked every frame.
        UpdateRacePositions();
    }
    protected void UpdateRacePositions()
    {
        foreach(Racer racer in vehicles)
        {
            if(racer.GetCheckpoint().next.HasVehiclePassed(racer.transform))
            {
                if (racer.GetCheckpoint().next.isLapFlag)
                {
                    racer.lapCount++;
                    if(racer.timeFinishedRace == -1 && //Make sure they havent finished already
                        racer.lapCount <= RaceManager.instance.lapCount)
                    {
                        winners.Add(racer);
                        racer.timeFinishedRace = Time.timeSinceLevelLoad;
                        if (winners.Count == vehicles.Length)
                            EndRace();
                    }
                }

                racer.checkpointPosition = racer.GetCheckpoint().next.index;
            }
            racer.progress = racer.GetCheckpoint().next.GetVehicleProgress(racer.transform);
        }
        //Sort by progress
        Array.Sort(vehicles);

        for(int i = 0; i < vehicles.Length; i++)
        {
            vehicles[i].racePosition = i + 1;
        }
    }

    private void EndRace()
    {
        RaceManager.instance.EndRace(winners.ToArray());
    }

    [Serializable]
    public class Racer : IComparable<Racer>
    {
        public Transform transform;
        public int index;

        public int racePosition = 0; //The position they are amongst the other racers

        public int lapCount = 0;
        public int checkpointPosition = 0; //Which checkpoint they are on
        public float progress; //How far they have made it from the last checkpoint

        public float timeFinishedRace = -1;
        public Racer(Transform transform, int index)
        {
            this.index = index;
            this.transform = transform; 
        }

        public CheckpointNode GetCheckpoint()
        {
            return CheckpointManager.instance.checkpoints[checkpointPosition];
        }

        public int CompareTo(Racer other)
        {
            // by highest lap first
            int result = -this.lapCount.CompareTo(other.lapCount);
            if(result == 0)
            { 
                //sort by highest checkpoint
                result = -this.checkpointPosition.CompareTo(other.checkpointPosition);
                if (result == 0) // units were equal
                {
                    // sort by lowest distance
                    result = this.progress.CompareTo(other.progress);
                }
            }
            return result;
        }

        public override string ToString()
        {
            return $"Position: {racePosition} / Lap {lapCount}";
        }
    }

    



    //Getter Functions
    public static int CheckpointIndex(int i)
    {
        if (instance == null)
            return -1;
        return (int)Mathf.Repeat(i + 1, instance.checkpoints.Length);
    }

    public static Racer GetRacerInfo(Transform self)
    {
        return instance._racerTransformDictonary[self];
    }




    public struct Spline
    {
        Transform[] _tPoints;
        Vector3[] _points;
        DistanceMap<Vector3> _map;
        DistanceMap<Transform> _tMap;

        bool useTransform;

        InterpolationType _type;

        public Spline(Vector3[] points, InterpolationType interpolationType)
        {
            _points = points;
            _tPoints = null;
            _map = null;
            _tMap = null;

            _type = interpolationType;

            useTransform = false;
            UpdateVectorMap();
        }
        public Spline(Transform[] points, InterpolationType interpolationType)
        {
            _points = null;
            _tPoints = points;
            _map = null;
            _tMap = null;

            _type = interpolationType;

            useTransform = true;
            UpdateTransformMap();
        }

        public void UpdateVectorMap()
        {
            float[] distance = new float[_points.Length-1];

            for (int i = 0; i < _points.Length; i++)
            {
                if (i > 0)
                {
                    distance[i - 1] = Vector3.Distance(_points[i - 1], _points[i]);
                }
            }
            _map = new DistanceMap<Vector3>(_points, distance);
        }
        public void UpdateTransformMap()
        {
            float[] distance = new float[_tPoints.Length-1];

            for (int i = 0; i < _tPoints.Length; i++)
            {
                if (i > 0)
                {
                    distance[i - 1] = Vector3.Distance(_tPoints[i - 1].position, _tPoints[i].position);
                }
            }
            _tMap = new DistanceMap<Transform>(_tPoints, distance);
        }

        private Vector3 GetPoint(int index)
        {
            index = Mathf.Clamp(index, 0, GetLength() - 1);
            return useTransform ? _tPoints[index].position : _points[index];
        }
        private int GetLength()
        {
            return useTransform ? _tPoints.Length : _points.Length;
        }
        private float GetCumulative(int i)
        {
            return useTransform ? _tMap.GetCumulative(i) : _map.GetCumulative(i);
        }

        public Vector3 GetPointAtDistance(float d)
        {
            float total = GetCumulative(GetLength() - 1);
            float t = d / total;
            return GetInterpolation(t);
        }
        public Vector3 GetPointAtIndex(float index)
        {
            float t = index / (float)GetLength();
            return GetInterpolation(t);
        }



        

        public enum InterpolationType
        {
            Linear,
            Brazier,
            Catmull
        }

        private Vector3 GetInterpolation(float t)
        {
            return _type switch
            {
                InterpolationType.Linear => CatmullInterpolation(t),
                InterpolationType.Brazier => BrazierInterpolation(t),
                InterpolationType.Catmull => CatmullInterpolation(t)
            };
        }


        private Vector3 CatmullInterpolation(float t)
        {
            int segments = GetLength() - 1;
            float scaledT = t * segments;

            int i = Mathf.FloorToInt(scaledT);
            float localT = scaledT - i;

            // Clamp last segment
            if (i >= segments)
            {
                i = segments - 1;
                localT = 1f;
            }

            return CatmullRom(
                GetPoint(i-1),
                GetPoint(i),
                GetPoint(i+1),
                GetPoint(i+2),
                localT
            );
        }
        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }


        private Vector3 BrazierInterpolation(float t)
        {
            Vector3 point = Vector3.zero;
            int n = GetLength() - 1;
            for (int i = 0; i < GetLength(); i++)
            {
                float binomial = Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * BinomialCoefficient(n, i);
                point += binomial * GetPoint(i);
            }
            return point;
        }
        private int BinomialCoefficient(int n, int k)
        {
            int result = 1;
            for (int i = 1; i <= k; i++)
                result = result * (n - (k - i)) / i;
            return result;
        }
        public class DistanceMap<T>
        {
            private readonly T[] _objects;
            private readonly float[] _cumulative; // prefix sums

            public DistanceMap(T[] objects, float[] distances)
            {
                if (objects == null || distances == null)
                    throw new ArgumentNullException();

                if (distances.Length != objects.Length - 1)
                    throw new ArgumentException("Distances length must be one less than objects length.");

                _objects = objects;
                _cumulative = new float[objects.Length];
                _cumulative[0] = 0;

                for (int i = 0; i < distances.Length; i++)
                    _cumulative[i + 1] = _cumulative[i] + distances[i];
            }

            public T GetObjectAt(float x)
            {
                return _objects[GetIndexAt(x)];
            }


            public int GetIndexAt(float x)
            {
                if (x <= 0)
                    return 0;

                if (x >= _cumulative[_cumulative.Length - 1])
                    return _objects.Length - 1;


                int index = Array.BinarySearch(_cumulative, x);
                if (index < 0)
                    index = ~index - 1;

                if (index >= _objects.Length)
                    index = _objects.Length - 1;

                return index;
            }

            public float GetClosestDistanceAt(float x)
            {
                return _cumulative[GetIndexAt(x)];
            }
            public float GetCumulative(int index)
            {
                index = Mathf.Clamp(index, 0, _cumulative.Length - 1);
                return _cumulative[index];
            }
            public float GetLerpToNext(float x)
            {
                if (x <= 0)
                    return 0;

                if (x >= _cumulative[_cumulative.Length - 1])
                    return 1;

                int index = GetIndexAt(x);
                float a = x - _cumulative[index];
                float b = _cumulative[index + 1] - _cumulative[index];

                return a / b;
            }

        }
    }

}
