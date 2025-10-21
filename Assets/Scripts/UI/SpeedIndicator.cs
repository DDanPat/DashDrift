using TMPro;
using UnityEngine;

public class SpeedIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speedText;

    private float currentSpeed;

    private CarController carController;

    public void SetTargetCar(CarController car)
    {
        if (car != null)
        {
            carController = car;
            Debug.Log("✅ SpeedIndicator: 플레이어 차량 참조 설정 완료.");
            // 차량이 설정된 직후 UI를 초기화할 수 있습니다.
            speedText.text = "0";
        }
        else
        {
            Debug.LogError("SpeedIndicator: 전달받은 차량 참조가 Null입니다.");
        }
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
