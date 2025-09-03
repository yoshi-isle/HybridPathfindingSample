using System;
using TMPro;
using UnityEngine;

public class DisplayHitpoints : MonoBehaviour
{
    private int currentHitpoints = 100;
    private TextMeshProUGUI hitpointsText;
    private Animator animator;

    void Start()
    {
        hitpointsText = GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        GameManager.instance.OnHitpointsDepleted += UpdateHitpoints;
    }

    private void UpdateHitpoints(int currentHitpoints, int damageAmount)
    {
        animator.SetTrigger("Shake");
        this.currentHitpoints = currentHitpoints - damageAmount;
        hitpointsText.text = this.currentHitpoints.ToString();
    }

    void ShakeComplete()
    {
        animator.SetTrigger("NotShake");
    }
}