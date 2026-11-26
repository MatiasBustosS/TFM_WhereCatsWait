using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [Header("Nombre de la Escena de Juego")]
    [SerializeField] private string nombreNivel = "SampleScene";

    public void ContinuarJuego()
    {
        SceneManager.LoadScene(nombreNivel);
    }

    public void NuevoJuego()
    {
        SceneManager.LoadScene(nombreNivel);
    }

    public void AbrirOpciones()
    {
        SceneManager.LoadScene("MenuOpciones");
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void IrACreditos()
    {
        SceneManager.LoadScene("Creditos");
    }

    public void SalirDelJuego()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }
}
