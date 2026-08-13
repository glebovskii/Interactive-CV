using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(ExternalLink))]
public sealed class PanelLinkButton : PanelRendererBehaviour
{
    private const string ButtonName = "button-link";

    private readonly UICallbackBinder uiCallbacks = new();
    private ExternalLink link;

    protected override void Awake()
    {
        link = GetComponent<ExternalLink>();
        base.Awake();
    }

    protected override void OnUIReload(VisualElement root)
    {
        uiCallbacks.Clear();

        Button linkButton = root.Q<Button>(ButtonName);

        if (linkButton == null)
        {
            Debug.LogError($"Button named '{ButtonName}' was not found.", this);
            return;
        }

        uiCallbacks.BindClick(linkButton, () => link.Open(false), sound => sound.PlayButtonClick());
    }

    protected override void OnDestroy()
    {
        uiCallbacks.Clear();
        base.OnDestroy();
    }
}
