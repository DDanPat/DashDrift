using UnityEngine;

public class DataManager : MonoBehaviour
{
    public string RaceTrack { get; private set; }
    public int AICount { get; private set; }
    public string AIGrade { get; private set; }


    public void SetRaceTrack(string track)
    {
        RaceTrack = track;
        Debug.Log("Track set to: " + RaceTrack);
    }

    public void SetRaceSettings(int carCount)
    {
        AICount = carCount;
        Debug.Log("Number of AI Cars set to: " + AICount);
    }

    public void SetRaceSettings(string grade)
    {
        AIGrade = grade;
        Debug.Log("AI Grade set to: " + AIGrade);
    }
}
