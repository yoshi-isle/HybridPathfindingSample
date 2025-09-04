using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    private List<string> dialogueLines;
    private string npcName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(string npcName, List<string> lines)
    {
        this.npcName = npcName;
        GameManager.instance.TriggerOnDialogueDisplay(npcName, lines);
    }
}