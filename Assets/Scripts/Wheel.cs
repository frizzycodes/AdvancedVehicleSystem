using UnityEngine;

public class Wheel : MonoBehaviour
{
    // Vehicle rigidbody reference
    private Rigidbody rb;

    [Header("Suspension")]

    public float restLength = 0.45f;
    public float suspensionTravel = 0.20f;
    public float springStrength = 60000f;
    public float damperStrength = 6500f;

    private float minLength;
    private float maxLength;

    private float currentSpringLength;
    private float previousSpringLength;

    private Vector3 suspensionForce;

    [Header("Wheel")]

    public float wheelRadius = 0.34f;

    public bool isDrivenWheel = true;
    public bool isSteeringWheel = true;
    public float maxSteerAngle = 35f;

    [Header("Tire Physics")]

    public float longitudinalStiffness = 1.0f;
    public float lateralStiffness = 2500f; // lower for stability

    private Vector3 contactVelocityWorld;
    private Vector3 contactVelocityLocal;

    private float longitudinalVelocity;
    private float lateralVelocity;

    private float throttleInput;
    private float steeringInput;
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

    public void SetSteering(float input)
    {
        steeringInput = input;
    }

    void Start()
    {
        minLength = restLength - suspensionTravel;
        maxLength = restLength + suspensionTravel;

        previousSpringLength = restLength;
    }

    void FixedUpdate()
    {
        float rayLength = maxLength + wheelRadius;

        Debug.DrawRay(transform.position, -transform.up * rayLength, Color.red);

        // Apply steering rotation
        if (isSteeringWheel)
        {
            float steerAngle = steeringInput * maxSteerAngle;
            transform.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        }

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength))
        {
            Debug.DrawRay(hit.point, hit.normal, Color.green);

            //-------------------------
            // Suspension
            //-------------------------

            currentSpringLength = hit.distance - wheelRadius;
            currentSpringLength = Mathf.Clamp(currentSpringLength, minLength, maxLength);

            float compression = restLength - currentSpringLength;

            float springForce = compression * springStrength;

            float springVelocity =
                (currentSpringLength - previousSpringLength) / Time.fixedDeltaTime;

            float damperForce = damperStrength * springVelocity;

            float normalForce = Mathf.Max(0f, springForce - damperForce);

            suspensionForce = normalForce * hit.normal;

            rb.AddForceAtPosition(suspensionForce, hit.point);

            previousSpringLength = currentSpringLength;

            //-------------------------
            // Contact patch velocity
            //-------------------------

            contactVelocityWorld = rb.GetPointVelocity(hit.point);

            contactVelocityLocal =
                transform.InverseTransformDirection(contactVelocityWorld);

            longitudinalVelocity = contactVelocityLocal.z;
            lateralVelocity = contactVelocityLocal.x;

            //-------------------------
            // Debug
            //-------------------------

            Debug.DrawRay(hit.point, contactVelocityWorld * 5f, Color.yellow);
            Debug.DrawRay(hit.point, transform.forward * longitudinalVelocity * 5f, Color.blue);
            Debug.DrawRay(hit.point, transform.right * lateralVelocity * 5f, Color.cyan);

            //-------------------------
            // Drive force
            //-------------------------

            if (isDrivenWheel)
            {
                Vector3 forwardDirection =
                    Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

                Vector3 driveForceVector =
                    forwardDirection * throttleInput * driveForce * longitudinalStiffness;

                rb.AddForceAtPosition(driveForceVector, hit.point);
            }

            //-------------------------
            // Lateral tire force (stable model)
            //-------------------------

            Vector3 lateralDirection =
                Vector3.ProjectOnPlane(transform.right, hit.normal).normalized;

            Vector3 lateralForce =
                -lateralDirection * lateralVelocity * lateralStiffness;

            rb.AddForceAtPosition(lateralForce, hit.point);
        }
        else
        {
            currentSpringLength = maxLength;
            previousSpringLength = currentSpringLength;
        }
    }
}