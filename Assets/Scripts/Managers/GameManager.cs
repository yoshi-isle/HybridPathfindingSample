using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public event Action OnTick;
    void Awake()
    {
        instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            yield return new WaitForSeconds(0.4f);
        }
    }

}
