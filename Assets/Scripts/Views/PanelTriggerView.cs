using Unity.VisualScripting;
using UnityEngine;

public class PanelTriggerView : PlayerTriggerBehaviour
{
    [SerializeField] private PanelView panel;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private string analyticsValue;
    [SerializeField] private bool addToCameraTarget = true;

    protected PanelView Panel => panel;
    protected Transform CameraTarget => cameraTarget != null ? cameraTarget : panel.transform;

    protected virtual void Awake()
    {
        panel.Hide(false);
    }

    protected override void OnLocalPlayerEnter(PlayerView view)
    {
        BeforePanelShow(view);

        if (!string.IsNullOrEmpty(analyticsValue))
            AnalyticsService.ProjectOpened(analyticsValue);

        if (addToCameraTarget)
            view.AddTarget(CameraTarget);
        panel.Show(view.Camera);
        AfterPanelShow(view);
    }

    protected override void OnLocalPlayerExit(PlayerView view)
    {
        BeforePanelHide(view);
        if (addToCameraTarget)
            view.RemoveTarget(CameraTarget);
        panel.Hide();
        AfterPanelHide(view);
    }

    protected virtual void BeforePanelShow(PlayerView view) { }
    protected virtual void AfterPanelShow(PlayerView view) { }
    protected virtual void BeforePanelHide(PlayerView view) { }
    protected virtual void AfterPanelHide(PlayerView view) { }
}
