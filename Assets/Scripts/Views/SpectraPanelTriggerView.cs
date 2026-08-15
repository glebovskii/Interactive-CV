using UnityEngine;

public class SpectraPanelTriggerView : PanelTriggerView
{
    [SerializeField] private Light light;
    [SerializeField] private MeshRenderer room;
    [SerializeField] private Material basicMaterial;
    [SerializeField] private Material spectraMaterial;

    protected override void OnLocalPlayerEnter(PlayerView view)
    {
        base.OnLocalPlayerEnter(view);
        if (view.IsLocalPlayer)
            room.sharedMaterial = spectraMaterial;
    }

    protected override void OnLocalPlayerExit(PlayerView view)
    {
        base.OnLocalPlayerExit(view);
        if (view.IsLocalPlayer)
            room.sharedMaterial = basicMaterial;
    }

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
