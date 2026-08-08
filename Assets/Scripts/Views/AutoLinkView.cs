using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UnityEngine.InputSystem.LowLevel.MouseButton;

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

        //LinkRoutine();
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
        //FakeClick();
    }

    private void FakeClick()
    {
        var mouse = Mouse.current;

        InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left, false));
        InputSystem.QueueEvent(new InputEventPtr());//.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Left, false));
        //InputSystem.QueueStateEvent(mouse, new MouseState().WithButton(MouseButton.Right, false));
    }

    //private void LinkRoutine()
    //{
    //    link.OpenLink();

    //}

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
