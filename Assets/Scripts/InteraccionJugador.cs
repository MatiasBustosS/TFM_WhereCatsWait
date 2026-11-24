using UnityEngine;

public class InteraccionJugador : MonoBehaviour
{
    private NPC_Dialogo npcCercano;

    void Update()
    {

        if (npcCercano != null && Input.GetKeyDown(KeyCode.E))
        {
            npcCercano.Interactuar();
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("NPC"))
        {
            npcCercano = other.GetComponent<NPC_Dialogo>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            npcCercano = null; 
        }
    }
}