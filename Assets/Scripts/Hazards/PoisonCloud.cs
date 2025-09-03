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
        Vector3 worldPosition = transform.position + Vector3.up * 2f;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        if (playerIsInCloud)
        {
            string labelText = "You are in a poison cloud! Taking damage...";
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(labelText));
            float padding = 16f;
            float width = size.x + padding;
            float height = size.y + padding / 2;
            float x = screenPosition.x - width / 2;
            float y = Screen.height - screenPosition.y - height / 2;
            GUI.Box(new Rect(x, y, width, height), labelText);
        }
        else
        {
            string labelText = "If you come in here you'll take damage...";
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(labelText));
            float padding = 16f;
            float width = size.x + padding;
            float height = size.y + padding / 2;
            float x = screenPosition.x - width / 2;
            float y = Screen.height - screenPosition.y - height / 2;
            GUI.Box(new Rect(x, y, width, height), labelText);
        }
    }
}
