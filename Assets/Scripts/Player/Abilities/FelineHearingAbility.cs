using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oído Felino (apartado 4.6.1.1 del GDD): detecta fuentes sonoras ocultas
/// (fragmentos, accesos, elementos narrativos) dentro de un radio de
/// escucha. La intensidad aumenta a medida que Yui se acerca a la fuente.
///
/// El paneo estéreo direccional que pide el GDD lo resuelve el propio
/// AudioSource 3D de Unity (spatialBlend = 1) en cada IFelineHearable — no
/// hace falta calcularlo aquí. Ver FelineHearableSource de ejemplo.
/// La opción de accesibilidad "visualizarlo en pantalla" (para sistemas sin
/// estéreo) puede construirse en el HUD suscribiéndose a OnFelineHearingDetected.
/// </summary>
public class FelineHearingAbility : FelineAbilityBase
{
    [Header("Oído Felino")]
    [SerializeField] private float hearingRadius = 7f;
    [SerializeField] private LayerMask hearableLayer;

    private readonly Collider[] _overlapBuffer = new Collider[16];
    private HashSet<IFelineHearable> _tracked = new();
    private HashSet<IFelineHearable> _trackedThisFrame = new();

    protected override void OnActivate()
    {
        _tracked.Clear();
        _trackedThisFrame.Clear();
    }

    protected override void TickActive(float deltaTime)
    {
        Vector3 center = transform.position;
        int count = Physics.OverlapSphereNonAlloc(center, hearingRadius, _overlapBuffer, hearableLayer);

        _trackedThisFrame.Clear();

        for (int i = 0; i < count; i++)
        {
            if (!_overlapBuffer[i].TryGetComponent(out IFelineHearable hearable)) continue;

            float distance = Vector3.Distance(center, _overlapBuffer[i].transform.position);
            float intensity01 = 1f - Mathf.Clamp01(distance / hearingRadius);

            hearable.OnFelineHearingDetected(intensity01, _overlapBuffer[i].transform.position);
            _trackedThisFrame.Add(hearable);
        }

        // Avisa a las fuentes que estaban sonando el frame anterior y ya han
        // quedado fuera del radio.
        foreach (var hearable in _tracked)
        {
            if (!_trackedThisFrame.Contains(hearable))
                hearable.OnFelineHearingLost();
        }

        (_tracked, _trackedThisFrame) = (_trackedThisFrame, _tracked);
    }

    protected override void OnDeactivate()
    {
        foreach (var hearable in _tracked)
        {
            hearable.OnFelineHearingLost();
        }
        _tracked.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, hearingRadius);
    }
}
