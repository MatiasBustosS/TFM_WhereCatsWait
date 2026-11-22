using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnergyOrbVfxController : MonoBehaviour
{
    [SerializeField] private ParticleSystem orbParticles;
    [SerializeField] private TrailRenderer orbTrail;
    [SerializeField] private float minEmissionRate = 2f;
    [SerializeField] private float maxEmissionRate = 8f;

    private readonly Gradient _trailGradient = new Gradient();

    private void Awake()
    {
        if (orbParticles == null)
        {
            orbParticles = GetComponentInChildren<ParticleSystem>(true);
        }

        if (orbTrail == null)
        {
            orbTrail = GetComponentInChildren<TrailRenderer>(true);
        }

        if (orbParticles == null)
        {
            Debug.LogError("Energy orb VFX requires a Particle System reference.", this);
        }

        if (orbTrail == null)
        {
            Debug.LogError("Energy orb VFX requires a Trail Renderer reference.", this);
        }
    }

    private void OnEnable()
    {
        if (orbParticles != null && !orbParticles.isPlaying)
        {
            orbParticles.Play();
        }
    }

    public void SetOrbVisuals(Color orbColor, float orbDiameter, float normalizedEnergy)
    {
        float progress = Mathf.Clamp01(normalizedEnergy);

        if (orbParticles != null)
        {
            ParticleSystem.EmissionModule emission = orbParticles.emission;
            emission.rateOverTimeMultiplier = Mathf.Lerp(minEmissionRate, maxEmissionRate, progress);
        }

        if (orbTrail != null)
        {
            Color trailColor = new Color(orbColor.r, orbColor.g, orbColor.b, 1f);
            _trailGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(trailColor, 0f),
                    new GradientColorKey(trailColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(Mathf.Clamp01(orbColor.a), 0f),
                    new GradientAlphaKey(0f, 1f)
                });

            orbTrail.colorGradient = _trailGradient;
            orbTrail.widthMultiplier = Mathf.Max(0f, orbDiameter);
        }
    }
}
