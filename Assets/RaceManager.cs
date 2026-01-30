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

    protected override void Awake()
    {
        base.Awake();
        
        if (RaceSettingsManager.Instance.CurrentMode == RaceMode.Custom)
        {
            trackPrefab.SetActive(false);
            customTrackPrefab.SetActive(true);
        }
        else
        {
            trackPrefab.SetActive(true);
            customTrackPrefab.SetActive(false);
        }
    }

    public Transform[] SpawnRacers()
    {
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
}
