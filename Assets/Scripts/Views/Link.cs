using UnityEngine;

public class Link : MonoBehaviour
{
    [SerializeField] protected string link;
    [SerializeField] private string analyticsValue;
    public void OpenLink()
    {
        if (ServiceLocator.TryGet(out UISoundController soundController))
            soundController.PlayButtonClick();
        AnalyticsService.LinkClicked(analyticsValue);

        Application.OpenURL(link);
    }

    protected void OpenLinkWithoutSound()
    {
        AnalyticsService.LinkClicked(analyticsValue);

        Application.OpenURL(link);
    }
}