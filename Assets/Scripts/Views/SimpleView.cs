using UnityEngine;

public class SimpleView : MonoBehaviour, IView
{
    [SerializeField] private PanelUI panel;
    [SerializeField] private PlayerTrigger playerTrigger;

    [SerializeField] private string analyticsValue;

    public PanelUI Panel => panel;

    private void Awake()
    {
        Hide(false);

        playerTrigger.TriggerEnter += ShowBtn;
        playerTrigger.TriggerExit += HideBtn;
    }

    private void HideBtn(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        view?.RemoveTarget(Panel.transform);
        Hide();
        
    }

    private void ShowBtn(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        AnalyticsService.ProjectOpened(analyticsValue);
        view.AddTarget(Panel.transform);
        panel.Show(view.Camera);
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
        playerTrigger.TriggerEnter -= ShowBtn;
        playerTrigger.TriggerExit -= HideBtn;
    }
}
