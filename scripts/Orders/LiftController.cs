using System.Collections;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    public Transform liftTransform;
    public float liftHeight = 20f;
    public float liftSpeed = 2f;
    private Vector3 initialPosition;
    private bool isMoving = false;

    private void Start()
    {
        initialPosition = liftTransform.position;
    }

    public void SendLift()
    {
        if (!isMoving)
            StartCoroutine(MoveLiftRoutine());
    }

    private IEnumerator MoveLiftRoutine()
    {
        isMoving = true;
        Vector3 targetPos = initialPosition + Vector3.up * liftHeight;

        // Подъём
        while (Vector3.Distance(liftTransform.position, targetPos) > 0.1f)
        {
            liftTransform.position = Vector3.MoveTowards(liftTransform.position, targetPos, Time.deltaTime * liftSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(2f); // "доставка"

        // Спуск
        while (Vector3.Distance(liftTransform.position, initialPosition) > 0.1f)
        {
            liftTransform.position = Vector3.MoveTowards(liftTransform.position, initialPosition, Time.deltaTime * liftSpeed);
            yield return null;
        }

        isMoving = false;
        Debug.Log("Лифт вернулся.");
    }
}
