using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePipeMania : PuzzleManager
{
    public override PuzzleType puzzleType => PuzzleType.PipeMania;
    public override bool solved { get; set; } =  false;


    [SerializeField] private Vector2Int size;
    [SerializeField] private Vector2Int[] solutionPath;

    private ManiaBoard board;
    
    public override void Initialize()
    {
        HudManager.Instance.SetPuzzleManager(this);
        HudManager.Instance.pipeManiaBoard.GenerateBoard(size.x, size.y);
    }

    public override void CheckWin()
    {
        ManiaCell start = null;
        board = HudManager.Instance.pipeManiaBoard;

        foreach (ManiaCell cell in board.cells)
        {
            if (cell.isStart)
            {
                start = cell;
                break;
            }
        }

        if (start == null) return;

        HashSet<ManiaCell> visited = new();

        bool connected = Search(start, visited);

        if (connected)
        {
            solved = true;
            OpenDoor();
        }
    }
    bool Search(ManiaCell start, HashSet<ManiaCell> visited)
    {
        Queue<ManiaCell> queue = new Queue<ManiaCell>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            ManiaCell current = queue.Dequeue();
            if (current.isEnd) return true;

            foreach (ManiaDirection dir in _directions)
            {
                if (!current.connections.HasFlag(dir)) continue;

                Vector2Int offset = DirectionToVector(dir);
                int nx = current.x + offset.x;
                int ny = current.y + offset.y;

                if (!IsInside(nx, ny)) continue;

                ManiaCell next = board.cells[nx, ny];
                if (visited.Contains(next)) continue;

                ManiaDirection opposite = GetOpposite(dir);
                if (!next.connections.HasFlag(opposite)) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return false;
    }

    static readonly ManiaDirection[] _directions =
    {
        ManiaDirection.Up,
        ManiaDirection.Right,
        ManiaDirection.Down,
        ManiaDirection.Left
    };

    bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < size.x && y < size.y;
    }

    Vector2Int DirectionToVector(ManiaDirection dir)
    {
        switch (dir)
        {
            case ManiaDirection.Up: return Vector2Int.down;
            case ManiaDirection.Right: return Vector2Int.right;
            case ManiaDirection.Down: return Vector2Int.up;
            case ManiaDirection.Left: return Vector2Int.left;
        }
        return Vector2Int.zero;
    }

    ManiaDirection GetOpposite(ManiaDirection dir)
    {
        switch (dir)
        {
            case ManiaDirection.Up: return ManiaDirection.Down;
            case ManiaDirection.Right: return ManiaDirection.Left;
            case ManiaDirection.Down: return ManiaDirection.Up;
            case ManiaDirection.Left: return ManiaDirection.Right;
        }

        return ManiaDirection.None;
    }
}
