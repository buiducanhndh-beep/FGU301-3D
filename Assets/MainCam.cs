using Unity.Cinemachine;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    public CinemachineVirtualCameraBase freeLookCam;
    public float rotationY;

    private void Update()
    {
        var state = freeLookCam.State;
        var rotation = state.GetFinalOrientation();
        rotationY = rotation.eulerAngles.y;
    }

    // Use UnityEngine.Quaternion instead of Unity.Mathematics.quaternion
    public Quaternion flatrotation => Quaternion.Euler(0, rotationY, 0);
}