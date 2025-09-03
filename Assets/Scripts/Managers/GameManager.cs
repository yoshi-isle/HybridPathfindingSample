using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_4 = new(0.4f);
    public static GameManager instance;
    public event Action OnTick;
    public event Action<Vector3> OnPathNextTile;
    public void TriggerOnPathNextTile(Vector3 tile)
    {
        OnPathNextTile?.Invoke(tile);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(TickCounterEvent());
    }

    private IEnumerator TickCounterEvent()
    {
        while (true)
        {
            print("Tick! From GameManager");
            OnTick?.Invoke();
            yield return _waitForSeconds0_4;
        }
    }
}
