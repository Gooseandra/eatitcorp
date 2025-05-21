using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ConveyorSegment : MonoBehaviour
{
    public Transform waypoint;
    public ConveyorSegment nextSegment;
    [SerializeField] Transform[] nextPoints;
    [SerializeField] private SnapPoint snapPoint;
    [SerializeField] private float speed = 0.5f;

    private Dictionary<Rigidbody, Vector3> lastPositions = new();

    private void Start()
    {
        snapPoint = this.GetComponent<SnapPoint>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("grabbable"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            ConveirMovement cm = collision.gameObject.GetComponent<ConveirMovement>();
            if (rb != null && cm != null)
            {
                MoveObject(rb, cm);
            }
        }
    }

    public void MoveObject(Rigidbody rb, ConveirMovement cm)
    {
        Vector3 targetPosition = waypoint.position;
        Vector3 direction = (targetPosition - rb.position).normalized;

        float checkDistance = 0.5f;
        Ray ray = new Ray(rb.position, direction);
        bool isBlocked = Physics.Raycast(ray, out RaycastHit hit, checkDistance);

        if (isBlocked && hit.collider.CompareTag("grabbable") && hit.rigidbody != rb)
        {
            cm.SetStopped(true);
            FreezeObjectPosition(rb);
            rb.linearVelocity = Vector3.zero;
            return;
        }
        else
        {
            cm.SetStopped(false);
            UnfreezeObjectPosition(rb);
        }

        float distance = Vector3.Distance(rb.position, targetPosition);
        if (distance < 0.05f)
        {
            rb.position = targetPosition;
            rb.linearVelocity = Vector3.zero;

            if (nextSegment != null)
            {
                nextSegment.MoveObject(rb, cm);
            }
            else
            {
                cm.SetStopped(true);
                FreezeObjectPosition(rb);
            }
        }
        else
        {
            rb.linearVelocity = direction * speed;
        }

        // 👇 Проталкивание по координатам
        if (!IsFrozen(rb) && !cm.IsStopped())
        {
            if (lastPositions.TryGetValue(rb, out Vector3 lastPos))
            {
                float movement = Vector3.Distance(rb.position, lastPos);
                if (movement < 0.001f)
                {
                    rb.position += direction * 0.1f; // Маленький шаг вручную

                }
            }

            lastPositions[rb] = rb.position;
        }
    }

    private bool IsFrozen(Rigidbody body)
    {
        return (body.constraints & RigidbodyConstraints.FreezePositionX) != 0 &&
               (body.constraints & RigidbodyConstraints.FreezePositionY) != 0 &&
               (body.constraints & RigidbodyConstraints.FreezePositionZ) != 0;
    }

    private void FreezeObjectPosition(Rigidbody rb)
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
    }

    private void UnfreezeObjectPosition(Rigidbody rb)
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void SetNextSegment(ConveyorSegment newNextSegment)
    {
        nextSegment = newNextSegment;

        if (nextSegment != null)
        {
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                UnfreezeObjectPosition(rb);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (nextSegment != null && waypoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextSegment.waypoint.position);
            DrawArrow(transform.position, nextSegment.waypoint.position);
        }
    }

    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float arrowSize = 0.5f;

        Gizmos.DrawLine(from, to);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

        Gizmos.DrawLine(to, to - right * arrowSize);
        Gizmos.DrawLine(to, to - left * arrowSize);
    }

    public Transform[] GetNextPoints()
    {
        return nextPoints;
    }
}
