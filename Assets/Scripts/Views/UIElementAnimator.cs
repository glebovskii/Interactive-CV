using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class UIElementAnimator
{
    private Tween tween;

    public void Show(VisualElement element, bool animate = true)
    {
        if (element == null)
            return;

        Stop();
        element.style.display = DisplayStyle.Flex;

        if (!animate)
        {
            element.style.opacity = 1f;
            element.style.scale = new Scale(Vector3.one);
            return;
        }

        element.style.opacity = 0f;
        element.style.scale = new Scale(new Vector3(0.8f, 0.8f, 1f));

        float progress = 0f;
        tween = DOTween.To(() => progress, value =>
        {
            progress = value;
            element.style.opacity = progress;

            float scale = Mathf.Lerp(0.8f, 1f, EaseOutBack(progress));
            element.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }, 1f, 0.25f).SetUpdate(true);
    }

    public void Hide(VisualElement element, bool animate = true)
    {
        if (element == null)
            return;

        Stop();

        if (!animate)
        {
            element.style.display = DisplayStyle.None;
            element.style.opacity = 0f;
            element.style.scale = new Scale(Vector3.one);
            return;
        }

        float progress = 1f;
        tween = DOTween.To(() => progress, value =>
        {
            progress = value;
            element.style.opacity = progress;
            element.style.scale = new Scale(new Vector3(progress, progress, 1f));
        }, 0f, 0.15f).SetUpdate(true).OnComplete(() =>
        {
            if (element != null)
                element.style.display = DisplayStyle.None;
        });
    }

    public void Stop()
    {
        tween?.Kill();
        tween = null;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }
}
