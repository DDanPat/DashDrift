using UnityEngine;

public class DataManager : MonoBehaviour
{
    public int NumberOfAICars { get; private set; }
    public string AIGrade { get; private set; }

    public void SetRaceSettings(int carCount)
    {
        NumberOfAICars = carCount;
        Debug.Log("Number of AI Cars set to: " + NumberOfAICars);
    }

    public void SetRaceSettings(string grade)
    {
        AIGrade = grade;
        Debug.Log("AI Grade set to: " + AIGrade);
    }
}
