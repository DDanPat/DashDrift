using UnityEngine;

public class CarManager : MonoBehaviour
{
    private CarController carController;
    private SpeedIndicator speedIndicator;

    private void Start()
    {
        carController = GetComponent<CarController>();
        speedIndicator = GetComponent<SpeedIndicator>();
    }


    private void Update()
    {
        //speedIndicator.SpeedUpdate(carController.GetCurrentSpeed());
    }
}
