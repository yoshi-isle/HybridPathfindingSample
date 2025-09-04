using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcNameText, dialogueText;
    private List<string> dialogueToDisplay = new();
    private string npcNameToDisplay;
    private int currentLineIndex = 0;

    void Start()
    {
        dialoguePanel.SetActive(false);
        GameManager.instance.OnDialogueDisplay += StartDialogue;
        GameManager.instance.OnPathNextTile += (Vector3 tile) => CloseDialogue();
    }

    private void StartDialogue(string npcName, List<string> list)
    {
        StopAllCoroutines();
        npcNameToDisplay = npcName;
        dialogueToDisplay = list;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        StartCoroutine(DrawTextInSequence(dialogueToDisplay[currentLineIndex]));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && dialoguePanel.activeInHierarchy)
        {
            NextDialogueLine();
        }
    }

    private void NextDialogueLine()
    {
        StopAllCoroutines();
        npcNameText.text = string.Empty;
        if (currentLineIndex < dialogueToDisplay.Count - 1)
        {
            currentLineIndex++;
            StartCoroutine(DrawTextInSequence(dialogueToDisplay[currentLineIndex]));
        }
        else
        {
            CloseDialogue();
        }
    }

    private IEnumerator DrawTextInSequence(string line)
    {
        npcNameText.text = npcNameToDisplay;
        dialogueText.text = "";
        foreach (var c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.02f);
        }
    }

    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}