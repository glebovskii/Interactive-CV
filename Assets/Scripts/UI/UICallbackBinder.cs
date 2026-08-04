using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public sealed class UICallbackBinder : IDisposable
{
    private readonly List<Action> unregisterActions = new();
    private UISoundController soundController;

    public void BindChange<T>(VisualElement element, Action<T> action, Action<UISoundController> sound)
    {
        Bind<ChangeEvent<T>>(element, evt => action(evt.newValue), sound);
    }

    public void Bind<TEvent>(VisualElement element, Action<TEvent> action, Action<UISoundController> sound, TrickleDown trickleDown = TrickleDown.NoTrickleDown) where TEvent : EventBase<TEvent>, new()
    {
        EventCallback<TEvent> callback = evt =>
        {
            action?.Invoke(evt);
            PlaySound(sound);
        };

        element.RegisterCallback(callback, trickleDown);
        unregisterActions.Add(() => element.UnregisterCallback(callback, trickleDown));
    }

    public void BindClick(Button button, Action action, Action<UISoundController> sound)
    {
        Action callback = () =>
        {
            action?.Invoke();
            PlaySound(sound);
        };

        button.clicked += callback;
        unregisterActions.Add(() => button.clicked -= callback);
    }

    public void Clear()
    {
        for (int i = unregisterActions.Count - 1; i >= 0; i--)
            unregisterActions[i]();

        unregisterActions.Clear();
    }

    public void Dispose()
    {
        Clear();
    }

    private void PlaySound(Action<UISoundController> sound)
    {
        if (sound == null)
            return;

        if (soundController == null && !ServiceLocator.TryGet(out soundController))
            return;

        sound(soundController);
    }
}