using UnityEngine;

public class DissolverRoomView : MonoBehaviour, IView
{
    [SerializeField] private PlayerTrigger playerTrigger;

    [SerializeField] private Material dissolveMat;

    private Material defaultMat;

    private int baseColorId = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        Hide();

        playerTrigger.TriggerEnter += Show;
        playerTrigger.TriggerExit += Hide;
    }

    private void Show(PlayerView view)
    {
        //if (!view.IsLocalPlayer)
        //    return;

        defaultMat = view.Renderer.material;
        view.Renderer.material = dissolveMat;
        view.Renderer.material.SetColor(baseColorId, defaultMat.color);
    }

    private void Hide(PlayerView view)
    {
        //if (!view.IsLocalPlayer)
            //return;

        view.ResetMaterial();
    }

    public void Hide()
    {
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
