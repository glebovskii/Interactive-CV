using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PlayerHUD : MonoBehaviour
{
    private const string currentPOILabelName = "current";
    private const string totalPOILabelName = "total";
    private const string playerMapIconName = "Player";

    [SerializeField] private PanelRenderer panelRenderer;

    private Label currentPOILabel;
    private Label totalPOILabel;
    private Image playerIcon;

    private PlayerPOIController playerPOIController;
    private Tween poiTween;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        totalPOILabel = rootElement.Q<Label>(totalPOILabelName);
        currentPOILabel = rootElement.Q<Label>(currentPOILabelName);
        playerIcon = rootElement.Q<Image>(playerMapIconName);

        currentPOILabel.SetBinding("text", new DataBinding
        {
            dataSource = playerPOIController,
            dataSourcePath = new Unity.Properties.PropertyPath(nameof(playerPOIController.VisitedPOI)),
            bindingMode = BindingMode.ToTarget
        });

        totalPOILabel.SetBinding("text", new DataBinding
        {
            dataSource = playerPOIController,
            dataSourcePath = new Unity.Properties.PropertyPath(nameof(playerPOIController.TotalPOI)),
            bindingMode = BindingMode.ToTarget
        });
    }

    public void Init(PlayerPOIController controller)
    {
        playerPOIController = controller;
        playerPOIController.propertyChanged += OnPOIPropertyChanged;
    }

    private void OnPOIPropertyChanged(object sender, BindablePropertyChangedEventArgs args)
    {
        if (args.propertyName == nameof(PlayerPOIController.VisitedPOI))
            AnimateCurrentPOI();
    }

    private void AnimateCurrentPOI()
    {
        poiTween?.Kill();

        float scale = 1f;

        poiTween = DOTween.Sequence()
            .Append(DOTween.To(() => scale, value =>
            {
                scale = value;
                currentPOILabel.style.scale = new Scale(new Vector2(scale, scale));
            }, 1.35f, 0.2f).SetEase(Ease.OutBack))
            .Append(DOTween.To(() => scale, value =>
            {
                scale = value;
                currentPOILabel.style.scale = new Scale(new Vector2(scale, scale));
            }, 1f, 0.4f).SetEase(Ease.OutCubic));
    }

    public void UpdateMap(float2 uv)
    {
        float x = uv.x;
        float y = 1f - uv.y;

        playerIcon.style.left = new Length(x * 100f, LengthUnit.Percent);
        playerIcon.style.top = new Length(y * 100f, LengthUnit.Percent);

        playerIcon.style.translate = new Translate(
            new Length(-50f, LengthUnit.Percent),
            new Length(-50f, LengthUnit.Percent));
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (playerPOIController != null)
            playerPOIController.propertyChanged -= OnPOIPropertyChanged;

        poiTween?.Kill();
    }
}