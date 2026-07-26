using UnityEngine.UIElements;

public interface IView
{
    public PanelUI Panel { get; }
    public void Show();
    public void Hide();
}
