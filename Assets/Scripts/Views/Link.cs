using UnityEngine;

public class Link : MonoBehaviour
{
    [SerializeField] protected string link;
    [SerializeField] private string analyticsName = "link_click";
    [SerializeField] private string analyticsKey = "link";
    [SerializeField] private string analyticsValue;
    public void OpenLink()
    {
        if (ServiceLocator.TryGet(out UISoundController soundController))
            soundController.PlayButtonClick();
        AnalyticsService.LogEvent(analyticsName, analyticsKey, analyticsValue);

        Application.OpenURL(link);
    }

    protected void OpenLinkWithoutSound()
    {
        AnalyticsService.LogEvent(analyticsName, analyticsKey, analyticsValue);

        Application.OpenURL(link);
    }
}