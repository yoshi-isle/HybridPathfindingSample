using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactableName;
    public string hoverText = "Default Interact Text";
    public Transform interactionTransform;
    public int interactionRadius = 1;

    public virtual void Interact(GameObject interactor)
    {
        Debug.Log($"{interactor.name} interacted with {gameObject.name}");
    }
}