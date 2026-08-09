using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private CharacterPreviewPresenter characterPreviewPresenter;
    [SerializeField] private ColorPickerController colorPickerController;
    [SerializeField] private NetworkSessionService networkSessionService;
    [SerializeField] private UISoundController uiSoundController;

    private void Start()
    {
        AnalyticsService.LogEvent("unity_test");
    }

    private void Awake()
    {
        ServiceLocator.Register(colorPickerController);
        ServiceLocator.Register(networkSessionService);
        ServiceLocator.Register(uiSoundController);
        characterPreviewPresenter.Init();
    }
}
