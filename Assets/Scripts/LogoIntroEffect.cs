using UnityEngine;
using System.Collections;

public class LogoIntroEffect : MonoBehaviour
{
    [Header("Referencias")]
    public SpriteRenderer spriteRenderer;
    public ParticleSystem dustParticles;

    [Header("Configuración de Tiempo")]
    public float fadeDuration = 2.0f;
    public float startDelay = 0.5f;

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0; 
            spriteRenderer.color = c;
        }

        if (dustParticles != null)
        {
            dustParticles.Play();
        }

        StartCoroutine(AnimateLogoFadeIn());
    }

    IEnumerator AnimateLogoFadeIn()
    {
        yield return new WaitForSeconds(startDelay);

        float elapsedTime = 0f;
        Color color = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration); 
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1f; // <-- Y aquí
        spriteRenderer.color = color;
    }
}
