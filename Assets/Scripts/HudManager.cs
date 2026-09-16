using System.Collections;
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
    
    
    [Header("FadeController")]
    [SerializeField] private FadeController fadeController;

    private bool isWin = false;
    private bool isLose = false; 

    public CircleBoard circleBoard => circlePuzzle;
    public FlowBoard flowBoard => flowFreePuzzle;
    public ManiaBoard pipeManiaBoard => pipeManiaPuzzle;
    public FadeController _FadeController => fadeController;
    
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
        
        isTarget = true;
        
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
        }
    }

    public void CheckWin()
    {
        puzzleManager.CheckWin();
    }
    
    public void ClosePuzzle()
    {
        isTarget = false;
        
        

        StartCoroutine(Wait());

    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        
        Slider.gameObject.SetActive(false);
        PipeMania.gameObject.SetActive(false);
        FlowFree.gameObject.SetActive(false);
        CirclePuzzle.gameObject.SetActive(false);
        fadeController.FadeIn();
        
        yield return new WaitForSeconds(0.3f);
        
        GameManager.instance.ActiveNpc.GetComponent<Animator>().SetTrigger("Victory");
        
        yield return new WaitForSeconds(2f);
        
        GameManager.instance.ChangeWorldState(true);
    }
}
