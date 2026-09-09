using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCircle : PuzzleManager
{
    public override PuzzleType puzzleType => PuzzleType.CirclePuzzle;
    
    private DiskPuzzle discA;
    private DiskPuzzle discB;
    private DiskPuzzle discC;
    
    [SerializeField] private Vector3Int initialPosition;
    [SerializeField] private Vector3Int solution;
    public override bool solved { get; set; } =  false;


    private void Start()
    {
        discA = HudManager.Instance.circleBoard.DiskA;
        discB = HudManager.Instance.circleBoard.DiskB;
        discC = HudManager.Instance.circleBoard.DiskC;
    }

    public override void Initialize()
    {
        HudManager.Instance.SetPuzzleManager(this);
        
        discA.SetInitialPos(initialPosition.x);
        discB.SetInitialPos(initialPosition.y);
        discC.SetInitialPos(initialPosition.z);
    }
    
    public override void CheckWin()
    {
        if (solved) return;

        if(!discA.IsAtIndex(solution.x)) return;
        if(!discB.IsAtIndex(solution.y)) return;
        if(!discC.IsAtIndex(solution.z)) return;
            
        solved = true;
        OpenDoor();
    }
}
