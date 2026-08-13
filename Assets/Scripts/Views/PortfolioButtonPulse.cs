using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class PortfolioButtonPulse : MonoBehaviour
{
    private const string ButtonName = "button-link";
    private const string ExpandedClass = "portfolio-pulse-button--expanded";

    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField, Min(100)] private long pulseIntervalMs = 650;

    private Button button;
    private IVisualElementScheduledItem pulseItem;

    private void Awake()
    {
        if (panelRenderer == null)
            panelRenderer = GetComponent<PanelRenderer>();
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        if (button != null)
            StartPulse();
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        StopPulse();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        StopPulse();

        button = root.Q<Button>(ButtonName);

        if (button != null)
            StartPulse();
    }

    private void StartPulse()
    {
        StopPulse();

        button.RemoveFromClassList(ExpandedClass);
        pulseItem = button.schedule.Execute(() => button.ToggleInClassList(ExpandedClass)).Every(pulseIntervalMs);
    }

    private void StopPulse()
    {
        pulseItem?.Pause();
        pulseItem = null;

        button?.RemoveFromClassList(ExpandedClass);
    }
}