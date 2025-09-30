using TMPro;
using UnityEngine;

public class SpeedIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;

    private float currentSpeed;


    private CarController carController;

    private void Start()
    {
        carController = FindAnyObjectByType<CarController>();
        currentSpeed = 0f;
    }

    private void Update()
    {
        SpeedUpdate();
    }

    public void SpeedUpdate()
    {
        float speed = carController.GetCurrentSpeed();

        currentSpeed = Mathf.Lerp(currentSpeed, speed, 0.1f);

        int intSpeed = Mathf.FloorToInt(currentSpeed + 0.5f);

        speedText.text = intSpeed.ToString();
    }


}
