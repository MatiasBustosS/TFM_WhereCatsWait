using UnityEngine;
using UnityEngine.EventSystems;

public class ControlHazLuz : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public GameObject objetoHazLuz;

    void Start()
    {
        if (objetoHazLuz != null)
        {
            objetoHazLuz.SetActive(false);
        }
    }

    //Uso del Mouse
    public void OnPointerEnter(PointerEventData eventData)
    {
        EncenderLuz();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApagarLuz();
    }

    //Uso de teclado/mando
    public void OnSelect(BaseEventData eventData)
    {
        EncenderLuz();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ApagarLuz();
    }

    //Metodos
    void EncenderLuz()
    {
        if (objetoHazLuz != null) objetoHazLuz.SetActive(true);
    }

    void ApagarLuz()
    {
        if (objetoHazLuz != null) objetoHazLuz.SetActive(false);
    }
}
