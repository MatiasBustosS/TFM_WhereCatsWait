using UnityEngine;

public class CreditosScrollFade : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform contenedorCreditos;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Movimiento y Opacidad")]
    [SerializeField] private float velocidadSubida = 40f; 
    [SerializeField] private float posicionYInicial = -500f;
    [SerializeField] private float posicionYFadeCompleto = -100f; 
    [SerializeField] private float posicionYFinal = 900f;

    private Vector2 posicionActual;

    void Start()
    {
        if (contenedorCreditos == null)
            contenedorCreditos = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        posicionActual = contenedorCreditos.anchoredPosition;
        posicionActual.y = posicionYInicial;
        contenedorCreditos.anchoredPosition = posicionActual;

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (contenedorCreditos == null) return;

  
        posicionActual.y += velocidadSubida * Time.deltaTime;
        contenedorCreditos.anchoredPosition = posicionActual;

    
        if (canvasGroup != null)
        {
            float progresoAlpha = Mathf.InverseLerp(posicionYInicial, posicionYFadeCompleto, posicionActual.y);
            canvasGroup.alpha = Mathf.Clamp01(progresoAlpha);
        }

      
        if (posicionActual.y >= posicionYFinal)
        {
            enabled = false;
        }
    }
}
