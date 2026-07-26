using UnityEngine;
using UnityEngine.UIElements;

public class BuildingVIew : MonoBehaviour, IView
{
    [SerializeField] private PanelUI panel;
    [SerializeField] private PlayerTrigger playerTrigger;
    
    public PanelUI Panel => panel;

    private void Awake()
    {
        Hide();
        playerTrigger.TriggerEnter += Show;
        playerTrigger.TriggerExit += Hide;
    }

    private void Show(PlayerView view)
    {
        view.AddTarget(Panel.transform);
        panel.Show(view.Camera);
    }

    private void Hide(PlayerView view)
    {
        view.RemoveTarget(Panel.transform);
        Hide();
    }

    public void Hide()
    {
        panel.Hide();
    }

    public void Show()
    {
        
    }

    private void OnDestroy()
    {
        playerTrigger.TriggerEnter -= Show;
        playerTrigger.TriggerExit -= Hide;
    }
}
