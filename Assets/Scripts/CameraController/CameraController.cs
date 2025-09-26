using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineFollow CinemachineFollow;

    public Vector3[] CameraPositions;
    int index = 0;

    public void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            index = (index < CameraPositions.Length - 1) ? index++ : 0;

            CinemachineFollow.FollowOffset = CameraPositions[index];
        }
    }
}
