using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class POIController : MonoBehaviour
{
    [SerializeField] private List<PointOfInterest> pois;

    private PlayerSpawner spawner;

    private void Start()
    {
        if (ServiceLocator.TryGet(out spawner))
        {
            spawner.OnPlayerSpawned += InitPlayerPOI;
        }
    }

    private void InitPlayerPOI(NetworkObject netObject)
    {
        if (netObject.gameObject.TryGetComponent<PlayerView>(out var view))
        {
            view.SetPOI(pois);
        }
    }


    private void OnDestroy()
    {
        if (spawner)
            spawner.OnPlayerSpawned -= InitPlayerPOI;
    }
}
