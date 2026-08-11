using System;
using UnityEngine;

public class PointOfInterest : MonoBehaviour
{
    [SerializeField] private PlayerTrigger trigger;

    public event Action<PointOfInterest, PlayerView> OnEnterPOI;
    public event Action<PointOfInterest, PlayerView> OnExitPOI;

    private void OnEnable()
    {
        trigger.TriggerEnter += ProcessTriggerEnter;
        trigger.TriggerExit += ProcessTriggerExit;
    }

    private void ProcessTriggerExit(PlayerView view)
    {
        OnExitPOI?.Invoke(this, view);
    }

    private void ProcessTriggerEnter(PlayerView view)
    {
        OnEnterPOI?.Invoke(this, view);
    }

    private void OnDisable()
    {
        trigger.TriggerEnter -= ProcessTriggerEnter;
        trigger.TriggerExit -= ProcessTriggerExit;
    }
}
