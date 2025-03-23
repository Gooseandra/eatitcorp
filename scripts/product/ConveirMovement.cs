using UnityEngine;

public class ConveirMovement : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] bool stopped = false;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("grabbable"))
        {
            ConveirMovement other = collision.gameObject.GetComponent<ConveirMovement>();
            if (other != null && other.IsStopped()) // Останавливаем только если передний объект уже стоит
            {
                stopped = true;
                FreezeObject();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("grabbable"))
        {
            stopped = false;
            UnfreezeObject();
        }
    }

    private void FreezeObject()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
        | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
    }

    private void UnfreezeObject()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
    }

    public bool IsStopped()
    {
        return stopped;
    }

    public void SetStopped(bool stopped)
    {
        this.stopped = stopped;
    }
}
