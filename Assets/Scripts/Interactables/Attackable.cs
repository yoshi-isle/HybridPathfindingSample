using System.Security.Cryptography;
using UnityEngine;

public class Attackable : Interactable
{
    public void ReceiveDamage(int damage)
    {
        Debug.Log($"I, {gameObject.name}, received {damage} damage.");
    }
}