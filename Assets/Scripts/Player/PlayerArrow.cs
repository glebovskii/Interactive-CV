using DG.Tweening;
using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    private PlayerPOIController controller;
    public void Init(PlayerPOIController playerPOIController)
    {
        controller = playerPOIController;
    }

    private void LateUpdate()
    {
        if (controller == null)
            return;

        transform.rotation = controller.GetArrowRotation();
    }

    public void SetVisible(bool visible)
    {
        gameObject.transform.DOScale(visible? 1f : 0f, 0.5f);
    }
}
