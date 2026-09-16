using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    public static List<SoundEmitter> AllEmitters = new();

    [SerializeField] private AudioSource audioSource;
    private EnergyCollectable energyCollectable;

    private void Start()
    {
        energyCollectable = GetComponent<EnergyCollectable>();
    }

    private void OnEnable()
    {
        AllEmitters.Add(this);
    }

    private void OnDisable()
    {
        AllEmitters.Remove(this);
    }

    public void EmitSound()
    {
        if(!energyCollectable.IsSound) return;
        StartCoroutine(ShowSound());
    }

    IEnumerator ShowSound()
    {
        if (audioSource != null) audioSource.Play();
        
        yield return new WaitForSeconds(0.5f);
        
        if (SoundIndicatorManager.Instance != null)
        {
            SoundIndicatorManager.Instance.ShowSound(this);
        }
    }
}