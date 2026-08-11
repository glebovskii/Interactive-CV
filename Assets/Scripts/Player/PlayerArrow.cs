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
        gameObject.SetActive(visible);
    }
}
