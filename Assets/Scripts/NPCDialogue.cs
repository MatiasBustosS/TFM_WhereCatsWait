using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Dialogue
{
    [TextArea(2, 5)] [SerializeField] private string[] dialogue;
    [SerializeField] private bool collect;
    [SerializeField] private bool endPuzle = false;
    [SerializeField] private bool finishLevel = false;
    
    public string[] _Dialogue => dialogue;
    public bool EndPuzle => endPuzle;
    public bool Collect => collect;
    public bool FinishLevel => finishLevel;
}

public class NPCDialogue : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject PressE;
    
    [Header("NPC Data")]
    [SerializeField] private string characterName;
    [SerializeField] private Sprite characterFace;
    [SerializeField] private AudioClip voice;
    
    [SerializeField] private Dialogue[] dialogues;
    [SerializeField] private DialogueManager dialogueManager;
    
    private int actualDialogue = 0;

    public void Talk()
    {
        dialogueManager.StartDialogue(characterName, dialogues[actualDialogue], voice, characterFace);
    }

    public void NextDialogue()
    {
        actualDialogue++;
        if (actualDialogue >= dialogues.Length)
        {
            actualDialogue = dialogues.Length -1;
            GameManager.instance.SetNpc(null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PressE.SetActive(true);
            other.gameObject.GetComponent<RailPlayerMovement>().SetNPC(this);
        }
    }
    

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PressE.SetActive(false);
            other.gameObject.GetComponent<RailPlayerMovement>().SetNPC(null);
        }
    }

}