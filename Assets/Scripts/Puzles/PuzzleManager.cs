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
    [SerializeField] private OpenDoor[] doorToOpen;
    public abstract PuzzleType puzzleType { get; }
    public abstract bool solved { get; set; }
    public abstract void Initialize();
    public abstract void CheckWin();

    protected void OpenDoor()
    {
        if (doorToOpen == null)  return;
        foreach (OpenDoor door in doorToOpen)
            door.Open();
    }
}
