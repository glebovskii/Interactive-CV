using UnityEngine;

public class BuildingView : MonoBehaviour, IView
{
    [SerializeField] private PanelUI panel;
    [SerializeField] private PlayerTrigger playerTrigger;

    [SerializeField] private string analyticsName = "project_open";
    [SerializeField] private string analyticsKey = "project";
    [SerializeField] private string analyticsValue;

    public PanelUI Panel => panel;

    private void Awake()
    {
        Hide(false);
        playerTrigger.TriggerEnter += Show;
        playerTrigger.TriggerExit += Hide;
    }

    private void Show(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        AnalyticsService.LogEvent(analyticsName, analyticsKey, analyticsValue);

        view.AddTarget(Panel.transform);
        panel.Show(view.Camera);
    }

    private void Hide(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        view.RemoveTarget(Panel.transform);
        Hide();
    }

    public void Hide(bool playSound = true)
    {
        panel.Hide(playSound);
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
