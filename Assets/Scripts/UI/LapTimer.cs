using System.Collections;
using UnityEngine;
using TMPro; // UI TextMeshPro를 사용하는 경우

public class LapTimer : MonoBehaviour
{
    // UI 표시를 위한 변수 추가 (LapManager에서 처리할 수도 있지만, 여기에서 직접 관리하도록 가정)
    [Header("UI References")]
    public TextMeshProUGUI totalTimeText; // 총 시간 표시
    public TextMeshProUGUI lapTimeText;   // 현재 랩 시간 표시
    public TextMeshProUGUI bestLapTimeText; // 최적 랩 시간 표시

    private LapManager lapManager;

    private float totalTime;     // 총 경과 시간 (레이스 전체)
    private float lapTime;       // 현재 랩 타임 (0부터 시작)
    private float bestLapTime;   // 최적 랩 타임
    private bool isTiming = false;

    private void Start()
    {
        // 동일 오브젝트에 LapManager가 있다고 가정합니다.
        lapManager = GetComponent<LapManager>();

        // 최적 랩 타임 초기화 (UI도 업데이트)
        bestLapTime = float.MaxValue;
        UpdateBestLapTimeUI(bestLapTime);

        StartLapTimer();
    }

    // --- 타이머 제어 메소드 ---

    public void StartLapTimer()
    {
        if (isTiming) return; // 이미 실행 중이면 무시

        isTiming = true;
        totalTime = 0f;
        lapTime = 0f;
        // bestLapTime은 Start()에서 한 번만 초기화하거나 별도 Reset 시 초기화합니다.

        // UI 초기화 (선택 사항)
        if (totalTimeText != null) totalTimeText.text = FormatTime(0);
        if (lapTimeText != null) lapTimeText.text = FormatTime(0);

        StartCoroutine(UpdateLapTimer());
        Debug.Log("Lap Timer Started.");
    }

    /// <summary>
    /// 현재 랩을 종료하고 새 랩을 시작하는 로직.
    /// LapManager 또는 FinishLine 스크립트에서 호출됩니다.
    /// </summary>
    public void EndLap()
    {
        if (!isTiming) return;

        // 1. 현재 랩 타임 기록 및 최적 랩 타임 갱신
        string currentLapTimeFormatted = FormatTime(lapTime);
        Debug.Log($"Lap Finished! Time: {currentLapTimeFormatted}");

        if (lapTime < bestLapTime)
        {
            bestLapTime = lapTime;
            UpdateBestLapTimeUI(bestLapTime);
            Debug.Log($"New Best Lap Time: {currentLapTimeFormatted}");
        }

        // 2. 랩 타임 초기화 (코루틴은 계속 실행)
        lapTime = 0f;

        // LapManager가 있다면 다음 랩으로 진행시키는 로직 호출
        if (lapManager != null)
        {
            // 예: lapManager.NextLap();
        }
    }

    public void EndTimer()
    {
        isTiming = false;
        StopAllCoroutines(); // 모든 코루틴을 중지
        Debug.Log($"Race Finished! Total Time: {FormatTime(totalTime)}");

        // 레이스 종료 후 필요한 로직 (예: 결과 화면 표시 등)
    }

    // --- 코루틴 ---

    private IEnumerator UpdateLapTimer()
    {
        while (isTiming)
        {
            // 총 시간 및 랩 시간 업데이트
            totalTime += Time.deltaTime;
            lapTime += Time.deltaTime;

            // UI 업데이트
            if (totalTimeText != null) totalTimeText.text = FormatTime(totalTime);
            if (lapTimeText != null) lapTimeText.text = FormatTime(lapTime);

            yield return null; // 다음 프레임까지 대기
        }
    }

    // --- 헬퍼 메소드 ---

    /// <summary>
    /// float 타입의 시간을 분:초.000 형식의 문자열로 변환합니다.
    /// </summary>
    private string FormatTime(float timeInSeconds)
    {
        // 최적 랩 타임이 float.MaxValue인 경우 "--:--.---"를 반환
        if (timeInSeconds == float.MaxValue)
        {
            return "--:--.---";
        }

        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        float seconds = timeInSeconds % 60f;

        // 분을 두 자릿수로, 초를 두 자릿수와 소수점 이하 세 자릿수로 포맷
        return string.Format("{0:00}:{1:00.000}", minutes, seconds);
    }

    /// <summary>
    /// 최적 랩 타임 UI를 업데이트합니다.
    /// </summary>
    private void UpdateBestLapTimeUI(float time)
    {
        if (bestLapTimeText != null)
        {
            bestLapTimeText.text = FormatTime(time);
        }
    }
}