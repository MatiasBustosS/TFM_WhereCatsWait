using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundIndicatorManager : MonoBehaviour
{
    public static SoundIndicatorManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private RectTransform indicatorContainer;
    [SerializeField] private GameObject indicatorPrefab;

    [Header("Indicator")]
    [SerializeField] private float indicatorRadius = 250f;
    [SerializeField] private float indicatorDuration = 2f;

    [Header("Distance")]
    [SerializeField] private float maxSoundDistance = 30f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1f;

    private readonly List<SoundIndicator> _activeIndicators = new();

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
    }

    private void Update()
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
        }
    }

    public void ShowSound(SoundEmitter emitter)
    {
        if (playerCamera == null) return;
        
        float distance = Vector3.Distance(playerCamera.transform.position, emitter.transform.position);
        
        if (distance > maxSoundDistance) return;

        GameObject newIndicator = Instantiate(indicatorPrefab,  indicatorContainer.transform);

        SoundIndicator indicator = newIndicator.GetComponent<SoundIndicator>();

        if (indicator == null)
        {
            Debug.LogError("El prefab necesita el componente SoundIndicator.");

            Destroy(newIndicator);
            return;
        }

        indicator.Initialize(emitter.transform.position, indicatorDuration, null);

        _activeIndicators.Add(indicator);

        UpdateIndicator(indicator);
    }

    private void UpdateIndicator(SoundIndicator indicator)
    {
        Vector3 direction = indicator.WorldPosition - playerCamera.transform.position;
        
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        direction.Normalize();
        
        Vector3 cameraForward = playerCamera.transform.forward; 
        cameraForward.y = 0f; 
        cameraForward.Normalize();

        float angle = Vector3.SignedAngle(cameraForward, direction, Vector3.up);

        indicator.RectTransform.anchoredPosition = Vector2.zero;

        indicator.RectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle);

        indicator.Tick(Time.deltaTime);

        if (indicator.IsFinished)
        {
            _activeIndicators.Remove(indicator);
            Destroy(indicator.gameObject);
        }
    }
}