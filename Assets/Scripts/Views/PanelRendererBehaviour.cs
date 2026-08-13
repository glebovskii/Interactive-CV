using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public abstract class PanelRendererBehaviour : MonoBehaviour
{
    protected PanelRenderer PanelRenderer { get; private set; }
    protected VisualElement Root { get; private set; }

    protected virtual void Awake()
    {
        PanelRenderer = GetComponent<PanelRenderer>();
        PanelRenderer.RegisterUIReloadCallback(HandleUIReload);
    }

    protected virtual void OnDestroy()
    {
        if (PanelRenderer != null)
            PanelRenderer.UnregisterUIReloadCallback(HandleUIReload);
    }

    private void HandleUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        Root = root;
        OnUIReload(root);
    }

    protected abstract void OnUIReload(VisualElement root);
}
