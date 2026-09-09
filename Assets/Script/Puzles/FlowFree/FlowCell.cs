using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlowCell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
{
    public int x;
    public int y;

    public bool occupied;

    public bool isDot;

    public FlowColor flowColor = FlowColor.None;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(int newX, int newY)
    {
        x = newX;
        y = newY;

        name = $"Cell {x} {y}";
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void ResetCell()
    {
        if (isDot) return;

        occupied = false;
        flowColor = FlowColor.None;
        image.color = Color.white;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        FlowInput.Instance.PointerDown(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FlowInput.Instance.PointerEnter(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        FlowInput.Instance.PointerUp();
    }
}