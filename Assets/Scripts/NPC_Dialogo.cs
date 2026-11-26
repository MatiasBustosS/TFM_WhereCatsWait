using System.Collections;
using UnityEngine;
using TMPro; 

public class NPC_Dialogo : MonoBehaviour
{
    [Header("Contenido del Diálogo")]
    [SerializeField, TextArea(2, 5)] private string[] frases;
    [SerializeField] private float tiempoEntreLetras = 0.05f;

    [Header("Conexiones de Interfaz")]
    [SerializeField] private GameObject cuadroDialogo;
    [SerializeField] private TextMeshProUGUI textoDialogo;

    private bool hablando = false;
    private int indiceActual = -1;

    public static bool dialogoActivo = false;

    public void Interactuar()
    {
        cuadroDialogo.SetActive(true);
        dialogoActivo = true;

        if (!hablando)
        {
            SiguienteFrase();
        }
        else
        {
            CompletarFrase();
        }
    }

    private void SiguienteFrase()
    {
        indiceActual++;

        if (indiceActual >= frases.Length)
        {
            TerminarDialogo();
        }
        else
        {
            hablando = true;
            textoDialogo.text = "";
            StartCoroutine(EscribirFrase());
        }
    }

    private IEnumerator EscribirFrase()
    {
        char[] caracteres = frases[indiceActual].ToCharArray();

        foreach (char letra in caracteres)
        {
            textoDialogo.text += letra;
            yield return new WaitForSeconds(tiempoEntreLetras);
        }

        hablando = false;
    }

    private void CompletarFrase()
    {
        StopAllCoroutines();
        textoDialogo.text = frases[indiceActual];
        hablando = false;
    }

    private void TerminarDialogo()
    {
        hablando = false;
        cuadroDialogo.SetActive(false);
        textoDialogo.text = "";
        indiceActual = -1;

        dialogoActivo = false; 
    }
}
