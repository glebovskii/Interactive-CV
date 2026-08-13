using UnityEngine;

public sealed class ExternalLink : MonoBehaviour
{
    [SerializeField] private string url;
    [SerializeField] private string analyticsValue;

    public void Open(bool playSound = true)
    {
        if (playSound && ServiceLocator.TryGet(out UISoundController soundController))
            soundController.PlayButtonClick();

        AnalyticsService.LinkClicked(analyticsValue);
        Application.OpenURL(url);
    }
}
