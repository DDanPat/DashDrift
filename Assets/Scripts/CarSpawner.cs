using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint; // 차량이 소환될 위치

    private void Start()
    {
        SpawnSelectedCar();
    }

    private void SpawnSelectedCar()
    {
        if (GameManager.Instance != null)
        {
            // GameManager에서 선택된 차량 프리팹을 가져옴
            GameObject selectedCarPrefab = GameManager.Instance.GetSelectedCarPrefab();

            if (selectedCarPrefab != null)
            {
                Instantiate(selectedCarPrefab, spawnPoint.position, spawnPoint.rotation);
            }
            else
            {
                Debug.LogError("선택된 차량이 없습니다. 기본 차량을 소환하거나 초기화면으로 돌아가게 해야 합니다.");
            }

            // 데이터 사용 후 GameManager에서 데이터 초기화 (선택 사항)
            //GameManager.Instance.SetSelectedCar(null);
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다. 메뉴 씬에서 시작했는지 확인하세요.");
        }
    }
}
