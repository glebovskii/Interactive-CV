using System;
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

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement)
    {
        Debug.LogError("PLAYER HUD INIT");
        totalPOILabel = rootElement.Q<Label>(name:totalPOILabelName);
        currentPOILabel = rootElement.Q<Label>(name:currentPOILabelName);
        playerIcon = rootElement.Q<Image>(name: playerMapIconName);

        currentPOILabel.SetBinding("text", new DataBinding()
        {
            dataSource = playerPOIController,
            dataSourcePath = new Unity.Properties.PropertyPath(nameof(playerPOIController.VisitedPOI))
        });

        totalPOILabel.SetBinding("text", new DataBinding()
        {
            dataSource = playerPOIController,
            dataSourcePath = new Unity.Properties.PropertyPath(nameof(playerPOIController.TotalPOI))
        });
    }

    public void Init(PlayerPOIController controller)
    {
        playerPOIController = controller;
    }

    private void Update()
    {
        Debug.LogError($"UV: {playerIcon.uv}");
    }

    public void UpdateMap(float2 uv)
    {
        float x = uv.x;
        float y = 1 - uv.y;

        playerIcon.style.left = new Length(x * 100f, LengthUnit.Percent);
        playerIcon.style.top = new Length(y * 100f, LengthUnit.Percent);

        playerIcon.style.translate = new Translate(
            new Length(-50f, LengthUnit.Percent),
            new Length(-50f, LengthUnit.Percent)
        );
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    
}
