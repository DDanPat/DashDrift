using System;
using UnityEngine;

public class KartController : MonoBehaviour
{
    public float motorForce = 100f;
    public float brakeForce = 1000f;
    public float maxSteerAngle = 30f;

    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    private float HorizontalInput;
    private float VerticalInput;
    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isBraking;

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }


    private void GetInput()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);

    }

    private void HandleMotor()
    {
        frontLeftWheel.motorTorque = VerticalInput * motorForce;
        frontRightWheel.motorTorque = VerticalInput * motorForce;

        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        frontLeftWheel.brakeTorque = currentBrakeForce;
        frontRightWheel.brakeTorque = currentBrakeForce;
        rearLeftWheel.brakeTorque = currentBrakeForce;
        rearRightWheel.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * HorizontalInput;
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    private void UpdateSingleWheels(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheels(frontLeftWheel, frontLeftTransform);
        UpdateSingleWheels(frontRightWheel, frontRightTransform);
        UpdateSingleWheels(rearLeftWheel, rearLeftTransform);
        UpdateSingleWheels(rearRightWheel, rearRightTransform);
    }
}
