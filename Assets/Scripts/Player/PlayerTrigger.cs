using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    public event Action<PlayerView> TriggerEnter;
    public event Action<PlayerView> TriggerExit;

    private List<Transform> triggers;

    public List<Transform> Triggers => triggers;

    private void Awake()
    {
        triggers = new List<Transform>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerView>(out var view) && other.transform != transform.parent)
        {
            TriggerEnter?.Invoke(view);
            AddTrigger(view);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerView>(out var view) && other.transform != transform.parent)
        {
            TriggerExit?.Invoke(view);
            RemoveTrigger(view);
        }
    }

    private void AddTrigger(PlayerView view)
    {
        if (triggers.Contains(view.transform))
            return;

        triggers.Add(view.transform);
    }

    private void RemoveTrigger(PlayerView view)
    {
        if(triggers.Contains(view.transform))
        {
            triggers.Remove(view.transform);
        }
    }
}
