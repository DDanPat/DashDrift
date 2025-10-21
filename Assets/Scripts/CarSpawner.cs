using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint; // 차량이 소환될 위치

    public void SpawnSelectedCar()
    {
        if (GameManager.Instance != null)
        {
            // GameManager에서 선택된 차량 프리팹을 가져옴
            GameObject selectedCarPrefab = GameManager.Instance.GetSelectedCarPrefab();

            if (selectedCarPrefab != null)
            {
                GameObject carController = Instantiate(selectedCarPrefab, spawnPoint.position, spawnPoint.rotation);
                
                SpeedIndicator speedIndicator = FindFirstObjectByType<SpeedIndicator>();

                if (speedIndicator != null)
                {
                    // SpeedIndicator에 차량 컨트롤러를 설정
                    speedIndicator.SetTargetCar(carController.GetComponent<CarController>());
                }
                else
                {
                    Debug.LogError("SpeedIndicator 컴포넌트를 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다. 메뉴 씬에서 시작했는지 확인하세요.");
        }
    }
}
