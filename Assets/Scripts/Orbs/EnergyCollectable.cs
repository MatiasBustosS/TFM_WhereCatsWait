using System;
using UnityEngine;

public class EnergyCollectable : MonoBehaviour
{
    private EnergyOrbController orb;

    private void Start()
    {
        orb = FindFirstObjectByType<EnergyOrbController>();
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
