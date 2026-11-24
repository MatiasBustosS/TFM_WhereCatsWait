using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonBrilloUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Material materialBrillo;
    private Material materialOriginal;
    private Image imagenGato;

    void Start()
    {
        imagenGato = GetComponent<Image>();
        materialOriginal = imagenGato.material;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (materialBrillo != null) imagenGato.material = materialBrillo;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (materialOriginal != null) imagenGato.material = materialOriginal;
    }
}
