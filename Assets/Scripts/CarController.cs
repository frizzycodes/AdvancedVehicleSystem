using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // Array containing references to all wheels attached to the car.
    // Each Wheel script handles suspension physics and tire forces.
    public Wheel[] wheels;

    // Rigidbody representing the physical body of the car.
    // All forces from the wheels will ultimately act on this body.
    private Rigidbody rb;

    // PlayerInput component from Unity's new Input System.
    // Used to read player actions (throttle, steering, etc.).
    private PlayerInput input;

    // Cached reference to the "Drive" input action.
    // This avoids repeated string lookups every frame.
    private InputAction driveAction;

    [Header("Engine")]

    // Maximum engine force applied to driven wheels.
    // This is a simplified drive force used for acceleration.
    public float motorForce = 8000f;

    // Current throttle value coming from player input.
    // Expected range:
    // -1 = reverse / braking
    //  0 = no throttle
    //  1 = full acceleration
    private float throttleInput;

    void Awake()
    {
        // Retrieve the PlayerInput component attached to the car.
        input = GetComponent<PlayerInput>();

        // Enable all input actions so they start receiving input.
        input.actions.Enable();

        // Cache the Drive action for efficient access later.
        driveAction = input.actions["Drive"];
    }

    void Start()
    {
        // Retrieve the Rigidbody attached to the car body.
        rb = GetComponent<Rigidbody>();

        // Lower the center of mass slightly.
        // This improves stability and reduces rollover tendency.
        rb.centerOfMass = new Vector3(0, -0.35f, 0);

        // Initialize each wheel with a reference to the car's Rigidbody.
        // This allows wheels to apply suspension and tire forces to the car body.
        foreach (Wheel wheel in wheels)
        {
            wheel.Initialize(rb);
        }
    }

    void Update()
    {
        // Read throttle input from the Drive action.
        // Values come from the input bindings (e.g. W/S keys).
        throttleInput = driveAction.ReadValue<float>();
    }

    void FixedUpdate()
    {
        // Send the current throttle value to each wheel during the physics step.
        // Wheels will use this to apply drive force at their contact point.
        foreach (Wheel wheel in wheels)
        {
            wheel.SetThrottle(throttleInput, motorForce);
        }
    }
}