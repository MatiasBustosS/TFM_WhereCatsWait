using System.Collections;
using UnityEngine;

/// <summary>
/// Ejemplo listo para usar de un elemento revelable por la Vista Felina
/// (huella, fragmento oculto, marcador de ruta). Colócalo en cualquier
/// GameObject dentro de la capa "detectableLayer" que hayas configurado en
/// FelineVisionAbility, y asigna aquí el efecto visual a mostrar/ocultar
/// (partícula, luz, glow de material, etc.). No necesitas tocar código para
/// añadir nuevas huellas o fragmentos al nivel.
/// </summary>
public class FelineDetectableObject : MonoBehaviour, IFelineRevealable
{
    [SerializeField] private GameObject visualEffect;
    [Tooltip("Actívalo para huellas de un solo uso que no deben volver a revelarse.")]
    [SerializeField] private bool consumeOnReveal = false;

    private Coroutine _hideRoutine;
    private bool _consumed;

    public void OnFelineVisionReveal(float activeDuration)
    {
        if (_consumed) return;

        if (visualEffect != null) visualEffect.SetActive(true);

        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(HideAfter(activeDuration));

        if (consumeOnReveal) _consumed = true;
    }

    public void OnFelineVisionHide()
    {
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        if (visualEffect != null) visualEffect.SetActive(false);
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (visualEffect != null) visualEffect.SetActive(false);
    }
}
