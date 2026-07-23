using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private CharacterPreviewPresenter characterPreviewPresenter;
    [SerializeField] private ColorPickerController colorPickerController;
    [SerializeField] private NetworkSessionService networkSessionService;

    private void Awake()
    {
        ServiceLocator.Register(colorPickerController);
        ServiceLocator.Register(networkSessionService);
        characterPreviewPresenter.Init();
    }
}
