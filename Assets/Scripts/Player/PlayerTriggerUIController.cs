using DG.Tweening;
using System;
using UnityEngine;

public class PlayerTriggerUIController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer renderer;
    [SerializeField] private PlayerTrigger playerTrigger;
    [SerializeField] private float animationTime = 1f;

    private Material triggerMat;

    private static readonly int CutoffId = Shader.PropertyToID("_Cutoff");

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Color emissionColor;
    private Sequence sequence;

    private void Awake()
    {
        playerTrigger.TriggerEnter += PlayTriggerOn;
        playerTrigger.TriggerExit += PlayTriggerOff;

        triggerMat = renderer.sharedMaterial;
        emissionColor = GetBaseEmissionColor(triggerMat.GetColor(EmissionColorId));
    }

    private static Color GetBaseEmissionColor(Color hdrColor)
    {
        float maximumComponent = Mathf.Max(hdrColor.r, hdrColor.g, hdrColor.b);

        if (maximumComponent <= 0f)
            return Color.white;

        if (maximumComponent > 1f)
            hdrColor /= maximumComponent;

        hdrColor.a = 1f;
        return hdrColor;
    }

    private void PlayTriggerOn(PlayerView view)
    {
        if (playerTrigger.Triggers.Count == 0)
        {
            Play(animationTime);
        }
    }

    private void PlayTriggerOff(PlayerView view)
    {
        if(playerTrigger.Triggers.Count == 1)
        {
            PlayReverse(animationTime);
        }
    }

    public Tween Play(float totalTime)
    {
        sequence?.Kill();

        totalTime = Mathf.Max(0.01f, totalTime);

        float emissionDelay = totalTime * 0.5f;
        float emissionDuration = totalTime - emissionDelay;

        triggerMat.EnableKeyword("_EMISSION");

        triggerMat.SetFloat(CutoffId, 1f);
        SetEmissionExposure(-10f);

        sequence = DOTween.Sequence().SetTarget(this);

        sequence.Insert(0f, DOTween.To(
                    () => triggerMat.GetFloat(CutoffId),
                    value => triggerMat.SetFloat(CutoffId, value),
                    0f,
                    totalTime)
                .SetEase(Ease.Linear));

        sequence.Insert(emissionDelay, DOTween.To(() => -10f, SetEmissionExposure, 1f, emissionDuration).SetEase(Ease.Linear));

        return sequence;
    }

    public Tween PlayReverse(float totalTime)
    {
        sequence?.Kill();

        totalTime = Mathf.Max(0.01f, totalTime);

        float alphaDelay = totalTime * 0.5f;
        float alphaDuration = totalTime - alphaDelay;

        triggerMat.EnableKeyword("_EMISSION");

        // Initial visible state.
        triggerMat.SetFloat(CutoffId, 0f);
        SetEmissionExposure(1f);

        sequence = DOTween.Sequence()
            .SetTarget(this);

        // Emission decreases throughout the entire tween.
        sequence.Insert(
            0f,
            DOTween.To(
                    () => 1f,
                    SetEmissionExposure,
                    -10f,
                    totalTime)
                .SetEase(Ease.Linear));

        // Alpha clipping starts halfway through.
        sequence.Insert(alphaDelay, triggerMat.DOFloat(1f, CutoffId, alphaDuration).SetEase(Ease.Linear));

        return sequence;
    }

    private void SetEmissionExposure(float exposure)
    {
        float intensity = Mathf.Pow(2f, exposure);

        triggerMat.SetColor(EmissionColorId, emissionColor * intensity);
    }

    private void OnDestroy()
    {
        sequence?.Kill();
        playerTrigger.TriggerEnter -= PlayTriggerOn;
        playerTrigger.TriggerExit -= PlayTriggerOff;
    }


}
