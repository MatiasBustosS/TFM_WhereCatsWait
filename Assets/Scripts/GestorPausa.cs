using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GestorPausa : MonoBehaviour
{
    [Header("Paneles del Menú")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelOpciones;

    [Header("Navegación por Teclado")]
    [SerializeField] private GameObject primerBotonPausa;    
    [SerializeField] private GameObject primerBotonOpciones; 

    private bool juegoPausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);
        panelOpciones.SetActive(false);
        Time.timeScale = 0f;
        juegoPausado = true;

        
        if (primerBotonPausa != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primerBotonPausa);
        }
    }

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        panelOpciones.SetActive(false);
        Time.timeScale = 1f;
        juegoPausado = false;

        
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AbrirOpciones()
    {
        panelPausa.SetActive(false);
        panelOpciones.SetActive(true);

        
        if (primerBotonOpciones != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primerBotonOpciones);
        }
    }

    public void CerrarOpciones()
    {
        panelOpciones.SetActive(false);
        panelPausa.SetActive(true);

        
        if (primerBotonPausa != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primerBotonPausa);
        }
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void IrACreditos()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Creditos");
    }
}