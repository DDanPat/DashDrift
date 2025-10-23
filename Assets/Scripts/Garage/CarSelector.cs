using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CarData
{
    public string carName;
    public GameObject carPrefab;
    public GameObject carControllerPrefab;
    public Sprite carIcon;
    public float carPrice;
    public bool isUnlocked;
}

public class CarSelector : MonoBehaviour
{
    [SerializeField] private CarData[] carData;
    [SerializeField] private Button carSlot;
    [SerializeField] private Transform content;

    [SerializeField] private Transform carDisplayPoint;

    [Header("Current Car Stats")]
    [SerializeField] private TextMeshProUGUI maxSpeedValue;
    [SerializeField] private TextMeshProUGUI accelerationValue;
    [SerializeField] private TextMeshProUGUI handlingValue;
    [SerializeField] private TextMeshProUGUI brakeValue;


    private GameObject currentCarModel;


    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        PopulateCarSlots();

        Init();
    }

    // 처음 시작시 슬롯에 index 0번의 차량을 기본 선택을 해줌
    private void Init()
    {
        OnCarSelected(carData[0]);
        this.gameObject.SetActive(false);
    }

    private void PopulateCarSlots()
    {
        foreach (var carData in carData)
        {
            Button newButton = Instantiate(carSlot, content);
            // Assuming CarSlots is another script that handles individual car slot UI
            CarSlots carSlotComponent = newButton.GetComponent<CarSlots>();
            carSlotComponent.Init(carData);
            newButton.onClick.AddListener(() => OnCarSelected(carData));
        }
    }

    private void OnCarSelected(CarData carData)
    {
        if (currentCarModel != null)
        {
            Destroy(currentCarModel);
        }
        currentCarModel = Instantiate(carData.carPrefab, carDisplayPoint.position, Quaternion.identity, carDisplayPoint);
       
        gameManager.SetSelectedCar(carData.carControllerPrefab);

        CarStatsView(carData);
    }

    private void CarStatsView(CarData carData)
    {
        CarStats carStats = carData.carControllerPrefab.GetComponent<CarStats>();

        maxSpeedValue.text = carStats.MaxSpeed.ToString("F1");
        accelerationValue.text = carStats.Acceleration.ToString("F2");
        handlingValue.text = carStats.SteelStrength.ToString("F2");
        //brakeValue.text = carStats.BrakeForce.ToString("F2");
    }
}
