using System;
using UnityEngine;

public class EnergyCollectable : MonoBehaviour
{
    private EnergyOrbController orb;

    [SerializeField] private bool isSound;
    [SerializeField] private bool isVisual;
    [SerializeField] private GameObject visualTrace;
    
    public bool IsSound => isSound;
    

    private void Start()
    {
        orb = FindFirstObjectByType<EnergyOrbController>();

        visualTrace.SetActive(isVisual);
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (orb == null) return;
            orb.AddEnergy(1);
            Destroy(gameObject);
        }
    }
}
