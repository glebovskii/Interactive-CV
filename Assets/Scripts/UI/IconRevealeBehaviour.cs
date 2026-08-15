using System;
using UnityEngine;
using UnityEngine.UIElements;

public class IconRevealeBehaviour : MonoBehaviour
{
    private const string iconName = "asset-store-icon";
    private const string unknownName = "unknown";

    [SerializeField] private PlayerTrigger playerTrigger;
    [SerializeField] private PanelRenderer panelRenderer;

    private VisualElement unknownElement;
    private VisualElement iconElement;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);

        playerTrigger.TriggerEnter += Reveale;
    }

    private void Reveale(PlayerView view)
    {
        if(view.IsLocalPlayer)
        {
            iconElement.style.display = DisplayStyle.Flex;
            unknownElement.style.display = DisplayStyle.None;
        }
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        unknownElement = rootElement.Q<VisualElement>(name:unknownName);
        iconElement = rootElement.Q<VisualElement>(name:iconName);
        iconElement.style.display = DisplayStyle.None;
        unknownElement.style.display = DisplayStyle.Flex;
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
        playerTrigger.TriggerEnter -= Reveale;
    }
}
