using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
public class StartPoint : MonoBehaviour
{
    public SplineContainer roadSpline;

    public void SetPlayers()
    {
        var knots = roadSpline.Spline.Knots;

        transform.position = knots.First().Position;
        transform.rotation = Quaternion.LookRotation(Quaternion.Euler(0, -90, 0) * knots.First().TangentOut);
    }
}
