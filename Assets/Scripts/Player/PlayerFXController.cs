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
    [SerializeField] private ParticleSystem groundFX;
    [SerializeField] private ParticleSystem grassFX;

    private Dictionary<SurfaceType, ParticleSystem> surfaceFX;

    [Networked, OnChangedRender("OnChangeSurface")] private bool _isOnGround { get; set; }

    public override void Spawned()
    {
        surfaceFX = new Dictionary<SurfaceType, ParticleSystem>
        {
            { SurfaceType.Ground, groundFX },
            { SurfaceType.Grass, grassFX }
        };

        _isOnGround = true;
        OnChangeSurface();
    }

    public void SetIsOnGround(bool isOnGround)
    {
        _isOnGround = isOnGround;

    }

    private void OnChangeSurface()
    {
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
