using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class PlayerHUD : MonoBehaviour
{
    private const string currentPOILabelName = "current";
    private const string totalPOILabelName = "total";

    [SerializeField] private PanelRenderer panelRenderer;

    private Label currentPOILabel;
    private Label totalPOILabel;

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

    public void UpdateCurrentPOI(int current)
    {
        //currentPOILabel.text = current.ToString();
    }

    public void UpdateTotalPOI(int total)
    {
        //totalPOILabel.text = total.ToString();
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }
}
