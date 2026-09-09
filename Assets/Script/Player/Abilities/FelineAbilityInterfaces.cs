using UnityEngine;

/// <summary>
/// Estados de una habilidad felina, tal y como se describen en el GDD
/// (apartado 4.6.1.1): Recarga (blanquecino) -> Disponible (azul) -> Activa
/// (amarillo) -> vuelta a Recarga.
/// </summary>
public enum FelineAbilityState
{
    Recharging,
    Available,
    Active
}

/// <summary>
/// Cualquier elemento del escenario que la Vista Felina pueda revelar:
/// huellas, fragmentos ocultos, opciones de navegación con acciones felinas.
/// Implementa esta interfaz en el componente que gestione el efecto visual
/// del objeto (o usa FelineDetectableObject de ejemplo).
/// </summary>
public interface IFelineRevealable
{
    void OnFelineVisionReveal(float activeDuration);
    void OnFelineVisionHide();
}

/// <summary>
/// Cualquier fuente sonora que el Oído Felino pueda detectar: fragmentos,
/// accesos ocultos, elementos narrativos con resonancia espiritual.
/// </summary>
public interface IFelineHearable
{
    /// intensity01: 0 = borde del radio de escucha, 1 = justo sobre la fuente.
    void OnFelineHearingDetected(float intensity01, Vector3 sourcePosition);
    void OnFelineHearingLost();
}
