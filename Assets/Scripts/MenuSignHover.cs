using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MenuSignHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Objeto Letrero con el texto respectivo")]
    public GameObject signObject;

    private static MenuSignHover activeOption = null;
    private static Coroutine hideCoroutine = null;
    private static MonoBehaviour globalRunner = null;

    void Start()
    {
        if (globalRunner == null)
        {
            globalRunner = this;
        }
    }

    //Control Mouse
    public void OnPointerEnter(PointerEventData eventData)
    {
        MostrarLetrero();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OcultarLetreroConRetardo();
    }

    //Control teclado/mando
    public void OnSelect(BaseEventData eventData)
    {
        MostrarLetrero();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OcultarLetreroConRetardo();
    }

    
    void MostrarLetrero()
    {
        if (activeOption != null && activeOption != this)
        {
            if (hideCoroutine != null)
            {
                globalRunner.StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            if (activeOption.signObject != null)
            {
                activeOption.signObject.SetActive(false);
            }
        }

        if (hideCoroutine != null && activeOption == this)
        {
            globalRunner.StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        activeOption = this;
        if (signObject != null)
        {
            signObject.SetActive(true);
        }
    }

    void OcultarLetreroConRetardo()
    {
        if (activeOption == this)
        {
            if (hideCoroutine != null)
            {
                globalRunner.StopCoroutine(hideCoroutine);
            }

            hideCoroutine = globalRunner.StartCoroutine(HideSignWithDelay());
        }
    }

    private IEnumerator HideSignWithDelay()
    {
        yield return new WaitForSeconds(2.0f);

        if (signObject != null)
        {
            signObject.SetActive(false);
        }

        if (activeOption == this)
        {
            activeOption = null;
        }
        hideCoroutine = null;
    }
}