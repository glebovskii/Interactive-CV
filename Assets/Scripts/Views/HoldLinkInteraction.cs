using System.Collections;
using UnityEngine;

public sealed class HoldLinkInteraction : PlayerTriggerBehaviour
{
    [SerializeField, Min(0.1f)] private float totalTime = 2f;
    [SerializeField] private LinkPromptView linkPrompt;
    [SerializeField, Min(0f)] private float timeBetweenSoundPlay = 0.2f;
    [SerializeField] private string analyticsValue;

    private UISoundController soundController;
    private Coroutine loadingRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        linkPrompt.SetFill(0f);
        ServiceLocator.TryGet(out soundController);
    }

    protected override void OnDisable()
    {
        StopLoading();
        linkPrompt.SetFill(0f);
        base.OnDisable();
    }

    protected override void OnLocalPlayerEnter(PlayerView view)
    {
        if (!string.IsNullOrEmpty(analyticsValue))
            AnalyticsService.LinkOpened(analyticsValue);

        StopLoading();
        loadingRoutine = StartCoroutine(LinkRoutine());
    }

    protected override void OnLocalPlayerExit(PlayerView view)
    {
        StopLoading();
        linkPrompt.SetFill(0f);
    }

    private IEnumerator LinkRoutine()
    {
        soundController?.SetPitch(1f);

        float time = 0f;
        float previousSoundTime = 0f;
        linkPrompt.SetFill(0f);

        while (time < totalTime)
        {
            if (time - previousSoundTime > timeBetweenSoundPlay)
            {
                soundController?.PlayLinkLoad();
                previousSoundTime = time;
            }

            soundController?.SetPitch(1f + time);
            linkPrompt.SetFill(time / totalTime);
            time += Time.deltaTime;
            yield return null;
        }

        soundController?.SetPitch(1f);
        linkPrompt.SetFill(1f);
        loadingRoutine = null;
    }

    private void StopLoading()
    {
        if (loadingRoutine != null)
        {
            StopCoroutine(loadingRoutine);
            loadingRoutine = null;
        }

    }
}
