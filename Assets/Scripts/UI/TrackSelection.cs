using TMPro;
using UnityEngine;

public class TrackSelection : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown trackDropdown;
    [SerializeField] private TMP_Dropdown aiCountDropdown;
    [SerializeField] private TMP_Dropdown aiGradeDropdown;


    private DataManager dataManager;

    private void Start()
    {
        dataManager = GameManager.Instance.DataManager;
        Init();
    }

    private void Init()
    {
        trackDropdown.value = 0; // Default to first option
        aiCountDropdown.value = 0; // Default to first option
        aiGradeDropdown.value = 0; // Default to first option
        dataManager.SetRaceTrack("Track001");
        dataManager.SetRaceSettings(0);
        dataManager.SetRaceSettings("Easy");


        trackDropdown.onValueChanged.AddListener(SetTrack);
        aiCountDropdown.onValueChanged.AddListener(SetAICount);
        aiGradeDropdown.onValueChanged.AddListener(SetAIGrade);
    }

    private void SetTrack(int value)
    {
        switch(value)
        {
            case 0:
                dataManager.SetRaceTrack("Track001");
                break;
            case 1:
                dataManager.SetRaceTrack("Track002");
                break;
            case 2:
                dataManager.SetRaceTrack("Track003");
                break;
            default:
                dataManager.SetRaceTrack("Track001");
                break;
        }
    }

    private void SetAICount(int value)
    {
        dataManager.SetRaceSettings(value);
    }

    private void SetAIGrade(int value)
    {
        string grade;
        switch (value)
        {
            case 1:
                grade = "Normal";
                break;
            case 2:
                grade = "Hard";
                break;
            default:
                grade = "Easy";
                break;
        }
        dataManager.SetRaceSettings(grade);
    }
}
