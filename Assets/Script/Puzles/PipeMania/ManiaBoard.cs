using UnityEngine;

public class ManiaBoard : MonoBehaviour
{
    public static ManiaBoard Instance;

    [Header("References")]
    [SerializeField] private GameObject cellPrefab;

    public ManiaCell[,] cells;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateBoard(int width, int height)
    {
        ClearBoard();
        cells = new ManiaCell[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject obj = Instantiate(cellPrefab, transform);
                ManiaCell cell = obj.GetComponent<ManiaCell>();

                cells[x, y] = cell;
                GenerateRandomCell(cell, x, y);
            }
        }

        CreateStartAndEnd(width, height);
    }

    void GenerateRandomCell(ManiaCell cell, int x, int y)
    {
        int random = Random.Range(0, 4);
        ManiaPipeType type;
        ManiaDirection dir;

        if (random == 0)
        {
            type = ManiaPipeType.Straight;
            dir = ManiaDirection.Up | ManiaDirection.Down;
        }
        else if (random == 1)
        {
            type = ManiaPipeType.Corner;
            dir = ManiaDirection.Up | ManiaDirection.Right;
        }
        else if (random == 2)
        {
            type = ManiaPipeType.TShape;
            dir = ManiaDirection.Right | ManiaDirection.Down | ManiaDirection.Left;
        }
        else
        {
            type = ManiaPipeType.Cross;
            dir = ManiaDirection.Up | ManiaDirection.Right |  ManiaDirection.Down | ManiaDirection.Left;
        }

        cell.Setup(x, y, type, dir);
        int rotations = Random.Range(0, 4);

        //for (int i = 0; i < rotations; i++) 
            //cell.RotatePiece();
        
    }

    void CreateStartAndEnd(int width, int height)
    {
        ManiaCell start = cells[0, 0];
        start.isStart = true;
        start.pipeType = ManiaPipeType.Start;
        start.connections = ManiaDirection.Right;
        start.transform.rotation = Quaternion.identity;
        start.UpdateVisual();

        ManiaCell end = cells[width - 1, height - 1];
        end.isEnd = true;
        end.pipeType = ManiaPipeType.End;
        end.connections = ManiaDirection.Left;
        end.transform.rotation = Quaternion.identity;
        end.UpdateVisual();
    }

    public void ClearBoard()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
        
    }
}