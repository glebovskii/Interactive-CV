using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private CharacterPreviewPresenter characterPreviewPresenter;
    [SerializeField] private ColorPickerController colorPickerController;

    private void Awake()
    {
        ServiceLocator.Register(colorPickerController);
        characterPreviewPresenter.Init();
    }
}
