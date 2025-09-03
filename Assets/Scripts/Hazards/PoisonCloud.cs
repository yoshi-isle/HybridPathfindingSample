using System;
using UnityEngine;

public class PoisonCloud : MonoBehaviour
{
    bool playerIsInCloud = false;
    private PlayerClient _playerClient;
    PlayerClient playerClient
    {
        get
        {
            if (_playerClient == null)
            {
                _playerClient = FindFirstObjectByType<PlayerClient>();
            }
            return _playerClient;
        }
    }

    void Start()
    {
        GameManager.instance.OnTick += HandleTick;
    }

    private void HandleTick()
    {
        Collider poisonCollider = GetComponent<Collider>();
        Collider playerCollider = playerClient.gameObject.GetComponent<Collider>();

        playerIsInCloud = false;
        if (CollisionHelpers.CheckPlayerOverlap(poisonCollider, playerCollider))
        {
            playerIsInCloud = true;
            Debug.Log("Player is in poison cloud! Taking damage.");
            playerClient.ReceiveEnvironmentalDamage(5);
        }
    }



    void OnGUI()
    {
        string labelText = playerIsInCloud ? "You are in a poison cloud! Taking damage..." : "If you come in here you'll take damage...";
        Rect boxRect = DisplayInfobox.DisplayInfoBox(transform.position, labelText);
        GUI.Box(boxRect, labelText);
    }
}
