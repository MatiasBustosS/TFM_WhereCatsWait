using UnityEngine;

public enum FlowColor
{
    None,
    Red,
    Blue,
    Green,
    Yellow
}

public class FlowBoard : MonoBehaviour
{
    public static FlowBoard Instance;


    public GameObject cellPrefab;

    public FlowCell[,] cells;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateBoard(int w,  int h)
    {
        cells = new FlowCell[w, h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                GameObject obj = Instantiate(cellPrefab, transform);
                FlowCell cell = obj.GetComponent<FlowCell>();
                
                cell.Setup(x, y);
                cells[x, y] = cell;
            }
        }
    }
    
    public void SetDot(int x, int y, FlowColor color)
    {
        FlowCell cell = cells[x, y];
        
        cell.isDot = true;
        cell.occupied = true;
        cell.flowColor = color;
        
        cell.SetColor(GetUnityColor(color));
    }

    public Color GetUnityColor(FlowColor color)
    {
        switch (color)
        {
            case FlowColor.Red: return Color.red;
            case FlowColor.Blue: return Color.blue;
            case FlowColor.Green: return Color.green;
            case FlowColor.Yellow: return Color.yellow;
            
            default: return Color.white;
        }
    }

    public void Clear()
    {
        cells = new FlowCell[0,0];
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        cells = null;
    }
}