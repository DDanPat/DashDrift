using UnityEngine;

public class AISpawner : MonoBehaviour
{
    private DataManager dataManager;
    // TODO : AI 차량 소환에 필요한 변수들 추가
    // 예: public GameObject aiCarPrefab;
    //      public Transform[] spawnPoints;


    private void Start()
    {
        // 1. GameManager를 통해 DataManager에 접근하여 설정값을 가져옵니다.
        dataManager = GameManager.Instance.DataManager;

        int count = dataManager.NumberOfAICars;
        string grade = dataManager.AIGrade;

        Debug.Log($"AI 차량 {count}대 ({grade}) 소환 시작.");

        // 2. 가져온 값으로 소환 로직 실행...
    }

    private void SpownAICar()
    {
        // AI 차량 소환 로직 구현

    }
}
