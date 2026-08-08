using Prymara;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PrymaraRoomView : MonoBehaviour, IView
{
    [SerializeField] private PlayerTrigger playerTrigger;
    [SerializeField] private ComicShaderPanelController panelController;


    private int defaultRendererIndex = 0;
    private UniversalAdditionalCameraData cameraData;

    private void Awake()
    {
        Hide();
        cameraData = Camera.main.GetUniversalAdditionalCameraData();

        var urp = UniversalRenderPipeline.asset;

        playerTrigger.TriggerEnter += Show;
        playerTrigger.TriggerExit += Hide;
    }

    private void Show(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        cameraData.SetRenderer(1);
        cameraData.requiresColorOption = CameraOverrideOption.On;
    }

    private void Hide(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        cameraData.SetRenderer(defaultRendererIndex);

    }

    public void Hide(bool playSound = true)
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
