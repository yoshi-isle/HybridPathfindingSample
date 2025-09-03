using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_4 = new(0.4f);
    public static GameManager instance;
    public event Action OnTick;
    public event Action<Vector3> OnPathNextTile;
    public event Action<int, int> OnHitpointsDepleted;
    public void TriggerOnPathNextTile(Vector3 tile)
    {
        OnPathNextTile?.Invoke(tile);
    }
    public void TriggerOnHitpointsDepleted(int currentHitpoints, int damage)
    {
        OnHitpointsDepleted?.Invoke(currentHitpoints, damage);
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
