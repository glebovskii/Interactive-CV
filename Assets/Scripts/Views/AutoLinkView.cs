using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class AutoLinkView : MonoBehaviour
{
    [SerializeField] private Link link;
    [SerializeField] private float totalTime = 2f;
    [SerializeField] private PlayerTrigger playerTrigger;
    [SerializeField] private TriggerLinkController linkController;

    [SerializeField] private float timeBetweenSoundPlay = 0.2f;

    private float prevTimeSoundPlay = 0f;

    private UISoundController soundController;

    private IEnumerator routine;

    [SerializeField] private string analyticsName = "link_open";
    [SerializeField] private string analyticsKey = "project";
    [SerializeField] private string analyticsValue;

    private void OnEnable()
    {
        //routine = LinkRoutine();

        linkController.SetFill(0f);

        playerTrigger.TriggerEnter += TriggerEnter;
        playerTrigger.TriggerExit += TriggerExit;

        ServiceLocator.TryGet(out soundController);
    }

    private void TriggerEnter(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;
        AnalyticsService.LogEvent(analyticsName, analyticsKey, analyticsValue);
        routine = LinkRoutine();
        StartCoroutine(routine);
    }

    private IEnumerator LinkRoutine()
    {
        soundController?.SetPitch(1);
        float time = 0;
        prevTimeSoundPlay = 0f;
        linkController.SetFill(0);
        while (time < totalTime)
        {
            if (time - prevTimeSoundPlay > timeBetweenSoundPlay)
            {
                soundController?.PlayLinkLoad();
                prevTimeSoundPlay = time;
            }
            soundController?.SetPitch(1 + time);
            linkController.SetFill(time / totalTime);
            time += Time.deltaTime;
            yield return null;
        }
        soundController?.SetPitch(1);
        linkController.SetFill(1);
    }

    private void TriggerExit(PlayerView view)
    {
        if (!view.IsLocalPlayer)
            return;

        StopCoroutine(routine);
        linkController.SetFill(0);
    }

    private void OnDisable()
    {
        playerTrigger.TriggerEnter -= TriggerEnter;
        playerTrigger.TriggerExit -= TriggerExit;
    }
}
