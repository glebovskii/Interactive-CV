using UnityEngine;

public abstract class PlayerTriggerBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerTrigger playerTrigger;
    [SerializeField] protected bool disablePlayerHUDOnEnter = false;
    //[SerializeField] private GameObject icon;
    //[SerializeField] private bool alwaysShowIcon = false;
    protected PlayerTrigger PlayerTrigger => playerTrigger;

    protected virtual void OnEnable()
    {
        playerTrigger.TriggerEnter += HandlePlayerEnter;
        playerTrigger.TriggerExit += HandlePlayerExit;

        //if (icon != null)
        //    icon.SetActive(alwaysShowIcon);
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
        {
            OnLocalPlayerEnter(view);
            //if (icon != null && !alwaysShowIcon)
            //    icon.SetActive(true);
        }
    }

    private void HandlePlayerExit(PlayerView view)
    {
        OnPlayerExit(view);

        if (view.IsLocalPlayer)
        {
            OnLocalPlayerExit(view);
            //if (icon != null && !alwaysShowIcon)
            //    icon.SetActive(false);
        }
    }

    protected virtual void OnPlayerEnter(PlayerView view) { }
    protected virtual void OnPlayerExit(PlayerView view) { }
    protected virtual void OnLocalPlayerEnter(PlayerView view) { }
    protected virtual void OnLocalPlayerExit(PlayerView view) { }
}
