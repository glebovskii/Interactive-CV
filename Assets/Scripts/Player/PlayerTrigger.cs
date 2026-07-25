using System;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    public event Action<PlayerView> TriggerEnter;
    public event Action<PlayerView> TriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerView>(out var view) && other.transform != transform.parent)
        {
            TriggerEnter?.Invoke(view);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerView>(out var view) && other.transform != transform.parent)
        {
            TriggerExit?.Invoke(view);
        }
    }
}
