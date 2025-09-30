using UnityEngine;

public class LapController : MonoBehaviour
{
    public int lapCount = 0;
    public int checkpointCount = 0;

    private void Start()
    {
        lapCount = 1;
        checkpointCount = 0;
    }
}
