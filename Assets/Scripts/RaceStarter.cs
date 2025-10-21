using System.Collections;
using TMPro;
using UnityEngine;

public class RaceStarter : MonoBehaviour
{
    [SerializeField] private CarSpawner carSpawner;
    [SerializeField] private AISpawner aiSpawner;
    [SerializeField] private TextMeshProUGUI countdownText;
    private bool isRaceStarted = false;

    private void Start()
    {
        carSpawner.SpawnSelectedCar();
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        //if (countdownText == null)
        //{
        //    Debug.LogError("Countdown Text가 설정되지 않았습니다.");
        //    yield break;
        //}

        // 3초 카운트다운
        for (int i = 3; i > 0; i--)
        {
            //countdownText.text = i.ToString();
            Debug.Log(i.ToString());
            yield return new WaitForSeconds(1f);
        }

        // 출발 표시
        //countdownText.text = "출발!";
        Debug.Log("출발!");
        yield return new WaitForSeconds(1f);

        foreach (var aiCar in aiSpawner.aiCarPrefabs)
        {
            aiCar.GetComponent<AICarController>().RaceStart();
        }
        // 텍스트 숨기기
        //countdownText.gameObject.SetActive(false);
        isRaceStarted = true;
    }

    public bool IsRaceStarted()
    {
        return isRaceStarted;
    }
}
