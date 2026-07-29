using Dissolver;
using UnityEngine;

public class DissolverRoomView : MonoBehaviour, IView
{
    [SerializeField] private PlayerTrigger playerTrigger;

    [SerializeField] private DissolverShaderPanelController controller;

    private void Awake()
    {
        Hide();

        playerTrigger.TriggerEnter += Show;
        playerTrigger.TriggerExit += Hide;
    }

    private void Show(PlayerView view)
    {
        if (view.IsLocalPlayer)
            controller.SetPlayer(view.DissolveController);

        view.SetDissolveMaterial();
    }

    private void Hide(PlayerView view)
    {
        view.ResetMaterial();
        if (!view.IsLocalPlayer)
            return;

        controller.Hide();
    }

    public void Hide()
    {
    }

    public void Show()
    {

    }

    private void OnDestroy()
    {
        playerTrigger.TriggerEnter -= Show;
        playerTrigger.TriggerExit -= Hide;
    }
}
