using Unity.Cinemachine;
using UnityEngine;

public class PanelView : MonoBehaviour
{
    [SerializeField] private PanelRevealAnimation panelRevealAnimation;
    [SerializeField] private WorldSpacePanelTilt worldSpaceTilt;

    public void Show(CinemachineCamera camera)
    {
        worldSpaceTilt?.Follow(camera);
        panelRevealAnimation?.Show();
    }

    public void Hide(bool playSound = true)
    {
        worldSpaceTilt?.StopFollowing();
        panelRevealAnimation?.Hide(playSound);
    }
}
