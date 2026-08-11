using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class UIInputBlocker : MonoBehaviour
{
    private static readonly HashSet<VisualElement> activeRoots = new();

    private PanelRenderer panelRenderer;
    private VisualElement root;

    private void Awake()
    {
        panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (root != null)
            activeRoots.Remove(root);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement newRoot, int version)
    {
        if (root != null)
            activeRoots.Remove(root);

        root = newRoot;
        activeRoots.Add(root);
    }

    public static bool IsPointerOverUI(Vector2 screenPosition)
    {
        Vector2 uiScreenPosition = new(screenPosition.x, Screen.height - screenPosition.y);

        foreach (VisualElement root in activeRoots)
        {
            if (root?.panel == null)
                continue;

            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(root.panel, uiScreenPosition);
            VisualElement picked = root.panel.Pick(panelPosition);

            for (VisualElement element = picked; element != null; element = element.parent)
            {
                if (element.ClassListContains("blocks-player-input"))
                    return true;
            }
        }

        return false;
    }
}