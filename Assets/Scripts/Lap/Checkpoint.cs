using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.GetComponentInParent<LapController>())
        {
            LapController player = collider.gameObject.GetComponentInParent<LapController>();
            if (player.checkpointCount == index )
            {
                player.checkpointCount += 1;
                Debug.Log("Checkpoint reached: " + index);
            }
        }
    }
}
