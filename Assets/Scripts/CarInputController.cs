using UnityEngine;

public class CarInputController : MonoBehaviour
{
    private CarController carController;

    [Header("Input System")]
    public CarInputSystem carInput { get; private set; }
    public CarInputSystem.CarActions carActions { get; private set; }



    private void Awake()
    {
        carController = GetComponent<CarController>();

        carInput = new CarInputSystem();
        carActions = carInput.Car;
    }
    private void OnEnable()
    {
        carInput.Enable();
    }

    private void OnDisable()
    {
        carInput.Disable();
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        Vector2 moveVector = carInput.Car.Move.ReadValue<Vector2>();

        float moveInput = moveVector.y;
        float steerInput = moveVector.x;

        bool isBraking = carInput.Car.Brake.inProgress;
        bool isDrifting = carInput.Car.Drift.inProgress;

        carController.GetPlayerInput(moveInput, steerInput, isBraking, isDrifting);
    }
}
