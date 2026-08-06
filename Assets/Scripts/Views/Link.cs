using UnityEngine;

public class Link : MonoBehaviour
{
    [SerializeField] protected string link;

    public void OpenLink()
    {
        //if (ServiceLocator.TryGet(out UISoundController soundController))
        //    soundController.PlayButtonClick();

        Application.OpenURL(link);
    }

    protected void OpenLinkWithoutSound()
    {
        Application.OpenURL(link);
    }
}