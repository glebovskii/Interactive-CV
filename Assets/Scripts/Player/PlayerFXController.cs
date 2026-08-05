using Fusion;
using System;
using System.Collections.Generic;
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

    private Dictionary<SurfaceType, ParticleSystem> surfaceFX;
    private Dictionary<SurfaceType, AudioClip> surfaceSFX;

    [Networked, OnChangedRender("OnChangeSurface")] private SurfaceType _surfaceType { get; set; }

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
            { SurfaceType.Grass, grassSFX },
            { SurfaceType.Metal, metalSFX }
        };

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
        audioSource.Play();
    }

    private void OnChangeSurface()
    {
        audioSource.Stop();
        switch(_surfaceType)
        {
            case SurfaceType.Ground:
                audioSource.clip = surfaceSFX[SurfaceType.Ground]; 
                break;
            case SurfaceType.Grass:
                audioSource.clip = surfaceSFX[SurfaceType.Grass];
                break;
            case SurfaceType.Metal:
                audioSource.clip = surfaceSFX[SurfaceType.Metal];
                break;
        }

        if (_surfaceType != SurfaceType.Grass)
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
