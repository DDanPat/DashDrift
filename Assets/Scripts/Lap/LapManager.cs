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
        lapUI.LapTextUpdate(0, totalLaps);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponentInParent<LapController>())
        {
            LapController player = collider.gameObject.GetComponentInParent<LapController>();
            if (player.checkpointCount == checkpoints.Count)
            {
                player.lapCount++;
                lapUI.LapTextUpdate(player.lapCount, totalLaps);
                player.checkpointCount = 0;

                lapTimer.EndLap();

                if (player.lapCount > totalLaps)
                {
                    // End race logic here
                    lapTimer.EndTimer();
                }
            }
        }        
    }
}
