using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vista Felina (apartado 4.6.1.1 del GDD): revela huellas, fragmentos
/// ocultos y opciones de navegación dentro de un radio que se mueve junto
/// a Yui mientras la habilidad está activa. Cualquier IFelineRevealable que
/// entre en el radio durante esos segundos se revela.
/// </summary>
public class FelineVisionAbility : FelineAbilityBase
{
    [Header("Vista Felina")]
    [Tooltip("Normalmente los ojos de Yui. Si se deja vacío, se usa este transform.")]
    [SerializeField] private Transform originPoint;
    [SerializeField] private float visionRadius = 4f;
    [SerializeField] private LayerMask detectableLayer;

    private readonly HashSet<IFelineRevealable> _revealedThisActivation = new();
    private readonly Collider[] _overlapBuffer = new Collider[16];

    protected override void OnActivate()
    {
        _revealedThisActivation.Clear();
    }

    protected override void TickActive(float deltaTime)
    {
        Vector3 center = originPoint != null ? originPoint.position : transform.position;
        int count = Physics.OverlapSphereNonAlloc(center, visionRadius, _overlapBuffer, detectableLayer);

        for (int i = 0; i < count; i++)
        {
            if (!_overlapBuffer[i].TryGetComponent(out IFelineRevealable revealable)) continue;
            if (_revealedThisActivation.Contains(revealable)) continue;

            _revealedThisActivation.Add(revealable);
            revealable.OnFelineVisionReveal(activeDuration);
        }
    }

    protected override void OnDeactivate()
    {
        // Cada objeto revelado se oculta solo pasado "activeDuration" (ver
        // FelineDetectableObject de ejemplo), pero avisamos igualmente por
        // si la habilidad se corta antes de tiempo (ForceDeactivate).
        foreach (var revealable in _revealedThisActivation)
        {
            revealable.OnFelineVisionHide();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = originPoint != null ? originPoint.position : transform.position;
        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.35f);
        Gizmos.DrawSphere(center, visionRadius);
    }
}
