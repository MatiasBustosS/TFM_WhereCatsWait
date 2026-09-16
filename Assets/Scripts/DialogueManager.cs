using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text characterName;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.03f;
    
    [SerializeField] private AudioSource dialogueAudioSource;
    private AudioClip currentVoiceClip;

    private Dialogue currentDialogue;
    private int currentIndex;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool isDialogueActive;

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isDialogueActive)
            return;

    }

    public void StartDialogue(string chName, Dialogue dialogue, AudioClip voice)
    {
        characterName.text = chName;
        currentDialogue = dialogue;
        currentIndex = 0;
        isDialogueActive = true;
        currentVoiceClip = voice;
        
        dialoguePanel.SetActive(true);

        ShowDialogue();
    }

    public void NextDialogue()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueText.text = currentDialogue._Dialogue[currentIndex];
            isTyping = false;

            return;
        }

        currentIndex++;

        if (currentIndex >= currentDialogue._Dialogue.Length)
        {
            EndDialogue();
            return;
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentDialogue._Dialogue[currentIndex]));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char character in text)
        {
            dialogueText.text += character;
            if (!char.IsWhiteSpace(character))
            {
                dialogueAudioSource.pitch = Random.Range(0.85f, 1.15f);
                dialogueAudioSource.PlayOneShot(currentVoiceClip);
            }
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false;
        dialogueText.text = "";

        if (currentDialogue.Collect) GameManager.instance.StartCollection();
        else GameManager.instance.EndCollection();
        

        if (!currentDialogue.EndPuzle) GameManager.instance.ChangeTalking(false);
        else StartCoroutine(Fade());
        
        if(currentDialogue.FinishLevel) StartCoroutine(HudManager.Instance._FadeController.backMenu());
        
        GameManager.instance.RailPlayerMovement.SetCanMove(true);
    }

    IEnumerator Fade()
    {
        HudManager.Instance._FadeController.FadeOut();
        yield return new WaitForSeconds(0.4f);
        
        var puzzleManager = GameManager.instance.ActiveNpc.GetComponent<PuzzleManager>();
        
        if (puzzleManager != null)
        {
            if (puzzleManager.solved) yield break;
        
            HudManager.Instance.OpenPuzzle(puzzleManager.puzzleType);
            puzzleManager.Initialize();
        }
    }
}