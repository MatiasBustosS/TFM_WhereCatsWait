using UnityEngine;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using System.Collections;

public class CameraMovement2 : MonoBehaviour
{
    private const float RuleOfThirdsOffset = 1f / 6f;

    [SerializeField] private CinemachineFollow cinemachineFollow;
    [SerializeField] private CinemachineRotationComposer rotationComposer;

    [Header("Character Facing")]
    [SerializeField] private Transform yuiFacing;
    [SerializeField] private float frontViewDistance = 2f;
    [SerializeField] private float frontViewHeight = 0.2f;
    [SerializeField] private Vector3 rightAbilitiesCameraLocalPosition = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 rightAbilitiesCameraLocalEulerAngles = new Vector3(0f, 115f, 0f);
    [SerializeField] private Vector3 leftAbilitiesCameraLocalPosition = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector3 leftAbilitiesCameraLocalEulerAngles = new Vector3(0f, 55f, 0f);

    [Header("Player Facing Camera")]
    [SerializeField] private Vector3 rightFollowOffset = new Vector3(-2f, 2f, -7f);
    [SerializeField] private Vector2 rightFacingScreenPosition = new Vector2(-0.225f, 0f);
    [SerializeField] private Vector3 leftFollowOffset = new Vector3(2f, 2f, -7f);
    [SerializeField] private Vector2 leftFacingScreenPosition = new Vector2(0.225f, 0f);

    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float waitTime = 0.5f;

    private bool isPlayingAnimation = false;
    public bool isSelectingAbility { get; private set; }
    private Coroutine facingTransition;
    private Vector3 startingFollowOffset;
    private Transform startingLookAt;
    private Transform yuiTransform;
    private Quaternion startingYuiRotation;
    private Vector3 startingFacingLocalPosition;
    private Quaternion startingFacingLocalRotation;
    private bool hasStartingCameraState;
    
    // Inicializa el componente y resuelve el compositor de rotación si hace falta.
    private void Start()
    {
        if (rotationComposer == null && cinemachineFollow != null)
        {
            rotationComposer = cinemachineFollow.GetComponent<CinemachineRotationComposer>();
        }
    }

    // Cambia la orientación de la cámara para mirar hacia la derecha o izquierda.
    public void SetPlayerFacing(bool right, float transitionDuration)
    {
        if (cinemachineFollow == null) return;

        if (rotationComposer == null)
            rotationComposer = cinemachineFollow.GetComponent<CinemachineRotationComposer>();

        if (rotationComposer != null)
        {
            var composition = rotationComposer.Composition;
            composition.DeadZone.Enabled = false;
            composition.DeadZone.Size = Vector2.zero;
            rotationComposer.Composition = composition;
            rotationComposer.CenterOnActivate = false;
        }

        if (facingTransition != null) StopCoroutine(facingTransition);
        facingTransition = StartCoroutine(TransitionFacing(
            right ? rightFollowOffset : leftFollowOffset,
            GetThirdsScreenPosition(right),
            transitionDuration));
    }

    private Vector2 GetThirdsScreenPosition(bool right)
    {
        Vector2 screenPosition = right ? rightFacingScreenPosition : leftFacingScreenPosition;
        screenPosition.x = right ? -RuleOfThirdsOffset : RuleOfThirdsOffset;
        return screenPosition;
    }

    // Inicia la animación de la cámara si aún no está reproduciéndose.
    public void PlayCameraAnimation()
    {
        if( !isPlayingAnimation) StartCoroutine(CameraAnimation());
    }

    // Reproduce la secuencia de acercamiento y retorno de la cámara durante la animación.
    private IEnumerator CameraAnimation()
    {
        isPlayingAnimation = true;
        Vector3 restoreOffset = cinemachineFollow.FollowOffset;
        yield return MoveCamera(restoreOffset, targetOffset, duration);

        yield return new WaitForSeconds(waitTime);

        yield return MoveCamera(targetOffset, restoreOffset, duration);
        
        GameManager.instance.RailPlayerMovement.SetCanMove(true);
        
        isPlayingAnimation = false;
    }

    // Transición suave del offset y la posición de pantalla de la cámara.
    private IEnumerator TransitionFacing(Vector3 targetFollowOffset, Vector2 targetScreenPosition, float transitionDuration)
    {
        Vector3 startFollowOffset = cinemachineFollow.FollowOffset;
        Vector2 startScreenPosition = rotationComposer != null
            ? rotationComposer.Composition.ScreenPosition
            : targetScreenPosition;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / transitionDuration);
            cinemachineFollow.FollowOffset = Vector3.Lerp(startFollowOffset, targetFollowOffset, t);

            if (rotationComposer != null)
            {
                var composition = rotationComposer.Composition;
                composition.ScreenPosition = Vector2.Lerp(startScreenPosition, targetScreenPosition, t);
                rotationComposer.Composition = composition;
            }

            yield return null;
        }

        cinemachineFollow.FollowOffset = targetFollowOffset;
        if (rotationComposer != null)
        {
            var composition = rotationComposer.Composition;
            composition.ScreenPosition = targetScreenPosition;
            rotationComposer.Composition = composition;
        }

        facingTransition = null;
    }

    // Desplaza el offset de la cámara entre dos posiciones con interpolación.
    private IEnumerator MoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            cinemachineFollow.FollowOffset = Vector3.Lerp(from, to, t);

            yield return null;
        }

        cinemachineFollow.FollowOffset = to;
    }

    // Coloca la cámara delante del personaje para preparar la vista de habilidad.
    public bool MoveCameraInFront(float transitionDuration)
    {
        if (cinemachineFollow == null)
        {
            Debug.LogError("CameraMovement requires a CinemachineFollow reference.", this);
            return false;
        }

        if (yuiFacing == null)
        {
            Debug.LogError("Assign Yui's facing transform to CameraMovement.", this);
            return false;
        }

        CinemachineCamera virtualCamera = cinemachineFollow.GetComponent<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineFollow must be on a CinemachineCamera.", this);
            return false;
        }

        if (rotationComposer == null)
            rotationComposer = cinemachineFollow.GetComponent<CinemachineRotationComposer>();

        if (rotationComposer == null || !rotationComposer.enabled)
        {
            Debug.LogError("The CinemachineCamera needs an enabled CinemachineRotationComposer to look at Yui.", this);
            return false;
        }

        Transform followTarget = cinemachineFollow.FollowTarget;
        if (followTarget == null)
        {
            Debug.LogError("The CinemachineCamera must have a Tracking Target.", this);
            return false;
        }

        if (cinemachineFollow.TrackerSettings.BindingMode != BindingMode.WorldSpace)
        {
            Debug.LogError("MoveCameraInFront requires the Cinemachine Follow Binding Mode to be World Space.", this);
            return false;
        }

        if (yuiFacing.parent == null)
        {
            Debug.LogError("AbilitiesCamera must be a child of Yui so her rotation can be changed.", this);
            return false;
        }

        Vector3 cameraPosition = followTarget.position + cinemachineFollow.FollowOffset;
        Vector3 directionToCamera = Vector3.ProjectOnPlane(
            cameraPosition - yuiFacing.parent.position,
            Vector3.up
        );
        if (directionToCamera.sqrMagnitude < 0.0001f)
        {
            Debug.LogError("The camera must not be directly above or below Yui.", this);
            return false;
        }

        startingFollowOffset = cinemachineFollow.FollowOffset;
        startingLookAt = virtualCamera.LookAt;
        yuiTransform = yuiFacing.parent;
        startingYuiRotation = yuiTransform.rotation;
        startingFacingLocalPosition = yuiFacing.localPosition;
        startingFacingLocalRotation = yuiFacing.localRotation;
        Vector3 cameraDirectionInYuiSpace = yuiTransform.InverseTransformDirection(directionToCamera.normalized);
        bool cameraIsOnYuiRight = cameraDirectionInYuiSpace.x >= 0f;
        Vector3 targetFacingLocalPosition = cameraIsOnYuiRight
            ? rightAbilitiesCameraLocalPosition
            : leftAbilitiesCameraLocalPosition;
        Quaternion targetFacingLocalRotation = Quaternion.Euler(
            cameraIsOnYuiRight
                ? rightAbilitiesCameraLocalEulerAngles
                : leftAbilitiesCameraLocalEulerAngles
        );
        hasStartingCameraState = true;
        virtualCamera.LookAt = yuiFacing;

        if (facingTransition != null)
            StopCoroutine(facingTransition);

        isSelectingAbility = true;
        facingTransition = StartCoroutine(TransitionToFront(
            transitionDuration,
            targetFacingLocalPosition,
            targetFacingLocalRotation));
        return true;
    }

    // Vuelve la cámara y la orientación del personaje a la posición inicial guardada.
    public bool ReturnToStartingPosition(float transitionDuration)
    {
        if (!hasStartingCameraState || cinemachineFollow == null || rotationComposer == null || yuiTransform == null)
        {
            Debug.LogError("No starting camera position has been saved.", this);
            return false;
        }

        CinemachineCamera virtualCamera = cinemachineFollow.GetComponent<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("CinemachineFollow must be on a CinemachineCamera.", this);
            return false;
        }

        if (facingTransition != null)
            StopCoroutine(facingTransition);

        isSelectingAbility = true;
        facingTransition = StartCoroutine(TransitionToStartingPosition(virtualCamera, transitionDuration));
        return true;
    }

    // Restaura gradualmente la posición inicial de la cámara y de la mirada.
    private IEnumerator TransitionToStartingPosition(CinemachineCamera virtualCamera, float transitionDuration)
    {
        Vector3 startFollowOffset = cinemachineFollow.FollowOffset;
        Quaternion currentYuiRotation = yuiTransform.rotation;
        Vector3 currentFacingLocalPosition = yuiFacing.localPosition;
        Quaternion currentFacingLocalRotation = yuiFacing.localRotation;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / transitionDuration);

            cinemachineFollow.FollowOffset = Vector3.Lerp(startFollowOffset, startingFollowOffset, t);
            yuiTransform.rotation = Quaternion.Slerp(currentYuiRotation, startingYuiRotation, t);
            yuiFacing.localPosition = Vector3.Lerp(currentFacingLocalPosition, startingFacingLocalPosition, t);
            yuiFacing.localRotation = Quaternion.Slerp(currentFacingLocalRotation, startingFacingLocalRotation, t);

            yield return null;
        }

        cinemachineFollow.FollowOffset = startingFollowOffset;
        yuiTransform.rotation = startingYuiRotation;
        yuiFacing.localPosition = startingFacingLocalPosition;
        yuiFacing.localRotation = startingFacingLocalRotation;
        virtualCamera.LookAt = startingLookAt;

        hasStartingCameraState = false;
        yuiTransform = null;
        isSelectingAbility = false;
        facingTransition = null;
    }

    // Lleva la cámara a la vista frontal de la habilidad con una transición suave.
    private IEnumerator TransitionToFront(
        float transitionDuration,
        Vector3 targetFacingLocalPosition,
        Quaternion targetFacingLocalRotation)
    {
        if (yuiFacing == null)
        {
            isSelectingAbility = false;
            facingTransition = null;
            yield break;
        }

        Transform followTarget = cinemachineFollow.FollowTarget;
        if (followTarget == null)
        {
            isSelectingAbility = false;
            facingTransition = null;
            yield break;
        }

        Vector3 up = Vector3.up;
        Vector3 cameraPosition = followTarget.position + cinemachineFollow.FollowOffset;
        Vector3 directionToCamera = Vector3.ProjectOnPlane(
            cameraPosition - yuiTransform.position,
            up
        ).normalized;
        Quaternion startYuiRotation = yuiTransform.rotation;
        Quaternion targetYuiRotation = Quaternion.LookRotation(directionToCamera, up);
        Vector3 targetFacingPosition = yuiTransform.position
            + targetYuiRotation * Vector3.Scale(targetFacingLocalPosition, yuiTransform.lossyScale);
        float targetCameraHeight = targetFacingPosition.y - yuiTransform.position.y + frontViewHeight;
        Vector3 targetCameraPosition =
            yuiTransform.position
            + directionToCamera * frontViewDistance
            + up * targetCameraHeight;
        Vector3 startFollowOffset = cinemachineFollow.FollowOffset;
        Vector3 targetFollowOffset = targetCameraPosition - followTarget.position;
        Vector3 startFacingLocalPosition = yuiFacing.localPosition;
        Quaternion startFacingLocalRotation = yuiFacing.localRotation;
        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / transitionDuration);

            cinemachineFollow.FollowOffset = Vector3.Lerp(startFollowOffset, targetFollowOffset, t);
            yuiTransform.rotation = Quaternion.Slerp(startYuiRotation, targetYuiRotation, t);
            yuiFacing.localPosition = Vector3.Lerp(startFacingLocalPosition, targetFacingLocalPosition, t);
            yuiFacing.localRotation = Quaternion.Slerp(startFacingLocalRotation, targetFacingLocalRotation, t);

            yield return null;
        }

        cinemachineFollow.FollowOffset = targetFollowOffset;
        yuiTransform.rotation = targetYuiRotation;
        yuiFacing.localPosition = targetFacingLocalPosition;
        yuiFacing.localRotation = targetFacingLocalRotation;

        isSelectingAbility = false;
        facingTransition = null;
    }
}
