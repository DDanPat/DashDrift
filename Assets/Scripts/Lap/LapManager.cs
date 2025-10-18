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
        if (collider.gameObject.GetComponentInParent<LapController>())
        {
            LapController player = collider.gameObject.GetComponentInParent<LapController>();
            if (player.checkpointCount == checkpoints.Count)
            {
                player.lapCount++;
                player.checkpointCount = 0;

                lapTimer.EndLap();

                if (player.lapCount < totalLaps)
                {
                    lapUI.LapTextUpdate(player.lapCount, totalLaps);
                }

                if (player.lapCount > totalLaps)
                {
                    // End race logic here
                    lapTimer.EndTimer();

                    // 3초 후에 로비 화면으로 씬 전환
                    StartCoroutine(EndGame());
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
