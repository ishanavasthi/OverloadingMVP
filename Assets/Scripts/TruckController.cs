using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Physics controller for the player's top-heavy truck.
///
/// Controls:
/// W - drive forward
/// S - brake, then reverse
/// A/D - steer left/right
///
/// The balancing challenge comes from an intentionally high Rigidbody center
/// of mass plus extra roll forces while steering at speed.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class TruckController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float motorForce = 9000f;
    [SerializeField] private float reverseForce = 4500f;
    [SerializeField] private float brakeForce = 7000f;
    [SerializeField] private float maxForwardSpeed = 18f;
    [SerializeField] private float maxReverseSpeed = 6f;
    [SerializeField] private float coastingDrag = 0.35f;
    [SerializeField] private float directMoveAssistSpeed = 8f;
    [SerializeField] private bool debugTransformMovementFallback = true;

    [Header("Steering")]
    [SerializeField] private float steeringTorque = 3500f;
    [SerializeField] private float steeringSpeedFactor = 0.6f;

    [Header("Top-Heavy Balance")]
    [Tooltip("Higher Y values make the truck more unstable and easier to topple.")]
    [SerializeField] private Vector3 centerOfMassOffset = new(0f, 1.1f, 0f);

    [Tooltip("Sideways force applied when steering. Higher values make sharp turns more dangerous.")]
    [SerializeField] private float lateralTipForce = 1800f;

    [Tooltip("Extra roll torque caused by steering. Higher values make the truck lean more.")]
    [SerializeField] private float steeringRollTorque = 1200f;

    [Header("Stability Assist")]
    [Tooltip("Small helper torque that gently reduces rolling. Keep low so the truck still feels risky.")]
    [SerializeField] private float rollStabilization = 450f;

    [Tooltip("Maximum roll angle where stabilization is applied. Beyond this, the truck is probably toppling.")]
    [SerializeField] private float stabilizationMaxAngle = 35f;

    private Rigidbody truckRigidbody;
    private float throttleInput;
    private float steerInput;
    private bool controlsEnabled = true;
    private float lastLoggedThrottle;
    private float lastLoggedSteer;
    private GUIStyle debugStyle;

    public Rigidbody Rigidbody => truckRigidbody;
    public float CurrentSpeed => truckRigidbody != null ? truckRigidbody.linearVelocity.magnitude : 0f;
    public float SignedRollAngle => GetSignedRollAngle();
    public float ThrottleInput => throttleInput;
    public float SteerInput => steerInput;
    public bool ControlsEnabled => controlsEnabled;

    private void Awake()
    {
        truckRigidbody = GetComponent<Rigidbody>();
        truckRigidbody.centerOfMass = centerOfMassOffset;
        truckRigidbody.isKinematic = false;
        truckRigidbody.WakeUp();
        Debug.Log($"TruckController awake on {name}. Rigidbody mass={truckRigidbody.mass}, isKinematic={truckRigidbody.isKinematic}, constraints={truckRigidbody.constraints}");
    }

    private void OnValidate()
    {
        maxForwardSpeed = Mathf.Max(0f, maxForwardSpeed);
        maxReverseSpeed = Mathf.Max(0f, maxReverseSpeed);
        stabilizationMaxAngle = Mathf.Clamp(stabilizationMaxAngle, 0f, 89f);

        if (truckRigidbody != null)
        {
            truckRigidbody.centerOfMass = centerOfMassOffset;
        }
    }

    private void Update()
    {
        ReadKeyboardInput();
        ApplyDebugTransformMovementFallback();
    }

    private void FixedUpdate()
    {
        ApplyMotorForce();
        ApplySteeringTorque();
        ApplyTopHeavyForces();
        ApplyRollStabilization();
    }

    private void ReadKeyboardInput()
    {
        throttleInput = 0f;
        steerInput = 0f;

        if (!controlsEnabled)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        if (keyboard.wKey.isPressed)
        {
            throttleInput += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            throttleInput -= 1f;
        }

        if (keyboard.aKey.isPressed)
        {
            steerInput -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            steerInput += 1f;
        }

        LogInputIfChanged();
    }

    private void ApplyMotorForce()
    {
        float forwardSpeed = Vector3.Dot(truckRigidbody.linearVelocity, transform.forward);
        float targetForwardSpeed = forwardSpeed;
        float speedChangeRate = coastingDrag;

        if (throttleInput > 0f && forwardSpeed < maxForwardSpeed)
        {
            targetForwardSpeed = maxForwardSpeed;
            speedChangeRate = ForceToAcceleration(motorForce);
        }
        else if (throttleInput < 0f)
        {
            if (forwardSpeed > 1f)
            {
                targetForwardSpeed = 0f;
                speedChangeRate = ForceToAcceleration(brakeForce);
            }
            else if (forwardSpeed > -maxReverseSpeed)
            {
                targetForwardSpeed = -maxReverseSpeed;
                speedChangeRate = ForceToAcceleration(reverseForce);
            }
        }
        else
        {
            targetForwardSpeed = Mathf.MoveTowards(forwardSpeed, 0f, coastingDrag * Time.fixedDeltaTime);
        }

        float speedDelta = targetForwardSpeed - forwardSpeed;
        float maxSpeedDelta = speedChangeRate * Time.fixedDeltaTime;
        float appliedDelta = Mathf.Clamp(speedDelta, -maxSpeedDelta, maxSpeedDelta);

        if (!Mathf.Approximately(appliedDelta, 0f))
        {
            truckRigidbody.AddForce(transform.forward * appliedDelta, ForceMode.VelocityChange);
        }

        if (!Mathf.Approximately(throttleInput, 0f))
        {
            Vector3 velocity = truckRigidbody.linearVelocity;
            Vector3 lateralVelocity = Vector3.ProjectOnPlane(velocity, transform.forward);
            Vector3 directForwardVelocity = transform.forward * Mathf.MoveTowards(forwardSpeed, targetForwardSpeed, maxSpeedDelta);
            truckRigidbody.linearVelocity = directForwardVelocity + lateralVelocity;

            Vector3 assistedPosition = truckRigidbody.position + transform.forward * throttleInput * directMoveAssistSpeed * Time.fixedDeltaTime;
            truckRigidbody.MovePosition(assistedPosition);
        }
    }

    private void ApplySteeringTorque()
    {
        float speedMultiplier = Mathf.Clamp01(truckRigidbody.linearVelocity.magnitude * steeringSpeedFactor);
        float yawDegreesPerSecond = steeringTorque * 0.02f;
        float yawDelta = steerInput * yawDegreesPerSecond * speedMultiplier * Time.fixedDeltaTime;

        if (!Mathf.Approximately(yawDelta, 0f))
        {
            Quaternion yawRotation = Quaternion.AngleAxis(yawDelta, Vector3.up);
            truckRigidbody.MoveRotation(yawRotation * truckRigidbody.rotation);
        }

        Vector3 yawTorque = Vector3.up * steerInput * steeringTorque * 0.25f * speedMultiplier;
        truckRigidbody.AddTorque(yawTorque, ForceMode.Force);
    }

    private void ApplyTopHeavyForces()
    {
        float speed = truckRigidbody.linearVelocity.magnitude;

        if (speed < 0.5f || Mathf.Approximately(steerInput, 0f))
        {
            return;
        }

        Vector3 sideDirection = transform.right * steerInput;
        truckRigidbody.AddForce(sideDirection * lateralTipForce * speed, ForceMode.Force);

        // Turning right rolls left, turning left rolls right.
        Vector3 rollTorque = -transform.forward * steerInput * steeringRollTorque * speed;
        truckRigidbody.AddTorque(rollTorque, ForceMode.Force);
    }

    private void ApplyRollStabilization()
    {
        float rollAngle = GetSignedRollAngle();

        if (Mathf.Abs(rollAngle) > stabilizationMaxAngle)
        {
            return;
        }

        Vector3 correctionTorque = -transform.forward * rollAngle * rollStabilization;
        truckRigidbody.AddTorque(correctionTorque, ForceMode.Force);
    }

    private float GetSignedRollAngle()
    {
        Vector3 localWorldUp = transform.InverseTransformDirection(Vector3.up);
        return Mathf.Atan2(localWorldUp.x, localWorldUp.y) * Mathf.Rad2Deg;
    }

    private float ForceToAcceleration(float force)
    {
        return force / Mathf.Max(1f, truckRigidbody.mass);
    }

    private void LogInputIfChanged()
    {
        if (Mathf.Approximately(throttleInput, lastLoggedThrottle) && Mathf.Approximately(steerInput, lastLoggedSteer))
        {
            return;
        }

        lastLoggedThrottle = throttleInput;
        lastLoggedSteer = steerInput;
        Debug.Log($"Truck input changed: throttle={throttleInput}, steer={steerInput}, controlsEnabled={controlsEnabled}, keyboardPresent={Keyboard.current != null}");
    }

    private void ApplyDebugTransformMovementFallback()
    {
        if (!debugTransformMovementFallback || !controlsEnabled)
        {
            return;
        }

        if (!Mathf.Approximately(steerInput, 0f))
        {
            transform.Rotate(Vector3.up, steerInput * 70f * Time.deltaTime, Space.World);
        }

        if (!Mathf.Approximately(throttleInput, 0f))
        {
            transform.position += transform.forward * throttleInput * directMoveAssistSpeed * Time.deltaTime;
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
        throttleInput = 0f;
        steerInput = 0f;
    }

    public void ResetTruck(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);

        if (truckRigidbody == null)
        {
            truckRigidbody = GetComponent<Rigidbody>();
        }

        truckRigidbody.linearVelocity = Vector3.zero;
        truckRigidbody.angularVelocity = Vector3.zero;
        truckRigidbody.Sleep();
        truckRigidbody.WakeUp();
        truckRigidbody.centerOfMass = centerOfMassOffset;
    }

    private void OnGUI()
    {
        if (debugStyle == null)
        {
            debugStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 18
            };

            debugStyle.normal.textColor = Color.white;
        }

        string keyboardState = Keyboard.current == null ? "missing" : "present";
        string debugMessage =
            "TRUCK DEBUG\n" +
            $"Keyboard: {keyboardState}\n" +
            $"Controls enabled: {controlsEnabled}\n" +
            $"Throttle W/S: {throttleInput:0}\n" +
            $"Steer A/D: {steerInput:0}\n" +
            $"Velocity: {(truckRigidbody != null ? truckRigidbody.linearVelocity.magnitude : 0f):0.00}\n" +
            $"Position: {transform.position.x:0.0}, {transform.position.y:0.0}, {transform.position.z:0.0}\n" +
            $"Kinematic: {(truckRigidbody != null && truckRigidbody.isKinematic)}";

        GUI.Box(new Rect(20f, 220f, 360f, 210f), debugMessage, debugStyle);
    }
}
