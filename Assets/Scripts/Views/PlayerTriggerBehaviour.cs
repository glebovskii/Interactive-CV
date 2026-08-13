using UnityEngine;

public abstract class PlayerTriggerBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerTrigger playerTrigger;

    protected PlayerTrigger PlayerTrigger => playerTrigger;

    protected virtual void OnEnable()
    {
        playerTrigger.TriggerEnter += HandlePlayerEnter;
        playerTrigger.TriggerExit += HandlePlayerExit;
    }

    protected virtual void OnDisable()
    {
        playerTrigger.TriggerEnter -= HandlePlayerEnter;
        playerTrigger.TriggerExit -= HandlePlayerExit;
    }

    private void HandlePlayerEnter(PlayerView view)
    {
        OnPlayerEnter(view);

        if (view.IsLocalPlayer)
            OnLocalPlayerEnter(view);
    }

    private void HandlePlayerExit(PlayerView view)
    {
        OnPlayerExit(view);

        if (view.IsLocalPlayer)
            OnLocalPlayerExit(view);
    }

    protected virtual void OnPlayerEnter(PlayerView view) { }
    protected virtual void OnPlayerExit(PlayerView view) { }
    protected virtual void OnLocalPlayerEnter(PlayerView view) { }
    protected virtual void OnLocalPlayerExit(PlayerView view) { }
}
