using System;
using UnityEngine;

public class EnergyOrbController : MonoBehaviour
{
    public enum OrbState { Empty, Collecting, MaxEnergy }

    [Header("Seguimiento")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 1.8f, -0.5f);
    [SerializeField] private float followSpeed = 4f;

    [Header("Efecto de Flotación")]
    [SerializeField] private float floatAmplitude = 0.25f; 
    [SerializeField] private float floatFrequency = 2f;   

    [Header("Referencias Visuales")]
    [SerializeField] private MeshRenderer orbMeshRenderer;
    [SerializeField] private ParticleSystem orbParticles;

    [Header("Configuración por Estado")]
    [Header("Estado 1: Vacío")]
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private float emptyScale = 0.5f;

    [Header("Estado 2: Recolectando")]
    [SerializeField] private Color collectingColor = new Color(1f, 0.9f, 0.2f, 0.8f);
    [SerializeField] private float baseCollectingScale = 0.6f;
    [SerializeField] private float scalePerEnergy = 0.05f;
    [SerializeField] private float maxCollectingScale = 1.8f;

    [Header("Estado 3: Energía Máxima")]
    [SerializeField] private Color maxEnergyColor = new Color(0f, 0.5f, 1f, 1f);
    [SerializeField] private float maxEnergyScale = 2.2f;

    private OrbState _currentState = OrbState.Empty;
    private int _collectedCount = 0;
    private Material _orbMaterial;
    private ParticleSystem.MainModule _particleMain;

    private static readonly int ColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (orbMeshRenderer != null)
        {
            _orbMaterial = orbMeshRenderer.material;
        }

        if (orbParticles != null)
        {
            _particleMain = orbParticles.main;
        }

        UpdateOrbVisuals();
    }

    private EnergyCollectable[] orbs;

    private void Start()
    {
        orbs = FindObjectsByType<EnergyCollectable>(FindObjectsSortMode.None);
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 targetPosition = playerTransform.position + followOffset;

        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        targetPosition.y += floatOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }


    public void SetEmptyState()
    {
        _currentState = OrbState.Empty;
        _collectedCount = 0;
        UpdateOrbVisuals();
    }

    public void AddEnergy(int amount = 1)
    {
        _collectedCount += amount;

        if (_collectedCount >= orbs.Length)
        {
            SetMaxEnergyState();
        }

        if (_currentState != OrbState.MaxEnergy)
        {
            _currentState = OrbState.Collecting;
        }

        UpdateOrbVisuals();
    }
    
    private void SetMaxEnergyState()
    {
        _currentState = OrbState.MaxEnergy;
        UpdateOrbVisuals();
    }


    private void UpdateOrbVisuals()
    {
        Color targetColor = Color.white;
        float targetScale = 1f;

        switch (_currentState)
        {
            case OrbState.Empty:
                targetColor = emptyColor;
                targetScale = emptyScale;
                SetParticleEmission(false);
                break;

            case OrbState.Collecting:
                targetColor = collectingColor;
                targetScale = Mathf.Min(baseCollectingScale + (_collectedCount * scalePerEnergy), maxCollectingScale);
                SetParticleEmission(true);
                break;

            case OrbState.MaxEnergy:
                targetColor = maxEnergyColor;
                targetScale = maxEnergyScale;
                SetParticleEmission(true);
                break;
        }

        transform.localScale = Vector3.one * targetScale;

        if (_orbMaterial != null)
        {
            if (_orbMaterial.HasProperty(ColorProperty))
                _orbMaterial.SetColor(ColorProperty, targetColor);

            if (_orbMaterial.HasProperty(EmissionColorProperty))
            {
                float intensity = (_currentState == OrbState.MaxEnergy) ? 4f : 2f;
                _orbMaterial.SetColor(EmissionColorProperty, targetColor * intensity);
            }
        }

        if (orbParticles != null)
        {
            _particleMain.startColor = targetColor;
        }
    }

    private void SetParticleEmission(bool enabled)
    {
        if (orbParticles == null) return;
        var emission = orbParticles.emission;
        emission.enabled = enabled;
    }
}
