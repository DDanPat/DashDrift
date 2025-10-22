using System.Collections;
using TMPro;
using UnityEngine;

public enum RaceMode
{
    Career,
    TimeAttack,
    FreeRide
}

public class RaceStarter : MonoBehaviour
{
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private AISpawner aiSpawner;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private RaceMode raceMode;

    public bool isRaceStarted = false;

    private void Start()
    {
        carSpawner.SpawnSelectedCar();
        HandleGameModeStart();
    }

    private void HandleGameModeStart()
    {
        switch(raceMode)
        {
            case RaceMode.Career:
                StartCoroutine(StartCountdown());
                break;
            case RaceMode.TimeAttack:
                StartCoroutine(StartCountdown());
                break;
            case RaceMode.FreeRide:
                isRaceStarted = true;
                carSpawner.spawnedCar.GetComponent<CarController>().raceStarted = true;
                break;
        }
    }

    private IEnumerator StartCountdown()
    {

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            Debug.Log(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        // 출발 표시
        countdownText.text = "출발!";
        Debug.Log("출발!");
        yield return new WaitForSeconds(1f);

        foreach (var aiCar in aiSpawner.AICars)
        {
            aiCar.GetComponent<CarController>().raceStarted = true;
            aiCar.GetComponent<AICarController>().RaceStart();
        }

        carSpawner.spawnedCar.GetComponent<CarController>().raceStarted = true;
        // 텍스트 숨기기
        countdownText.gameObject.SetActive(false);
        isRaceStarted = true;
    }

    public bool IsRaceStarted()
    {
        return isRaceStarted;
    }
}
