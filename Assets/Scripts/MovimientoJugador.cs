using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Ajustes")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float fuerzaSalto = 6f;

    private Rigidbody rb;
    private bool enSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    void Update()
    {

        if (NPC_Dialogo.dialogoActivo)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }
        float movX = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector3(movX * velocidad, rb.linearVelocity.y, 0f);

        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        enSuelo = true;
    }
}
