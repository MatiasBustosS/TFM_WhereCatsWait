using System.Collections.Generic;
using UnityEngine;

public class FlowInput : MonoBehaviour
{
    public static FlowInput Instance;

    private bool dragging;

    private FlowColor currentColor;

    private List<FlowCell> currentPath = new();

    private void Awake()
    {
        Instance = this;
    }

    public void PointerDown(FlowCell cell)
    {
        if (!cell.isDot) return;

        ClearColor(cell.flowColor);

        dragging = true;
        currentColor = cell.flowColor;
        
        currentPath.Clear();
        currentPath.Add(cell);
    }

    public void PointerEnter(FlowCell cell)
    {
        if (!dragging) return;

        FlowCell lastCell = currentPath[^1];

        if (cell == lastCell) return;
        if (!IsNeighbour(lastCell, cell)) return;

        if (currentPath.Contains(cell))
        {
            Backtrack(cell);
            return;
        }

        if (cell.occupied && !cell.isDot) return;
        if (cell.flowColor != FlowColor.None && cell.flowColor != currentColor) return;
        if (cell.isDot && cell.flowColor == currentColor)
        {
            AddCell(cell);
            dragging = false;
            return;
        }

        AddCell(cell);
    }

    public void PointerUp()
    {
        dragging = false;
        HudManager.Instance.CheckWin();
    }

    void AddCell(FlowCell cell)
    {
        cell.occupied = true;
        cell.flowColor = currentColor;
        
        cell.SetColor(FlowBoard.Instance.GetUnityColor(currentColor));
        currentPath.Add(cell);
    }

    void Backtrack(FlowCell cell)
    {
        int index = currentPath.IndexOf(cell);

        for (int i = currentPath.Count - 1; i > index; i--)
        {
            currentPath[i].ResetCell();
            currentPath.RemoveAt(i);
        }
    }

    void ClearColor(FlowColor color)
    {
        foreach (FlowCell cell in FlowBoard.Instance.cells)
        {
            if (cell.isDot) continue;
            if (cell.flowColor == color) cell.ResetCell();
        }
    }

    bool IsNeighbour(FlowCell a, FlowCell b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return dx + dy == 1;
    }
}