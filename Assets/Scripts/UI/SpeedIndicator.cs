using TMPro;
using UnityEngine;

public class SpeedIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;

    private float currentSpeed;

    public void SpeedUpdate(float speed)
    {
        currentSpeed = Mathf.Lerp(currentSpeed, speed, 0.1f);

        int intSpeed = Mathf.FloorToInt(currentSpeed + 0.5f);

        speedText.text = intSpeed.ToString();
    }


}
