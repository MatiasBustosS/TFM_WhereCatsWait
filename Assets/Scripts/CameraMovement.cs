using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private CinemachineFollow cinemachineFollow;

    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float waitTime = 0.5f;

    private Vector3 originalOffset;

    private void Start()
    {
        originalOffset = cinemachineFollow.FollowOffset;
    }

    public void PlayCameraAnimation()
    {
        StartCoroutine(CameraAnimation());
    }

    private IEnumerator CameraAnimation()
    {
        yield return MoveCamera(originalOffset, targetOffset, duration);

        yield return new WaitForSeconds(waitTime);

        yield return MoveCamera(targetOffset, originalOffset, duration);
        
        GameManager.instance.RailPlayerMovement.SetCanMove(true);
    }

    private IEnumerator MoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            cinemachineFollow.FollowOffset =
                Vector3.Lerp(from, to, t);

            yield return null;
        }

        cinemachineFollow.FollowOffset = to;
    }
}