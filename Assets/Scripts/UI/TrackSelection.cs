using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TrackSelection : MonoBehaviour
{
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
        aiCountDropdown.value = 0; // Default to first option
        aiGradeDropdown.value = 0; // Default to first option
        dataManager.SetRaceSettings(1);
        dataManager.SetRaceSettings("Easy");


        aiCountDropdown.onValueChanged.AddListener(SetAICount);
        aiGradeDropdown.onValueChanged.AddListener(SetAIGrade);
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
