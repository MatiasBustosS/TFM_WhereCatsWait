using UnityEngine;
using System;
using System.Collections.Generic;

public class PuzzleFlowFree : PuzzleManager
{
    public override PuzzleType puzzleType => PuzzleType.FlowFree;

    [SerializeField] private Vector2Int size;
    [SerializeField] private List<Road> colorsRoads;

    public override bool solved { get; set; } =  false;
    public override void Initialize()
    {
        HudManager.Instance.SetPuzzleManager(this);
        
        HudManager.Instance.flowBoard.GenerateBoard(size.x, size.y);

        foreach (Road road in colorsRoads)
        {
            HudManager.Instance.flowBoard.SetDot(road.initial.x, road.initial.y, road.color);
            HudManager.Instance.flowBoard.SetDot(road.final.x, road.final.y, road.color);
        }
    }
    
    public override void CheckWin()
    {
        foreach (FlowCell cell in HudManager.Instance.flowBoard.cells)
        {
            if (!cell.isDot && !cell.occupied)
            {
                return;
            }
        }

        solved = true;
        OpenDoor();
    }
    
}

[Serializable]
public class Road
{
    public FlowColor color;
    public Vector2Int initial;
    public Vector2Int final;
}