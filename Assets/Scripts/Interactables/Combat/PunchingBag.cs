using UnityEngine;

public class PunchingBag : Attackable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        Debug.Log($"{interactor.name} punched the {gameObject.name}");
    }
}