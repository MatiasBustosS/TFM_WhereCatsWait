using UnityEngine;

/// <summary>
/// Ejemplo listo para usar de una fuente sonora detectable por el Oído
/// Felino. Usa un AudioSource 3D (spatialBlend = 1) para que Unity gestione
/// automáticamente el paneo estéreo direccional que pide el GDD; este
/// script sólo ajusta el volumen según la intensidad recibida (más fuerte
/// cuanto más cerca esté Yui de la fuente).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class FelineHearableSource : MonoBehaviour, IFelineHearable
{
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Volumen en función de la intensidad (0 = borde del radio, 1 = justo encima).")]
    [SerializeField] private AnimationCurve volumeByIntensity = AnimationCurve.Linear(0f, 0.15f, 1f, 1f);

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.spatialBlend = 1f; // 3D: habilita el paneo estéreo automático
        audioSource.playOnAwake = false;
    }

    public void OnFelineHearingDetected(float intensity01, Vector3 sourcePosition)
    {
        if (!audioSource.isPlaying) audioSource.Play();
        audioSource.volume = volumeByIntensity.Evaluate(intensity01);
    }

    public void OnFelineHearingLost()
    {
        audioSource.Stop();
    }
}
