using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Un solo SplineContainer con varias splines (camino principal + ramas).
/// Las ramas deben empezar/terminar exactamente sobre un punto de otra spline.
/// </summary>
[RequireComponent(typeof(SplineContainer))]
public class TrackRail : MonoBehaviour
{
    [Tooltip("Distancia máxima (metros) para considerar que dos splines se cruzan en un punto.")]
    [SerializeField] private float junctionTolerance = 0.3f;

    private SplineContainer _container;
    public SplineContainer Container
    {
        get
        {
            if (_container == null) _container = GetComponent<SplineContainer>();
            return _container;
        }
    }

    public int SplineCount => Container.Splines.Count;

    public Vector3 GetPosition(int spline, float t) =>
        Container.EvaluatePosition(spline, Mathf.Clamp01(t));

    public Vector3 GetTangent(int spline, float t)
    {
        Vector3 tan = Container.EvaluateTangent(spline, Mathf.Clamp01(t));
        return tan.sqrMagnitude > 0.000001f ? tan.normalized : Vector3.forward;
    }

    public float GetLength(int spline) => Container.CalculateLength(spline);

    public struct Branch
    {
        public int spline;
        public float t;
        public Vector3 direction; // dirección de avance en mundo
    }

    public void GetBranchesAt(int spline, float t, List<Branch> result)
    {
        result.Clear();
        Vector3 pos = GetPosition(spline, t);
        Vector3 localPos = Container.transform.InverseTransformPoint(pos);

        for (int s = 0; s < SplineCount; s++)
        {
            float nearT;

            if (s == spline)
            {
                nearT = Mathf.Clamp01(t);
            }
            else
            {
                if (GetLength(s) <= 0f) continue;

                SplineUtility.GetNearestPoint(Container.Splines[s], (float3)localPos,
                    out float3 nearest, out nearT);

                Vector3 nearestWorld = Container.transform.TransformPoint((Vector3)nearest);
                if (Vector3.Distance(nearestWorld, pos) > junctionTolerance) continue;
            }

            Vector3 tan = GetTangent(s, nearT);

            if (nearT < 0.999f)
                result.Add(new Branch { spline = s, t = nearT, direction = tan });

            if (nearT > 0.001f)
                result.Add(new Branch { spline = s, t = nearT, direction = -tan });
        }
    }
}