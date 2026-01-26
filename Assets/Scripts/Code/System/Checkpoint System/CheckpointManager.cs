using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CheckpointManager;
using static UnityEngine.GraphicsBuffer;

//------------------------------------------------------------------------
//
//  This script was created by Milo. If you have questions or problems, ask her. 
//
//------------------------------------------------------------------------


public class CheckpointManager : SceneOnlySingleton<CheckpointManager>
{

    
    public CheckpointNode[] checkpoints;

    public Transform[] DEBUG_VEHICLES;

    public Racer[] vehicles;
    private Dictionary<Transform, Racer> _racerTransformDictonary = new Dictionary<Transform, Racer>();

    public Transform DEBUG_FOCUSED_RACER;

    public TextMeshProUGUI DEBUG_DRAW_RACERS_STATUS;

    public Racer DEBUG_FOCUSED_RACER_INFO;


    protected override void Awake()
    {
        base.Awake();
        FindAllNodes();
        AssignRacers(DEBUG_VEHICLES);
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

        string text = "Racers:";
        foreach (var racer in vehicles)
        {
            text += $"\n{racer.transform.gameObject.name} | POSITION {racer.racePosition} | LAP {racer.lapCount} | CHECKPOINT {racer.checkpointPosition} | PROGRESS {racer.progress}";
        }

        if(DEBUG_DRAW_RACERS_STATUS != null)
            DEBUG_DRAW_RACERS_STATUS.text = text;
    }
    protected void UpdateRacePositions()
    {
        foreach(Racer racer in vehicles)
        {
            if(racer.GetCheckpoint().next.HasVehiclePassed(racer.transform))
            {
                if(racer.GetCheckpoint().next.isLapFlag)
                    racer.lapCount++; //HERE YOU CAN CHECK THE RULES TO SEE IF YOU WIN

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

    [Serializable]
    public class Racer : IComparable<Racer>
    {
        public Transform transform;
        public int index;

        public int racePosition = 0; //The position they are amongst the other racers

        public int lapCount = 0;
        public int checkpointPosition = 0; //Which checkpoint they are on
        public float progress; //How far they have made it from the last checkpoint

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

}
