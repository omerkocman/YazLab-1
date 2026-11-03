using System;
using UnityEngine;

public class PoisonGasVent : MonoBehaviour
{
    [Header("Gas References")]
    [SerializeField] private ParticleSystem mainParticles;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private PoisonGasDamageZone damageZone;

    [Header("Settings")]
    [SerializeField] private bool isActive = true;

    public bool IsActive => isActive;

    private void Awake()
    {
        SetGasActive(isActive);
    }

    public void SetGasActive(bool active)
    {
        isActive = active;

        if(mainParticles != null)
        {
            if(isActive)
            {
                mainParticles.Play();
            }

            else
            {
                mainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if(smokeParticles != null)
        {
            if(isActive)
            {
                smokeParticles.Play();
            }

            else
            {
                smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

        }

        if(damageZone != null)
        {
            damageZone.SetDamageActive(isActive);
        }

        Debug.Log($"[POISON GAS] Active: {isActive}");
    }
}
