using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance;

    [Header("GameObjects")]
    [SerializeField] private Transform aim;
    [SerializeField] private Transform Slider;
    [SerializeField] private Transform PipeMania;
    [SerializeField] private Transform FlowFree;
    [SerializeField] private Transform CirclePuzzle;

    [Header("Puzzles UI")]
    [SerializeField] private CircleBoard circlePuzzle;
    [SerializeField] private FlowBoard flowFreePuzzle;
    [SerializeField] private ManiaBoard pipeManiaPuzzle;
    
    [Header("Others")]
    [SerializeField] private GameObject win;
    [SerializeField] private GameObject lose;
    [SerializeField] private Animator fade;

    private bool isWin = false;
    private bool isLose = false; 

    public CircleBoard circleBoard => circlePuzzle;
    public FlowBoard flowBoard => flowFreePuzzle;
    public ManiaBoard pipeManiaBoard => pipeManiaPuzzle;
    
    private PuzzleManager puzzleManager;
    
    private bool isTarget = false;
    
    public bool IsTarget => isTarget;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void SetPuzzleManager(PuzzleManager manager)
    {
        puzzleManager = manager;
    }
    
    public void OpenPuzzle(PuzzleType getType)
    {
        if(isTarget) return;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        aim.gameObject.SetActive(false);
        
        isTarget = true;
        
        print(getType.ToString());
        switch (getType)
        {
            case PuzzleType.Slider:
                Slider.gameObject.SetActive(true);
                break;
            case PuzzleType.PipeMania:
                PipeMania.gameObject.SetActive(true);
                break;
            case PuzzleType.FlowFree:
                FlowFree.gameObject.SetActive(true);
                break;
            case PuzzleType.CirclePuzzle:
                CirclePuzzle.gameObject.SetActive(true);
                break;
            
            default:
                break;
        }
    }

    public void CheckWin()
    {
        puzzleManager.CheckWin();
    }
    
    public void ClosePuzzle()
    {
        isTarget = false;
        aim.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Win()
    {
        if(isLose) return;
        isWin = true;
        FadeOut();
        win.SetActive(true);
    }

    public void Lose()
    {
        if(isWin)  return;
        isLose = true;
        FadeOut();
        lose.SetActive(true);
    }
    
    private void FadeOut()
    {
        fade.SetTrigger("FadeOut");
    }
}
