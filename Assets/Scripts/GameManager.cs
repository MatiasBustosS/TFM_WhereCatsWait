using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    
    [SerializeField] private ShaderPosition shaderWorldPosition;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private HudManager hudManager;
    [SerializeField] private RailPlayerMovement playerMovement;
    [SerializeField] private EnergyOrbController energyOrbController; 
    
    [Header("Victory")]
    [SerializeField] private bool Victory;

    private NPCDialogue activeNpc;
    
    
    private bool isTalking = false;
    private bool ShaderSize = false;
    
    
    public DialogueManager DialogueManager => dialogueManager;
    public HudManager HudManager => hudManager;
    public RailPlayerMovement RailPlayerMovement => playerMovement;
    public bool IsTalking => isTalking;
    public NPCDialogue ActiveNpc => activeNpc;
    
    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        if (Victory) StartCoroutine(BuildVictory());
    }

    private void Update()
    {
        if(ShaderSize) shaderWorldPosition.ChangeSize();
    }
    
    public void ChangeTalking(bool talking)
    {
        isTalking = talking;
    }

    public void SetNpc(NPCDialogue npcDialogue)
    {
        activeNpc = npcDialogue;
    }

    public void ChangeWorldState(bool state)
    {
        ShaderSize = state;
        if (state) GetComponent<CameraMovement>().PlayCameraAnimation();
    }

    public void StartCollection()
    {
        energyOrbController.gameObject.SetActive(true);
        energyOrbController.SetActiveOrbs();
    }

    public void EndCollection()
    {
        energyOrbController.gameObject.SetActive(false);
    }

    private IEnumerator BuildVictory()
    {
        yield return new WaitForSeconds(3f);
        ChangeWorldState(true);
    }
}
