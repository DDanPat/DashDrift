using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineFollow CinemachineFollow;

    public Vector3[] CameraPositions;
    int index = 0;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Camera Position Changed");
            Debug.Log(index);
            index = (index < CameraPositions.Length - 1) ? index + 1 : 0;

            CinemachineFollow.FollowOffset = CameraPositions[index];
        }
    }
}
