using System.Collections.Generic;
using UnityEngine;

public class DialogueNPC : Interactable
{
    public List<string> dialogueLines = new List<string>
    {
        "Hello, traveler!",
        "This is just some sample dialogue.",
        "Be careful out there!",
        "Maybe this game will have mechanics one day."
    };

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        DialogueManager.Instance.StartDialogue(interactableName, dialogueLines);
    }
}