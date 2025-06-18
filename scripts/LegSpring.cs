using UnityEngine;

public class LegSpring : MonoBehaviour
{
    private Vector3 startPos;
    private Rigidbody rb;
    public float springForce = 200f;
    public float damper = 5f;
    private Vector3 velocity;

    void Start()
    {
        startPos = transform.localPosition;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 displacement = transform.localPosition - startPos;
        Vector3 spring = -springForce * displacement;
        Vector3 damping = -damper * rb.linearVelocity;

        rb.AddForce(spring + damping);
    }
}
