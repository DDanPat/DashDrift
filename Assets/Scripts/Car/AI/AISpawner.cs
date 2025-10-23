using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    private List<GameObject> aiCars = new List<GameObject>();
    public IReadOnlyList<GameObject> AICars => aiCars;

    public GameObject[] aiCarPrefabs;

    [SerializeField] private Transform[] spawnPoints;

    private int aiCarCount;
    private string aiCargrade;

    private DataManager dataManager;

    private void Start()
    {
        // 1. GameManager를 통해 DataManager에 접근하여 설정값을 가져옵니다.
        dataManager = GameManager.Instance.DataManager;

        aiCarCount = dataManager.AICount;
        aiCargrade = dataManager.AIGrade;

        Debug.Log($"AI 차량 {aiCarCount}대 ({aiCargrade}) 소환 시작.");

        SpownAICar();
    }

    private void SpownAICar()
    {
        // AI 차량 소환 로직 구현

        if (aiCarCount == 0) return;

        aiCars.Clear();

        for (int i = 0; i < aiCarCount; i++)
        {
            if (i >= spawnPoints.Length)
            {
                Debug.LogWarning("Not enough spawn points for the number of AI cars.");
                break;
            }
            // AI 차량 프리팹 선택 (예: 등급에 따라 다르게 선택)

            // AI 차량 소환
            GameObject aiCar = Instantiate(aiCarPrefabs[i], spawnPoints[i].position, spawnPoints[i].rotation);

            aiCars.Add(aiCar);
            
        }

    }
}
