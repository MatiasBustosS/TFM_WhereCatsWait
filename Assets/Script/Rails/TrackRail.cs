using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class TrackRail : MonoBehaviour
{
    private SplineContainer _splineContainer;

    public SplineContainer SplineContainer
    {
        get
        {
            if (_splineContainer == null)
                _splineContainer = GetComponent<SplineContainer>();
            return _splineContainer;
        }
    }

    public Vector3 GetPositionAt(float t)
    {
        return SplineContainer.EvaluatePosition(t);
    }

    public Vector3 GetForwardAt(float t)
    {
        return Vector3.Normalize(SplineContainer.EvaluateTangent(t));
    }

    public float GetLength()
    {
        return SplineContainer.CalculateLength();
    }
}