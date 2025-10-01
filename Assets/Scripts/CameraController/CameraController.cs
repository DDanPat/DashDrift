using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineFollow CinemachineFollow;

    public Vector3[] CameraPositions;
    int index = 0;

    public TextMeshProUGUI cameraIndex;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            index = (index < CameraPositions.Length - 1) ? index + 1 : 0;

            CinemachineFollow.FollowOffset = CameraPositions[index];
            //cameraIndex.text = "Camera" + (index + 1).ToString();
        }
    }
}
