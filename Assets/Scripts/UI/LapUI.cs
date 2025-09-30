using TMPro;
using UnityEngine;

public class LapUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lapText;


    public void LapTextUpdate(int lapCount, int totalLaps)
    {
        lapText.text = $"{lapCount}/{totalLaps}";
    }
}
