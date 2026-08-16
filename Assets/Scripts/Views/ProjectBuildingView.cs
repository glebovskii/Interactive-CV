using UnityEngine;

public sealed class ProjectBuildingView : PlayerTriggerBehaviour
{
    [SerializeField] private PanelView panel;
    [SerializeField] private PanelView openProjectBtnPanel;
    [SerializeField] private string analyticsValue;

    [SerializeField] private bool useMediatorButton;
    private PlayerView currentView;
    private ProjectPromptView openProjectButton;

    private void Awake()
    {
        panel.Hide(false);
        if (useMediatorButton)
        {
            openProjectBtnPanel.Hide(false);

            openProjectButton = GetComponentInChildren<ProjectPromptView>(true);

            if (openProjectButton == null)
                Debug.LogError($"{nameof(ProjectBuildingView)} requires a {nameof(ProjectPromptView)} in its children.", this);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (useMediatorButton && openProjectButton != null)
            openProjectButton.Clicked += OpenProject;
    }

    protected override void OnDisable()
    {
        if (useMediatorButton && openProjectButton != null)
            openProjectButton.Clicked -= OpenProject;

        HideAll(false);
        base.OnDisable();
    }

    protected override void OnLocalPlayerEnter(PlayerView view)
    {
        currentView = view;

        if (useMediatorButton)
        {
            if (openProjectButton == null)
                return;

            currentView.AddTarget(openProjectButton.transform);
            openProjectBtnPanel.Show(currentView.Camera);
            openProjectButton.Show();
        }
        else
            OpenProject();

        if(disablePlayerHUDOnEnter)
            view.SetHUDEnabled(false);
    }

    protected override void OnLocalPlayerExit(PlayerView view)
    {
        HideAll();
        currentView = null;
        view.SetHUDEnabled(true);
    }

    private void OpenProject()
    {
        if (currentView == null || (useMediatorButton && openProjectButton == null))
            return;

        if (useMediatorButton)
        {
            openProjectButton.Hide();
            openProjectBtnPanel.Hide(false);
            currentView.RemoveTarget(openProjectButton.transform);
        }
        currentView.AddTarget(panel.transform);

        if (!string.IsNullOrEmpty(analyticsValue))
            AnalyticsService.ProjectOpened(analyticsValue);

        panel.Show(currentView.Camera);
    }

    private void HideAll(bool playSound = true)
    {
        if (currentView != null)
        {
            if (useMediatorButton && openProjectButton != null)
                currentView.RemoveTarget(openProjectButton.transform);

            currentView.RemoveTarget(panel.transform);
        }

        if (useMediatorButton)
        {
            openProjectButton?.Hide(false);
            openProjectBtnPanel.Hide(false);
        }
        panel.Hide(playSound);
    }
}
