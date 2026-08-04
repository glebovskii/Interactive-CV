using UnityEngine.UIElements;

public interface IView
{
    public void Show();
    public void Hide(bool playSound = true);
}
