using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RaceManager : SceneOnlySingleton<RaceManager>
{    
    //Racers
    public int playerRacers = 1;

    public int racerCount { get; private set; } 
    private int maxPlayerCount = 1;

    public GameObject playerPrefab;
    public GameObject AiPrefab;

    public GameObject trackPrefab;
    public GameObject customTrackPrefab;

    public Vector3 racerStartDistance = new Vector3(4,4, 2);

    //Game

    public int lapCount = 1;

    //End of race
    public TextMeshProUGUI endOfRaceText;
    public GameObject endOfRaceCanvas;

    private void Start()
    {
        endOfRaceCanvas.SetActive(false);
    }

    protected override void Init()
    {
        base.Init();

        bool custom = RaceSettingsManager.Instance.CurrentMode == RaceMode.Custom;
        if (trackPrefab != null)
            trackPrefab.SetActive(!custom);
        if (customTrackPrefab != null)
            customTrackPrefab.SetActive(custom);

        roadSpline =
        (custom ? customTrackPrefab : trackPrefab)
        .GetComponent<SplineContainer>();
    }



    public Transform[] SpawnRacers()
    {
        if(!hasInit)
            Init();

        playerRacers = Mathf.Clamp(playerRacers, 0, maxPlayerCount);    
        racerCount = playerRacers + RaceSettingsManager.Instance.CarsOpAmount;

        Transform[] list = new Transform[racerCount];

        for (int i = 0; i < list.Length; i++)
        {
            list[i] = Instantiate(i < playerRacers ? playerPrefab : AiPrefab).transform;
        }

        int index = 0;
        float time = 1;
        foreach (Transform trans in list)
        {
            SetPlayer(trans, time, index);
            index++;   
            if(index%2==0)
                time -= racerStartDistance.x;
        }
        return list;
    }


    public SplineContainer roadSpline;

    public void SetPlayer(Transform trans, float time, int index)
    {
        trans.gameObject.SetActive(false);

        float3 position = roadSpline.Spline.EvaluatePosition(time);
        float3 tangent = roadSpline.Spline.EvaluateTangent(time);

        float3 add = Quaternion.Euler(0, 90, 0)*tangent * (index % 2 == 0 ? racerStartDistance.y : -racerStartDistance.y);
        trans.position = position + add + new float3(0, racerStartDistance.z,0);
        trans.rotation = Quaternion.LookRotation(Quaternion.Euler(0, 0, 0) * tangent);

        trans.gameObject.SetActive(true);
    }


    public void EndRace(CheckpointManager.Racer[] winners)
    {
        endOfRaceCanvas.SetActive(true);
        string text = "PLACEMENTS";
        for (int i = 0; i < winners.Length; i++)
        {
            CheckpointManager.Racer racer = winners[i];
            string name = racer.transform.gameObject.name;
            int position = i + 1;
            string time = TimeSpan.FromSeconds(racer.timeFinishedRace).ToString();
            text += $"\n{position} <pos=70>| {name} \t <pos=350>| {time}";
        }

        endOfRaceText.text = text;
    }
}
