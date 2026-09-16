using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuzzleType
{
    Slider,
    FlowFree,
    PipeMania,
    CirclePuzzle
    
}

public abstract class PuzzleManager : MonoBehaviour
{
    public abstract PuzzleType puzzleType { get; }
    public abstract bool solved { get; set; }
    public abstract void Initialize();
    public abstract void CheckWin();

    protected void CompletePuzzle()
    {
        HudManager.Instance.ClosePuzzle();
    }
}
