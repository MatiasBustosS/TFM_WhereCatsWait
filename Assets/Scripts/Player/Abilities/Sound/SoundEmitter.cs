using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    private static readonly List<SoundEmitter> activeEmitters = new();

    public static IReadOnlyList<SoundEmitter> ActiveEmitters => activeEmitters;

    [SerializeField] private AudioSource audioSource;
    private EnergyCollectable energyCollectable;
    public bool CanEmitSound => energyCollectable != null && energyCollectable.IsSound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveEmitters()
    {
        activeEmitters.Clear();
    }

    private void Awake()
    {
        energyCollectable = GetComponent<EnergyCollectable>();
        if (energyCollectable == null)
            Debug.LogError("SoundEmitter requires an EnergyCollectable component.", this);
    }

    private void OnEnable()
    {
        activeEmitters.Add(this);
    }

    private void OnDisable()
    {
        activeEmitters.Remove(this);
    }

    public void EmitSound()
    {
        if (!CanEmitSound)
            return;

        StartCoroutine(ShowSound());
    }

    private IEnumerator ShowSound()
    {
        if (audioSource != null) audioSource.Play();
        
        yield return new WaitForSeconds(0.5f);
        
        if (SoundIndicatorManager.Instance != null)
        {
            SoundIndicatorManager.Instance.ShowSound(this);
        }
    }
}