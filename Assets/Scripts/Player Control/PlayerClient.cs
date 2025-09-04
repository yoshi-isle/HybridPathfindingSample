using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerClient : MonoBehaviour
{
    Unit playerUnit;
    Vector3 hoveredLocation = Vector3.zero;
    public GameObject hoveredLocationCubePrefab;
    GameObject hoveredLocationCube;
    public int HitPoints = 100;
    Interactable hoveredInteractable;
    private Interactable targetedInteractable;

    void Start()
    {
        playerUnit = GetComponent<Unit>();
        GameManager.instance.OnTick += HandleTick;
        hoveredLocationCube = Instantiate(hoveredLocationCubePrefab, hoveredLocation, Quaternion.identity);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        int groundLayerMask = 1 << LayerMask.NameToLayer("Walkable");
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            hoveredLocation = hit.point;
        }
        hoveredLocation = new Vector3(Mathf.Round(hoveredLocation.x), Mathf.Round(hoveredLocation.y), Mathf.Round(hoveredLocation.z));
        hoveredLocationCube.transform.position = hoveredLocation;

        int interactableLayerMask = 1 << LayerMask.NameToLayer("Interactable");
        if (Physics.Raycast(ray, out RaycastHit interactableHit, Mathf.Infinity, interactableLayerMask))
        {
            hoveredInteractable = interactableHit.collider.GetComponent<Interactable>();
        }
        else
        {
            hoveredInteractable = null;
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (hoveredInteractable != null)
            {
                targetedInteractable = hoveredInteractable;
                playerUnit.SetDestination(targetedInteractable.interactionTransform.position);
            }
            else
            {
                playerUnit.SetDestination(hoveredLocation);
            }
        }
    }
    
    private void HandleTick()
    {
        if (targetedInteractable != null)
        {
            float distanceToTarget = Vector3.Distance(playerUnit.transform.position, targetedInteractable.interactionTransform.position);
            if (distanceToTarget <= targetedInteractable.interactionRadius + 0.1f)
            {
                Console.WriteLine("woah");
                targetedInteractable.Interact(gameObject);
                targetedInteractable = null;
                //cancel the path
                playerUnit.ClearPath();
                return;
            }
        }
        playerUnit.MoveAlongPath();
    }

    public void ReceiveEnvironmentalDamage(int damageAmount)
    {
        GameManager.instance.TriggerOnHitpointsDepleted(HitPoints, damageAmount);
        HitPoints -= (int)damageAmount;
    }

    void OnGUI()
    {
        string labelText = $"{transform.position.x:F2}, {transform.position.z:F2}\nInteract: {(hoveredInteractable != null ? hoveredInteractable.hoverText : "None")}\nHP: {HitPoints}";
        Rect boxRect = DisplayInfobox.DisplayInfoBox(transform.position, labelText);
        GUI.Box(boxRect, labelText);
    }
}
