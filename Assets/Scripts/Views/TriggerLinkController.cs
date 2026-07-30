using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public sealed class TriggerLinkController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private string buttonName = "link";
    [SerializeField] private string fillName = "fill";
    [SerializeField, Min(0f)] private float idleDistance = 3f;
    [SerializeField, Min(0.1f)] private float halfCycleSeconds = 0.9f;

    private VisualElement orb;
    private VisualElement fill;
    private Tween idleTween;
    private Vector3 basePosition;
    private float normalizedFill;

    private void Awake()
    {
        if (panelRenderer == null) 
            panelRenderer = GetComponent<PanelRenderer>();
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnEnable()
    {
        if (orb != null) 
            StartIdle();
    }

    private void OnDisable()
    {
        StopIdle();
    }

    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
    private void OnDestroy()
    {
        StopIdle();
        if (panelRenderer != null) 
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    public void SetFill(float value)
    {
        normalizedFill = Mathf.Clamp01(value);
        if (fill != null)
            fill.style.scale = new Scale(new Vector3(normalizedFill, normalizedFill, 1f));
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement root, int version)
    {
        StopIdle();
        orb = root.Q<VisualElement>(buttonName);
        fill = root.Q<VisualElement>(fillName);
        if (fill != null)
            fill.style.scale = new Scale(new Vector3(normalizedFill, normalizedFill, 1f));

        if (orb == null) 
            return;

        basePosition = orb.resolvedStyle.translate;
        if (isActiveAndEnabled) 
            StartIdle();
    }

    private void StartIdle()
    {
        StopIdle();
        orb.style.translate = basePosition + Vector3.down * idleDistance;
        idleTween = DOVirtual.Float(-idleDistance, idleDistance, halfCycleSeconds, y => orb.style.translate = basePosition + Vector3.up * y)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    private void StopIdle()
    {
        idleTween?.Kill();
        idleTween = null;
        if (orb != null) 
            orb.style.translate = basePosition;
    }
}
