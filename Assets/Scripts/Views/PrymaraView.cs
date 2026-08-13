using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class PrymaraView : PanelTriggerView
{
    [SerializeField] private int defaultRendererIndex;
    [SerializeField] private int prymaraRendererIndex = 1;

    private UniversalAdditionalCameraData cameraData;
    private CameraOverrideOption defaultColorOption;

    protected override void Awake()
    {
        base.Awake();

        cameraData = Camera.main.GetUniversalAdditionalCameraData();
        defaultColorOption = cameraData.requiresColorOption;
    }

    protected override void BeforePanelShow(PlayerView view)
    {
        cameraData.SetRenderer(prymaraRendererIndex);
        cameraData.requiresColorOption = CameraOverrideOption.On;
    }

    protected override void AfterPanelHide(PlayerView view)
    {
        cameraData.SetRenderer(defaultRendererIndex);
        cameraData.requiresColorOption = defaultColorOption;
    }
}
