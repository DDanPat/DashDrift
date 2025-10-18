using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    public List<Checkpoint> checkpoints;
    public int totalLaps = 3;

    public LapUI lapUI;

    public LapTimer lapTimer;

    private void Start()
    {
        lapUI.LapTextUpdate(1, totalLaps);
    }

    private void OnTriggerEnter(Collider collider)
    {
        LapController lapController = collider.gameObject.GetComponentInParent<LapController>();

        if (lapController.gameObject.tag == "Player")
        {
            if (lapController.checkpointCount == checkpoints.Count)
            {
                lapController.lapCount++;
                lapController.checkpointCount = 0;

                lapTimer.EndLap();

                if (lapController.lapCount < totalLaps)
                {
                    lapUI.LapTextUpdate(lapController.lapCount, totalLaps);
                }

                if (lapController.lapCount > totalLaps)
                {
                    // End race logic here
                    lapTimer.EndTimer();

                    // 3초 후에 로비 화면으로 씬 전환
                    StartCoroutine(EndGame());

                    // TODO: 결과 화면 UI 표시 로직 추가 필요 및 결과 화면에서 아무키 입력 시 로비 씬으로 전환

                }
            }
        }
        else
        {
            // AI 차량의 경우 체크포인트만 갱신
            if (lapController.checkpointCount == checkpoints.Count)
            {
                lapController.lapCount++;
                lapController.checkpointCount = 0;

                if (lapController.lapCount > totalLaps)
                {
                    lapController.gameObject.GetComponent<AICarController>().StopAICar();
                    // AI 차량이 레이스를 완료했을 때의 로직 (필요 시 추가)
                }
            }
        }
    }
    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(3f);
        // 로비 화면으로 씬 전환 로직 추가 필요
        SceneLoader.LoadGameScene("Garage");
    }
}
