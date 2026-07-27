using Unity.Cinemachine;
using UnityEngine;

public class CameraBrainController : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain;

    private void Update()
    {
        brain.ManualUpdate();
        //brain.UpdateCameraState(Vector3.up, Time.deltaTime);
    }
}
