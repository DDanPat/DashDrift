using UnityEngine;
using UnityEngine.UI;

public class GarageUI : MonoBehaviour
{
    [Header("Game Scene Settings")]
    [SerializeField] private string inGameSceneName = "GameScene"; // 로드할 인게임 씬 이름
    [SerializeField] private Button startGameButton; // 게임 시작 버튼 (선택 사항)


    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
        startGameButton.onClick.AddListener(OnGameStartButtonClicked);
    }

    private void OnGameStartButtonClicked()
    {
        inGameSceneName = gameManager.DataManager.RaceTrack;
        SceneLoader.LoadGameScene(inGameSceneName);
    }
}
