using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ConveyorSegment : MonoBehaviour
{
    public Transform waypoint; // Целевая точка для текущего сегмента
    public ConveyorSegment nextSegment; // Следующий сегмент конвейера
    [SerializeField] Transform[] nextPoints;

    [SerializeField] private float speed = 0.5f; // Скорость движения объектов

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("grabbable"))
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            ConveirMovement cm = collision.gameObject.GetComponent<ConveirMovement>();
            if (rb != null)
            {
                MoveObject(rb, cm);
            }
        }
    }

    public void MoveObject(Rigidbody rb, ConveirMovement cm)
    {
        if (nextSegment == null)
        {
            cm.SetStopped(true);
            FreezeObjectPosition(rb);
            return;
        }
        else
        {
            cm.SetStopped(false);
            UnfreezeObjectPosition(rb);
        }

        if (!cm.IsStopped())
        {
            Vector3 targetPosition = nextSegment.waypoint.position;
            Vector3 direction = (targetPosition - rb.position).normalized;

            if (Physics.Raycast(rb.position, direction, out RaycastHit hit, 0.6f))
            {
                if (hit.collider.CompareTag("grabbable"))
                {
                    cm.SetStopped(true);
                    FreezeObjectPosition(rb);
                    return;
                }
            }

            rb.linearVelocity = direction * speed;

            if (Vector3.Distance(rb.position, waypoint.position) < 0.05f)
            {
                rb.position = waypoint.position;
                rb.linearVelocity = Vector3.zero;
                nextSegment.MoveObject(rb, cm);
            }
        }
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
        print("New next segment recived");
        print(newNextSegment.gameObject.name);
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

            // Рисуем стрелку
            DrawArrow(transform.position, nextSegment.waypoint.position);
        }
    }

    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float arrowSize = 0.5f; // Размер стрелки

        // Основная линия стрелки
        Gizmos.DrawLine(from, to);

        // Левые и правые "крылья" стрелки
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
