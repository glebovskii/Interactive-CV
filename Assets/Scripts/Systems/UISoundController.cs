using UnityEngine;

public class UISoundController : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [Space(5)]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip panelOpen;
    [SerializeField] private AudioClip panelClose;
    [SerializeField] private AudioClip linkLoad;
    [SerializeField] private AudioClip sliderChange;
    [SerializeField] private AudioClip toggle;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetPitch(float pitch)
    {
        source.pitch = pitch;
    }

    public void PlayButtonClick()
    {
        source.PlayOneShot(buttonClick);
    }

    public void PlayPanelOpen()
    {
        source.PlayOneShot(panelOpen);
    }

    public void PlayPanelClose()
    {
        source.PlayOneShot(panelClose);
    }

    public void PlayLinkLoad()
    {
        source.PlayOneShot(linkLoad);
    }

    public void PlaySliderChange()
    {
        source.PlayOneShot(sliderChange);
    }

    public void PlayToggle()
    {
        source.PlayOneShot(toggle);
    }

}
