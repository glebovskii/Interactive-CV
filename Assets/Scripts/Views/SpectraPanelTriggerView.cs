using UnityEngine;

public class SpectraPanelTriggerView : PanelTriggerView
{
    [SerializeField] private Light light;

    protected override void BeforePanelShow(PlayerView view)
    {
        base.BeforePanelShow(view);
        if (view.IsLocalPlayer)
            light.enabled = true;
    }

    protected override void BeforePanelHide(PlayerView view)
    {
        base.BeforePanelHide(view);
        if (view.IsLocalPlayer)
            light.enabled = false;
    }
}
