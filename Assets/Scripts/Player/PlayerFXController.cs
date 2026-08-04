using Fusion;
using System.Collections.Generic;
using UnityEngine;

public enum SurfaceType
{
    None = 0,
    Grass,
    Ground,
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

    private Dictionary<SurfaceType, ParticleSystem> surfaceFX;
    private Dictionary<SurfaceType, AudioClip> surfaceSFX;

    [Networked, OnChangedRender("OnChangeSurface")] private bool _isOnGround { get; set; }

    public override void Spawned()
    {
        surfaceFX = new Dictionary<SurfaceType, ParticleSystem>
        {
            { SurfaceType.Ground, groundFX },
            { SurfaceType.Grass, grassFX }
        };

        surfaceSFX = new Dictionary<SurfaceType, AudioClip>
        {
            { SurfaceType.Ground, groundSFX },
            { SurfaceType.Grass, grassSFX }
        };

        _isOnGround = true;
        OnChangeSurface();
    }

    public void SetIsOnGround(bool isOnGround)
    {
        if (_isOnGround != isOnGround)
            _isOnGround = isOnGround;
    }

    public void PlayFootstep()
    {
        audioSource.Play();
    }

    private void OnChangeSurface()
    {
        audioSource.Stop();
        audioSource.clip = _isOnGround ? surfaceSFX[SurfaceType.Ground] : surfaceSFX[SurfaceType.Grass];

        if (_isOnGround)
        {
            surfaceFX[SurfaceType.Ground].Play();
            surfaceFX[SurfaceType.Grass].Stop();
        }
        else
        {
            surfaceFX[SurfaceType.Ground].Stop();
            surfaceFX[SurfaceType.Grass].Play();
        }
    }
}
