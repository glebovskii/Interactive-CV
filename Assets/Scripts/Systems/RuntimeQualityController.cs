using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RuntimeQualityController : MonoBehaviour
{
    [SerializeField] private UniversalRenderPipelineAsset mobileRPAsset;
    [SerializeField] private UniversalRenderPipelineAsset pcRPAsset;

    private void Start()
    {
        SetQuality(PlayerInfoSave.GetQualityIndex());
    }

    private void SetQuality(int index)
    {
        PlayerInfoSave.SaveQualityIndex(index);

        QualitySettings.renderPipeline = index switch
        {
            0 => mobileRPAsset,
            1 => pcRPAsset,
            _ => mobileRPAsset
        };
    }
}