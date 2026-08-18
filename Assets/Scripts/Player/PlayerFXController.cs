using Fusion;
using UnityEngine;

public enum SurfaceType
{
    Grass,
    Ground,
    Metal
}

public class PlayerFXController : NetworkBehaviour
{
    [Header("FX")]
    [SerializeField] private ParticleSystem groundFX;
    [SerializeField] private ParticleSystem grassFX;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip groundSFX;
    [SerializeField] private AudioClip grassSFX;
    [SerializeField] private AudioClip metalSFX;

    [Networked, OnChangedRender(nameof(OnChangeSurface))]
    private SurfaceType _surfaceType { get; set; }

    public override void Spawned()
    {
        _surfaceType = SurfaceType.Ground;
        OnChangeSurface();
    }

    public void SetSurfaceType(SurfaceType surfaceType)
    {
        if (_surfaceType != surfaceType)
            _surfaceType = surfaceType;
    }

    public void PlayFootstep()
    {
        //audioSource.Play();
    }

    private void OnChangeSurface()
    {
        audioSource.Stop();

        switch (_surfaceType)
        {
            case SurfaceType.Grass:
                audioSource.clip = grassSFX;
                groundFX.Stop();
                grassFX.Play();
                break;

            case SurfaceType.Ground:
                audioSource.clip = groundSFX;
                grassFX.Stop();
                groundFX.Play();
                break;

            case SurfaceType.Metal:
                audioSource.clip = metalSFX;
                grassFX.Stop();
                groundFX.Play();
                break;
        }
    }
}