using System;
using UnityEngine;

public class Link : MonoBehaviour
{
    [SerializeField] protected string link;
    [SerializeField] protected bool checkLinkValidOnClick = true;

    private bool isLinkValid = false;

    private UISoundController soundController;

    private void Awake()
    {
        if(!checkLinkValidOnClick)
        {
            isLinkValid = IsValidWebLink(link);
        }

        ServiceLocator.TryGet(out soundController);
    }

    public void OpenLink()
    {
        if (checkLinkValidOnClick)
        {
            isLinkValid = IsValidWebLink(link);
            if (!isLinkValid)
            {
                Debug.LogError(
                    $"Invalid link assigned to {nameof(PanelLinkButton)}: '{link}'",
                    this);

                return;
            }
        }

        if (isLinkValid)
        {
            ServiceLocator.TryGet(out soundController);

            soundController?.PlayButtonClick();
            Application.OpenURL(link);
        }
        else
            Debug.LogError($"Link {link} is not valid");
    }

    protected static bool IsValidWebLink(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
}