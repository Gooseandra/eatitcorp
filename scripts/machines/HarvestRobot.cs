using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class HarvestRobot : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float scanInterval = 2f;
    [SerializeField] private float harvestDistance = 1f;
    [SerializeField] private float harvestTime = 0.5f;

    private NavMeshAgent agent;
    private GardenBed currentTarget;
    private bool isHarvesting;
    private float lastScanTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = harvestDistance * 0.9f;
    }

    private void Update()
    {
        if (ShouldScanForTarget())
        {
            ScanForTarget();
        }

        if (ShouldStartHarvesting())
        {
            StartCoroutine(HarvestCoroutine());
        }
    }

    private bool ShouldScanForTarget()
    {
        return !isHarvesting && Time.time - lastScanTime > scanInterval;
    }

    private void ScanForTarget()
    {
        lastScanTime = Time.time;
        GardenBed bestTarget = FindBestTarget();

        if (bestTarget != null)
        {
            SetTarget(bestTarget);
        }
    }

    private GardenBed FindBestTarget()
    {
        GardenBed[] allBeds = FindObjectsOfType<GardenBed>();
        GardenBed bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var bed in allBeds)
        {
            if (bed.IsReadyToHarvest && !bed.IsReserved)
            {
                float distance = Vector3.Distance(transform.position, bed.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = bed;
                }
            }
        }

        return bestTarget;
    }

    private void SetTarget(GardenBed target)
    {
        if (currentTarget != null)
        {
            currentTarget.IsReserved = false;
        }

        currentTarget = target;
        currentTarget.IsReserved = true;
        agent.SetDestination(target.transform.position);
    }

    private bool ShouldStartHarvesting()
    {
        return currentTarget != null &&
               !isHarvesting &&
               !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance + 0.1f &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    private IEnumerator HarvestCoroutine()
    {
        isHarvesting = true;

        // Поворот к грядке
        yield return RotateTowardsTarget();

        // Ожидание времени сбора
        yield return new WaitForSeconds(harvestTime);

        // Сбор урожая
        if (currentTarget != null && currentTarget.IsReadyToHarvest)
        {
            currentTarget.Harvest();
        }

        // Сброс состояния
        if (currentTarget != null)
        {
            currentTarget.IsReserved = false;
            currentTarget = null;
        }

        isHarvesting = false;
    }

    private IEnumerator RotateTowardsTarget()
    {
        if (currentTarget == null) yield break;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        float rotationSpeed = 5f;
        float time = 0;

        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time);
            time += Time.deltaTime * rotationSpeed;
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (agent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}