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
        Debug.LogError("PlayButtonClick");
    }

    public void PlayPanelOpen()
    {
        source.PlayOneShot(panelOpen);
        Debug.LogError("PlayPanelOpen");
    }

    public void PlayPanelClose()
    {
        source.PlayOneShot(panelClose);
        Debug.LogError("PlayPanelClose");
    }

    public void PlayLinkLoad()
    {
        source.PlayOneShot(linkLoad);
        Debug.LogError("PlayLinkLoad");
    }

    public void PlaySliderChange()
    {
        source.PlayOneShot(sliderChange);
        Debug.LogError("PlaySliderChange");
    }

    public void PlayToggle()
    {
        source.PlayOneShot(toggle);
        Debug.LogError("PlayToggle");
    }

}
