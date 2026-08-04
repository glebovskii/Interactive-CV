using UnityEngine;

public class BuildingView : MonoBehaviour, IView
{
    [SerializeField] private PanelUI panel;
    [SerializeField] private PlayerTrigger playerTrigger;
    
    public PanelUI Panel => panel;

    private void Awake()
    {
        Hide(false);
        playerTrigger.TriggerEnter += Show;
        playerTrigger.TriggerExit += Hide;
    }

    private void Show(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        view.AddTarget(Panel.transform);
        panel.Show(view.Camera);
    }

    private void Hide(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        view.RemoveTarget(Panel.transform);
        Hide();
    }

    public void Hide(bool playSound = true)
    {
        panel.Hide(playSound);
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
