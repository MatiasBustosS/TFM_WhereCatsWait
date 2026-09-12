using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public enum ManiaPipeType
{
    Empty,
    Straight,
    Corner,
    TShape,
    Cross,
    Start,
    End
}

[Flags]
public enum ManiaDirection
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 4,
    Left = 8
}

public class ManiaCell : MonoBehaviour, IPointerClickHandler
{
    [Header("Grid")]
    public int x;
    public int y;

    [Header("Pipe")]
    public ManiaPipeType pipeType;

    public ManiaDirection connections;
    private ManiaDirection originalConnections;

    public int rotation;

    [Header("State")]
    public bool isStart;
    public bool isEnd;

    private Image pipeImage;
    
    [SerializeField] private Sprite straightSprite;
    [SerializeField] private Sprite cornerSprite;
    [SerializeField] private Sprite tSprite;
    [SerializeField] private Sprite crossSprite;
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite endSprite;
    
    private void Awake()
    {
        pipeImage = GetComponent<Image>();
    }

    public void Setup(int newX, int newY, ManiaPipeType type, ManiaDirection dirs)
    {
        x = newX;
        y = newY;

        pipeType = type;
        connections = dirs;
        rotation = 0;
        UpdateVisual();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isStart || isEnd) return;

        RotatePiece();
        
        HudManager.Instance.CheckWin();
    }

    public void RotatePiece()
    {
        if (isStart || isEnd) return;
        
        rotation++;
        if (rotation >= 4) rotation = 0;
        
        originalConnections = connections;
        
        connections = RotateDirection(originalConnections);
        
        
        transform.Rotate(0, 0, -90);
    }

    ManiaDirection RotateDirection(ManiaDirection dir)
    {
        ManiaDirection newDir = ManiaDirection.None;

        if (dir.HasFlag(ManiaDirection.Up)) newDir |= ManiaDirection.Right;
        if (dir.HasFlag(ManiaDirection.Right)) newDir |= ManiaDirection.Down;
        if (dir.HasFlag(ManiaDirection.Down)) newDir |= ManiaDirection.Left;
        if (dir.HasFlag(ManiaDirection.Left)) newDir |= ManiaDirection.Up;

        return newDir;
    }
    
    
    public void UpdateVisual()
    {
        switch(pipeType)
        {
            case ManiaPipeType.Straight:
                pipeImage.sprite = straightSprite;
                break;
            case ManiaPipeType.Corner:
                pipeImage.sprite = cornerSprite;
                break;
            case ManiaPipeType.TShape:
                pipeImage.sprite = tSprite;
                break;
            case ManiaPipeType.Cross:
                pipeImage.sprite = crossSprite;
                break;
            case ManiaPipeType.Start:
                pipeImage.sprite = startSprite;
                break;
            case ManiaPipeType.End:
                pipeImage.sprite = endSprite;
                break;
        }
    }
    
}