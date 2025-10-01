using UnityEngine;
using UnityEngine.UI;

public class CarSlots : MonoBehaviour
{
    [SerializeField] private CarData carData;


    public void Init(CarData data)
    {
        carData = data;
        // Here you can set up the UI elements like car icon, name, price, etc.
        // For example:
        // carIconImage.sprite = carData.carIcon;
        // carNameText.text = carData.carName;
        // carPriceText.text = carData.carPrice.ToString();
    }
}
