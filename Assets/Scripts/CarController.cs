using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    // Array containing references to all wheels attached to the vehicle.
    // Each wheel handles its own suspension and tire physics.
    public Wheel[] wheels;

    // Rigidbody representing the vehicle body.
    // All forces from wheels are applied to this object.
    private Rigidbody rb;

    // PlayerInput component from Unity's Input System.
    // Used to access input actions defined in the Input Actions asset.
    private PlayerInput input;

    // Cached reference to the "Drive" action.
    // This avoids repeated dictionary lookups every frame.
    private InputAction driveAction;

    [Header("Engine")]

    // Maximum drive force sent to driven wheels.
    // This represents engine output in a simplified form.
    public float driveForce = 8000f;

    // Current throttle input from the player.
    // Range typically:
    // -1 = reverse / braking
    //  0 = no throttle
    //  1 = full acceleration
    private float throttleInput;

    void Awake()
    {
        // Retrieve the PlayerInput component attached to this object.
        input = GetComponent<PlayerInput>();

        // Enable all input actions.
        input.actions.Enable();

        // Cache the "Drive" input action for performance.
        driveAction = input.actions["Drive"];
    }

    void Start()
    {
        // Retrieve the Rigidbody that represents the vehicle body.
        rb = GetComponent<Rigidbody>();

        // Lower the center of mass slightly to improve stability
        // and reduce the likelihood of the car rolling over.
        rb.centerOfMass = new Vector3(0f, -0.35f, 0f);

        // Initialize all wheels with a reference to the vehicle Rigidbody.
        // This allows them to apply suspension and tire forces.
        foreach (Wheel wheel in wheels)
        {
            wheel.Initialize(rb);
        }
    }

    void Update()
    {
        // Read throttle input from the player.
        // This value will be forwarded to the wheels in FixedUpdate.
        throttleInput = driveAction.ReadValue<float>();
    }

    void FixedUpdate()
    {
        // Send throttle information to each wheel during the physics step.
        // Each driven wheel will apply engine force at its contact point.
        foreach (Wheel wheel in wheels)
        {
            wheel.SetThrottle(throttleInput, driveForce);
        }
    }
}