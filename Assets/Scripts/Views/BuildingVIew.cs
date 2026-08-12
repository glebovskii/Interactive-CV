using System.Collections;
using UnityEngine;

public class BuildingView : MonoBehaviour, IView
{
    [SerializeField] private PanelUI panel;
    [SerializeField] private PanelUI openProjectBtnPanel;
    [SerializeField] private PlayerTrigger playerTrigger;

    [SerializeField] private string analyticsValue;

    private PlayerView currentView;
    private OpenProjectButton openProjectButton;
    public PanelUI Panel => panel;

    private void Awake()
    {
        Hide(false);
        
        openProjectButton = GetComponentInChildren<OpenProjectButton>();
        playerTrigger.TriggerEnter += ShowBtn;
        playerTrigger.TriggerExit += HideBtn;
        StartCoroutine(InitButton());
    }

    private IEnumerator InitButton()
    {
        while(!openProjectButton.Inited)
            yield return null;

        openProjectButton.Set(Show);
    }

    private void HideBtn(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        currentView?.RemoveTarget(openProjectButton.transform);
        openProjectBtnPanel.Hide();
        openProjectButton.HideOpenButton(view);
        Hide();
    }

    private void ShowBtn(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        currentView = view;
        currentView?.AddTarget(openProjectButton.transform);
        openProjectBtnPanel.Show(currentView?.Camera);
        openProjectButton.ShowOpenButton(view);
    }

    public void Hide(bool playSound = true)
    {
        if (currentView!= null && !currentView.IsLocalPlayer)
            return;

        currentView?.RemoveTarget(openProjectButton.transform);
        currentView?.RemoveTarget(Panel.transform);
        panel.Hide(playSound);
    }

    public void Show()
    {
        openProjectButton.HideOpenButton();
        AnalyticsService.ProjectOpened(analyticsValue);
        currentView.RemoveTarget(openProjectButton.transform);
        currentView.AddTarget(Panel.transform);
        panel.Show(currentView.Camera);
    }

    private void OnDestroy()
    {
        playerTrigger.TriggerEnter -= ShowBtn;
        playerTrigger.TriggerExit -= HideBtn;
    }
}
