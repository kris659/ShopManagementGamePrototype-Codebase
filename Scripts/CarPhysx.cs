using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPhysx : MonoBehaviour, IVehicle
{
    public GameObject PlayerControlling { get; set; }
    public int PrefabIndex { get; set; }

    public bool IsThirdPersonCamera { get { return isThirdPersonCamera; } set { isThirdPersonCamera = value; } }
    [SerializeField] private bool isThirdPersonCamera;
    public GameObject ThirdPersonCamera { get { return thirdPersonCamera; } }
    [SerializeField] private GameObject thirdPersonCamera;

    public Transform Transform { get { return tr; } }
    [SerializeField] private Transform tr;
    public Transform PlayerPosition { get { return playerPosition; } }
    [SerializeField] private Transform playerPosition;
    public Transform GettingOutPosition { get { return gettingOutPosition; } }
    [SerializeField] private Transform gettingOutPosition;

    public float PlayerScale { get { return playerScale; } }

    [SerializeField] private float playerScale;

    [SerializeField] private Transform steeringWheel;
    [SerializeField] private Vector3 steeringWheelRotation;
    [SerializeField] private Transform[] wheels;
    [SerializeField] private Transform[] wheelsVisual;
    [SerializeField] private float wheelRadius;

    [SerializeField] AnimationCurve _powerCurve;
    [SerializeField] AnimationCurve _reversingPowerCurve;
    [SerializeField] private float _carTopSpeed;
    [SerializeField] private float _carTopReversingSpeed;
    [SerializeField] private float _carAcceleration;
    [Range(1f, 5f)]
    [SerializeField] private float _carBraking = 1.5f;

    [Range(0f, 2f)]
    [SerializeField] private float _idleDrag;

    [SerializeField] private float _suspensionForce;
    [SerializeField] private float _suspesionDamping;
    [SerializeField] private float _suspensionRestDistance;
    [SerializeField] private float _rayLength;

    [Range(0f, 1f)]
    [SerializeField] private float _tireGrip = 0.5f;
    [SerializeField] private float _tireMass;

    [SerializeField] private float maxWheelsAngle;
    [SerializeField] private float wheelRotationSpeed;
    [SerializeField] private AnimationCurve wheelRotationMultCurve;
    [SerializeField] private bool invertedSteering;

    [SerializeField] private LayerMask _layerMask;

    private Rigidbody rb;
    private LiftController liftController;

    Vector2 currentInput;
    Vector3 previousCarPosition;

    public UpdateCarState OnVehicleEnter { get; set; }
    public UpdateCarState OnVehicleLeave { get; set; }

    Quaternion frontWheelsAngle { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        liftController = GetComponent<LiftController>();

        previousCarPosition = transform.position;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(wheels[i].position, -wheels[i].up, out hit, _rayLength, _layerMask))
            {
                //Debug.DrawRay(wheels[i].position, -wheels[i].up * hit.distance, Color.yellow, 0.1f);

                Vector3 suspensionForce = GetSuspensionForce(wheels[i], hit.distance);
                Vector3 steeringForce = GetSteeringForce(wheels[i]) / rb.mass;
                Vector3 dragForce = Vector3.zero;
                if (PlayerControlling == null)
                {
                    Vector3 accelDirection = wheels[i].forward;
                    float carSpeed = Vector3.Dot(transform.forward, rb.velocity);
                    if (Mathf.Abs(carSpeed) <= 0.1f) dragForce = -accelDirection * carSpeed * 1;
                    else
                        if (Mathf.Abs(carSpeed) <= 0.5f) dragForce = -accelDirection * carSpeed * 0.4f;
                        else dragForce = -accelDirection * carSpeed * _idleDrag / 5;
                    if (carSpeed < 0 && dragForce != Vector3.zero) // Nie wiem dlaczego jest obrócony -3.5 stopnia wiêc nie dzia³a bez tego
                        dragForce += transform.TransformDirection(new Vector3(0, 0, 0.1f));
                }
                Vector3 finalForce = suspensionForce + steeringForce + dragForce;
                rb.AddForceAtPosition(finalForce * rb.mass, wheels[i].position);
            }
            else
            {
                //Debug.DrawRay(wheels[i].position, -wheels[i].up * _rayLength, Color.blue, 0.1f);
            }
        }
    }

    public void HandleVehicleMovement(InputData data)
    {
        currentInput = new Vector2(data.direction.x, data.direction.z);
        RotateFrontWheels();

        for (int i = 0; i < wheels.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(wheels[i].position, -wheels[i].up, out hit, _rayLength, _layerMask))
            {
                //Debug.DrawRay(wheels[i].position, -wheels[i].up * hit.distance, Color.yellow, 0.1f);

                Vector3 accelerationForce = GetAccelerationForce(wheels[i]);
                rb.AddForceAtPosition(accelerationForce * rb.mass, wheels[i].position);
            }
            else
            {
                //Debug.DrawRay(wheels[i].position, -wheels[i].up * _rayLength, Color.blue, 0.1f);
            }
        }

        if (liftController != null) liftController.HandleInput(data.buttonE || data.buttonCapsLock, data.buttonQ || data.buttonShift);
    }

    void RotateFrontWheels()
    {
        float carSpeed = Vector3.Dot(transform.forward, rb.velocity);
        //Debug.Log(carSpeed);
        float horizontal = currentInput.x;
        if (invertedSteering) horizontal *= -1;
        float desiredAngle = horizontal * maxWheelsAngle * wheelRotationMultCurve.Evaluate(carSpeed / _carTopSpeed);
        frontWheelsAngle = Quaternion.Lerp(wheels[0].localRotation, Quaternion.Euler(0,desiredAngle,0), Time.deltaTime * wheelRotationSpeed);

        wheels[0].localRotation = frontWheelsAngle;
        wheels[1].localRotation = frontWheelsAngle;

        //Debug.Log("Input: " + horizontal);
        //Debug.Log("Desired angle: " + desiredAngle);
        //Debug.Log(frontWheelsAngle);
    }

    Vector3 GetSuspensionForce(Transform wheel, float hitDistance)
    {
        Vector3 springDirection = wheel.up;
        Vector3 tireWorldVelocity = rb.GetPointVelocity(wheel.position);

        float offset = _suspensionRestDistance - hitDistance;
        if (offset < 0)
            offset *= 0.5f;
        float velocity = Vector3.Dot(springDirection, tireWorldVelocity);

        float force = (offset * _suspensionForce) - (velocity * _suspesionDamping);

        return springDirection * force;
    }
    Vector3 GetSteeringForce(Transform wheel)
    {
        Vector3 steeringDirection = wheel.right;
        Vector3 tireVelocity = rb.GetPointVelocity(wheel.position);

        float steeringVelocity = Vector3.Dot(steeringDirection, tireVelocity);
        float desiredVelocityChange = -steeringVelocity * _tireGrip;
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        return steeringDirection * _tireMass * desiredAcceleration;
    }
    Vector3 GetAccelerationForce(Transform wheel)
    {
        Vector3 accelDirection = wheel.forward;
        float vertical = currentInput.y;

        float carSpeed = Vector3.Dot(transform.forward, rb.velocity);
        float brakingBonus = 1;

        //Debug.Log("input: " + vertical);
        //Debug.Log("car speed: " + carSpeed);
        
        if (vertical == 0)
        {
            if (Mathf.Abs(carSpeed) <= 0.1f) return -accelDirection * carSpeed * 10;            
            if (Mathf.Abs(carSpeed) <= 0.5f) return -accelDirection * carSpeed * 0.4f;
            return -accelDirection * carSpeed * _idleDrag / 5;
        }
        if (vertical * carSpeed < 0)
        { // Changing direction 
            brakingBonus = _carBraking;
        }
        else
            if (carSpeed > _carTopSpeed || carSpeed < -_carTopReversingSpeed)
                return Vector3.zero;

        float normalizedSpeed;
        float avaiableTorque;
        if (vertical > 0)
        {
            normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _carTopSpeed);
            avaiableTorque = _powerCurve.Evaluate(normalizedSpeed) * vertical;
        }
        else
        {
            normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _carTopReversingSpeed);
            avaiableTorque = _reversingPowerCurve.Evaluate(normalizedSpeed) * vertical;
        }

        //Debug.Log(avaiableTorque);
        return accelDirection * avaiableTorque * _carAcceleration * brakingBonus;
    }

    private void Update()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(wheelsVisual[i].position, -wheelsVisual[i].up, out hit, _rayLength, _layerMask))
            {
                wheelsVisual[i].GetChild(0).position = hit.point + wheelsVisual[i].TransformDirection(wheelsVisual[i].up * wheelRadius);
            }
            else
                wheelsVisual[i].GetChild(0).localPosition = Vector3.zero;
        }
        wheelsVisual[0].localRotation = frontWheelsAngle;
        wheelsVisual[1].localRotation = frontWheelsAngle;

        float frontWheelsY = wheelsVisual[0].localEulerAngles.y;
        if (frontWheelsY > 180)
            frontWheelsY -= 360;

        if (steeringWheel != null)
            steeringWheel.transform.localRotation = Quaternion.Euler(steeringWheelRotation * frontWheelsY * 1.75f);
        //Quaternion.Lerp(steeringWheel.transform.localRotation, Quaternion.Euler(steeringWheelRotation * wheelsVisual[0].localEulerAngles.y * 1.75f), 1);
        //Quaternion.Euler(wheelsVisual[0].localEulerAngles * 1.75f); //Quaternion.Euler(steeringWheelRotation * wheelsVisual[0].localEulerAngles.y * 1.75f);

        Vector3 deltaPosition = transform.position - previousCarPosition;
        previousCarPosition = transform.position;

        float distance = deltaPosition.magnitude;

        if (Vector3.Dot(transform.forward, deltaPosition) < 0) distance = -distance;

        for (int i = 0; i < wheelsVisual.Length; i++)
        {
            float angle = 180 * distance / (wheelRadius * Mathf.PI);
            wheelsVisual[i].GetChild(0).Rotate(new Vector3(angle, 0), Space.Self);
        }
    }
}
