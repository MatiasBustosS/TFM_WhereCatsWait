using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundIndicatorManager : MonoBehaviour
{
    private const int HearingPulseSegmentCount = 2;

    public static SoundIndicatorManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform indicatorOrigin;
    [SerializeField] private RectTransform indicatorContainer;
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Sprite noDirectionSprite;

    [Header("No target feedback")]
    [SerializeField] private Color noDirectionColor = Color.red;

    [Header("Indicator")]
    [SerializeField] private float indicatorRadius = 250f;
    [SerializeField] private float indicatorDuration = 2f;

    [Header("Distance")]
    [SerializeField] private float maxSoundDistance = 30f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1f;

    private readonly List<SoundIndicator> _activeIndicators = new();
    private Camera indicatorUICamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerCamera == null)
            Debug.LogError("SoundIndicatorManager requires a player camera.", this);

        if (indicatorOrigin == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                indicatorOrigin = player.transform;
        }

        if (indicatorOrigin == null)
            Debug.LogError("SoundIndicatorManager requires a Player transform as its indicator origin.", this);

        if (indicatorContainer == null)
        {
            Debug.LogError("SoundIndicatorManager requires an indicator container.", this);
            return;
        }

        if (indicatorPrefab == null)
        {
            Debug.LogError("SoundIndicatorManager requires an indicator prefab.", this);
            return;
        }

        Canvas canvas = indicatorContainer.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("SoundIndicatorManager requires its indicator container to be inside a Canvas.", this);
            return;
        }
        else if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            indicatorUICamera = canvas.worldCamera != null
                ? canvas.worldCamera
                : playerCamera;
        }
    }

    private void LateUpdate()
    {
        for (int i = _activeIndicators.Count - 1; i >= 0; i--)
        {
            SoundIndicator indicator = _activeIndicators[i];

            if (indicator == null)
            {
                _activeIndicators.RemoveAt(i);
                continue;
            }

            UpdateIndicator(indicator);
            if (indicator.IsFinished)
            {
                _activeIndicators.RemoveAt(i);
                Destroy(indicator.gameObject);
            }
        }
    }

    public void ShowSound(SoundEmitter emitter)
    {
        if (emitter == null)
        {
            Debug.LogError("SoundIndicatorManager cannot show a sound for a missing emitter.", this);
            return;
        }

        if (playerCamera == null || indicatorOrigin == null || indicatorContainer == null || indicatorPrefab == null)
            return;
        
        float distance = Vector3.Distance(indicatorOrigin.position, emitter.transform.position);
        
        if (distance > maxSoundDistance) return;

        CreateIndicator(emitter.transform.position, true);
    }

    public void ShowHearingPulse()
    {
        if (playerCamera == null || indicatorOrigin == null || indicatorContainer == null || indicatorPrefab == null)
            return;

        if (noDirectionSprite == null)
        {
            Debug.LogError("SoundIndicatorManager requires a sprite for hearing without a direction.", this);
            return;
        }

        float rotationStep = 360f / HearingPulseSegmentCount;
        for (int i = 0; i < HearingPulseSegmentCount; i++)
        {
            if (!CreateIndicator(
                indicatorOrigin.position,
                false,
                noDirectionSprite,
                i * rotationStep,
                noDirectionColor))
                break;
        }
    }

    private bool CreateIndicator(
        Vector3 worldPosition,
        bool hasDirection,
        Sprite icon = null,
        float initialRotation = 0f,
        Color? tint = null)
    {
        GameObject newIndicator = Instantiate(indicatorPrefab, indicatorContainer.transform);

        SoundIndicator indicator = newIndicator.GetComponent<SoundIndicator>();

        if (indicator == null || indicator.RectTransform == null)
        {
            Debug.LogError("El prefab necesita un componente SoundIndicator con RectTransform.", this);

            Destroy(newIndicator);
            return false;
        }

        indicator.Initialize(worldPosition, indicatorDuration, icon, hasDirection, initialRotation, tint);

        _activeIndicators.Add(indicator);
        return true;
    }

    private void UpdateIndicator(SoundIndicator indicator)
    {
        if (indicatorOrigin != null && playerCamera != null && indicatorContainer != null)
        {
            Vector3 originScreenPosition = playerCamera.WorldToScreenPoint(indicatorOrigin.position);
            Vector3 soundScreenPosition = playerCamera.WorldToScreenPoint(indicator.WorldPosition);
            if (originScreenPosition.z > 0f)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    indicatorContainer,
                    originScreenPosition,
                    indicatorUICamera,
                    out Vector2 originLocalPosition))
                {
                    indicator.RectTransform.anchoredPosition = originLocalPosition;
                }
            }

            if (indicator.HasDirection)
            {
                Vector2 screenDirection = Vector2.zero;
                if (originScreenPosition.z > 0f)
                {
                    screenDirection = new Vector2(
                        soundScreenPosition.x - originScreenPosition.x,
                        soundScreenPosition.y - originScreenPosition.y);
                    if (soundScreenPosition.z < 0f)
                        screenDirection = -screenDirection;
                }

                if (screenDirection.sqrMagnitude < 0.001f)
                {
                    Vector3 worldDirection = Vector3.ProjectOnPlane(
                        indicator.WorldPosition - indicatorOrigin.position,
                        Vector3.up);
                    Vector3 cameraForward = Vector3.ProjectOnPlane(
                        playerCamera.transform.forward,
                        Vector3.up);

                    if (worldDirection.sqrMagnitude > 0.001f && cameraForward.sqrMagnitude > 0.001f)
                    {
                        float bearing = Vector3.SignedAngle(cameraForward, worldDirection, Vector3.up);
                        float bearingRadians = bearing * Mathf.Deg2Rad;
                        screenDirection = new Vector2(Mathf.Sin(bearingRadians), Mathf.Cos(bearingRadians));
                    }
                }

                if (screenDirection.sqrMagnitude > 0.001f)
                {
                    float angle = Mathf.Atan2(screenDirection.x, screenDirection.y) * Mathf.Rad2Deg;
                    indicator.RectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);
                }
            }
        }

        indicator.Tick(Time.deltaTime);
    }
}