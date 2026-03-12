using UnityEngine;

public class Wheel : MonoBehaviour
{
    // Reference to the vehicle Rigidbody
    private Rigidbody rb;

    [Header("Suspension")]

    // Suspension rest length (equilibrium spring length)
    public float restLength = 0.45f;

    // Maximum suspension compression / extension distance
    public float suspensionTravel = 0.20f;

    // Spring stiffness (Hooke's law coefficient)
    public float springStrength = 60000f;

    // Damper coefficient to reduce oscillation
    public float damperStrength = 6500f;

    // Suspension limits
    private float minLength;
    private float maxLength;

    // Spring length tracking
    private float currentSpringLength;
    private float previousSpringLength;

    // Final suspension force
    private Vector3 suspensionForce;

    [Header("Wheel")]

    // Tire radius
    public float wheelRadius = 0.34f;

    // Determines whether this wheel receives engine power
    public bool isDrivenWheel = true;

    [Header("Tire Physics")]

    // Tire stiffness in longitudinal direction (acceleration/braking)
    public float longitudinalStiffness = 1.0f;

    // Tire stiffness in lateral direction (cornering)
    public float lateralStiffness = 12000f;

    // Velocity at contact patch
    private Vector3 contactVelocityWorld;
    private Vector3 contactVelocityLocal;

    // Velocity components in tire coordinate system
    private float longitudinalVelocity;
    private float lateralVelocity;

    // Input values from CarController
    private float throttleInput;
    private float driveForce;

    public void Initialize(Rigidbody carRb)
    {
        rb = carRb;
    }

    public void SetThrottle(float throttle, float force)
    {
        throttleInput = throttle;
        driveForce = force;
    }

    void Start()
    {
        // Calculate suspension limits
        minLength = restLength - suspensionTravel;
        maxLength = restLength + suspensionTravel;

        previousSpringLength = restLength;
    }

    void FixedUpdate()
    {
        float rayLength = maxLength + wheelRadius;

        Debug.DrawRay(transform.position, -transform.up * rayLength, Color.red);

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength))
        {
            Debug.DrawRay(hit.point, hit.normal, Color.green);

            //--------------------------------
            // Suspension physics
            //--------------------------------

            currentSpringLength = hit.distance - wheelRadius;
            currentSpringLength = Mathf.Clamp(currentSpringLength, minLength, maxLength);

            float compression = restLength - currentSpringLength;

            float springForce = compression * springStrength;

            float springVelocity =
                (currentSpringLength - previousSpringLength) / Time.fixedDeltaTime;

            float damperForce = damperStrength * springVelocity;

            suspensionForce = Mathf.Max(0f, springForce - damperForce) * hit.normal;

            rb.AddForceAtPosition(suspensionForce, hit.point);

            previousSpringLength = currentSpringLength;

            //--------------------------------
            // Contact patch velocity
            //--------------------------------

            contactVelocityWorld = rb.GetPointVelocity(hit.point);

            contactVelocityLocal =
                transform.InverseTransformDirection(contactVelocityWorld);

            longitudinalVelocity = contactVelocityLocal.z;
            lateralVelocity = contactVelocityLocal.x;

            //--------------------------------
            // Debug visualization
            //--------------------------------

            Debug.DrawRay(hit.point, contactVelocityWorld * 5f, Color.yellow);
            Debug.DrawRay(hit.point, transform.forward * longitudinalVelocity * 5f, Color.blue);
            Debug.DrawRay(hit.point, transform.right * lateralVelocity * 5f, Color.cyan);

            //--------------------------------
            // Longitudinal tire force
            //--------------------------------

            if (isDrivenWheel)
            {
                Vector3 forwardDirection =
                    Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

                Vector3 driveForceVector =
                    forwardDirection * throttleInput * driveForce * longitudinalStiffness;

                rb.AddForceAtPosition(driveForceVector, hit.point);
            }

            //--------------------------------
            // Lateral tire force
            //--------------------------------

            float lateralForceMagnitude = -lateralVelocity * lateralStiffness;

            Vector3 lateralForce =
                transform.right * lateralForceMagnitude;

            rb.AddForceAtPosition(lateralForce, hit.point);
        }
        else
        {
            currentSpringLength = maxLength;
            previousSpringLength = currentSpringLength;
        }
    }
}