using Dissolver;
using UnityEngine;

public sealed class DissolverView : PanelTriggerView
{
    [SerializeField] private DissolverShaderPanelController controller;

    protected override void OnPlayerEnter(PlayerView view)
    {
        view.SetDissolveMaterial();
    }

    protected override void OnPlayerExit(PlayerView view)
    {
        view.ResetMaterial();
    }

    protected override void BeforePanelShow(PlayerView view)
    {
        controller.SetPlayer(view.DissolveController);
    }

    protected override void BeforePanelHide(PlayerView view)
    {
        controller.Hide();
    }
}
