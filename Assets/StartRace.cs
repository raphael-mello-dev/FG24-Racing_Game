using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
public class StartRace : MonoBehaviour
{
    public int playerRacers = 1;
    public int aiRacers = 2;

    public GameObject playerPrefab;
    public GameObject AiPrefab;

    public Vector3 racerStartDistance = new Vector3(4,4, 2);

    public Transform[] SpawnRacers()
    {
        Transform[] list = new Transform[playerRacers + aiRacers];

        for (int i = 0; i < list.Length; i++)
        {
            list[i] = Instantiate(i < playerRacers ? playerPrefab : AiPrefab).transform;
        }

        int index = 0;
        float time = 0;
        foreach (Transform trans in list)
        {
            SetPlayer(trans, time, index);
            index++;   
            if(index%2==1)
                time += racerStartDistance.x;
        }
        return list;
    }


    public SplineContainer roadSpline;

    public void SetPlayer(Transform trans, float time, int index)
    {
        trans.gameObject.SetActive(false);

        float3 position = roadSpline.Spline.EvaluatePosition(time);
        float3 tangent = roadSpline.Spline.EvaluateTangent(time);

        float3 add = tangent * (index % 2 == 0 ? racerStartDistance.y : -racerStartDistance.y);
        trans.position = position + add + new float3(0, racerStartDistance.z,0);
        trans.rotation = Quaternion.LookRotation(Quaternion.Euler(0, 0, 0) * tangent);

        trans.gameObject.SetActive(true);
    }
}
