using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [SerializeField] private DataManager dataManager;

    public DataManager DataManager => dataManager;
    // 다음 씬에서 소환할 차량 컨트롤러 프리팹을 저장할 변수
    [SerializeField] private GameObject selectedCarControllerPrefab;

    public void SetSelectedCar(GameObject carControllerPrefab)
    {
        selectedCarControllerPrefab = carControllerPrefab;
    }

    public GameObject GetSelectedCarPrefab()
    {
        return selectedCarControllerPrefab;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 씬 전환 시 파괴되지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 이미 존재하면 중복 객체 파괴
            Destroy(gameObject);
        }
    }
}
