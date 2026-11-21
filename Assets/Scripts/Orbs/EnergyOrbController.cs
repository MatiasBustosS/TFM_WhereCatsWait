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
    [SerializeField] private GameObject orbParticles;
    [SerializeField] private EnergyOrbVfxController orbVfxController;

    [Header("Configuración por Estado")]
    [Header("Estado 1: Vacío")]
    [SerializeField] private Color emptyColor = new Color(0.82f, 0.93f, 1f, 1f);
    [SerializeField] private float emptyScale = 0.12f;

    [Header("Estado 3: Energía Máxima")]
    [SerializeField] private Color maxEnergyColor = new Color(0.06f, 0.2f, 0.5f, 1f);
    [SerializeField] private float maxEnergyScale = 2.2f;

    private OrbState _currentState = OrbState.Empty;
    private int _collectedCount = 0;
    private Material _orbMaterial;
    private float EnergyProgress => orbs == null || orbs.Length == 0
        ? 0f
        : Mathf.Clamp01((float)_collectedCount / orbs.Length);

    private static readonly int ColorProperty = Shader.PropertyToID("_ShallowColor");
    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        if (orbMeshRenderer != null)
        {
            _orbMaterial = orbMeshRenderer.material;
        }

        if (orbVfxController == null)
        {
            orbVfxController = GetComponent<EnergyOrbVfxController>();
        }

        UpdateOrbVisuals();
    }

    private EnergyCollectable[] orbs;

    private void Start()
    {
        //orbs = FindObjectsByType<EnergyCollectable>(FindObjectsSortMode.None);
        
        orbs = FindObjectsByType<EnergyCollectable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        foreach (EnergyCollectable orb in orbs)
        {
            orb.gameObject.SetActive(false);
        }
        
        gameObject.SetActive(false);
        
    }

    private void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector3 targetPosition = playerTransform.position + followOffset;

        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        targetPosition.y += floatOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    public void SetActiveOrbs()
    {
        foreach (EnergyCollectable orb in orbs)
        {
            orb.gameObject.SetActive(true);
        }
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
        GameManager.instance.ActiveNpc.NextDialogue();
    }


    private void UpdateOrbVisuals()
    {
        float energyProgress = EnergyProgress;
        Color targetColor = Color.Lerp(emptyColor, maxEnergyColor, energyProgress);
        float targetScale = Mathf.Lerp(emptyScale, maxEnergyScale, energyProgress);

        if (orbParticles != null)
        {
            orbParticles.SetActive(true);
        }

        transform.localScale = Vector3.one * targetScale;

        if (_orbMaterial != null)
        {
            if (_orbMaterial.HasProperty(ColorProperty))
                _orbMaterial.SetColor(ColorProperty, targetColor);

            if (_orbMaterial.HasProperty(EmissionColorProperty))
            {
                float intensity = Mathf.Lerp(1f, 1.5f, energyProgress);
                _orbMaterial.SetColor(EmissionColorProperty, targetColor * intensity);
            }
        }

        if (orbVfxController != null)
        {
            float orbDiameter = orbMeshRenderer != null ? orbMeshRenderer.bounds.size.x : targetScale;
            orbVfxController.SetOrbVisuals(targetColor, orbDiameter, energyProgress);
        }
    }
}
