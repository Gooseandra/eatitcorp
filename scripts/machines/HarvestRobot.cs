using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class HarvestRobot : MonoBehaviour
{
    [Header("Настройки сбора")]
    [SerializeField] private float scanInterval = 2f;
    [SerializeField] private float harvestDistance = 1f;
    [SerializeField] private float harvestTime = 0.5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Настройки порта")]
    [SerializeField] private float portSearchRadius = 15f;
    [SerializeField] private float portApproachDistance = 2f;
    [SerializeField] private Vector3 carryOffset = new Vector3(0, 0.5f, 0);

    private NavMeshAgent agent;
    private GardenBed currentTarget;
    private HarvestorPortOut targetPort;
    public GameObject carriedPlant;
    private bool isHarvesting;
    private float lastScanTime;
    private List<HarvestorPortOut> allPorts = new List<HarvestorPortOut>();
    private int hiddenLayer;
    private int defaultLayer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = harvestDistance * 0.7f;
        hiddenLayer = LayerMask.NameToLayer("Hidden");
        defaultLayer = LayerMask.NameToLayer("Default");

        if (hiddenLayer == -1)
        {
            Debug.LogWarning("Слой 'Hidden' не найден. Создайте его в настройках слоев!");
        }
    }

    private void Start()
    {
        allPorts = FindObjectsOfType<HarvestorPortOut>().ToList();
        Debug.Log($"Найдено портов при старте: {allPorts.Count}");
    }

    private void Update()
    {
        DebugState();

        if (!IsCarryingPlant())
        {
            HandleHarvesting();
        }
        else
        {
            HandlePlantDelivery();
            UpdateCarriedPlantPosition();
        }
    }

    private void UpdateCarriedPlantPosition()
    {
        if (carriedPlant != null)
        {
            carriedPlant.transform.position = transform.position;
            carriedPlant.transform.rotation = transform.rotation;
        }
    }

    private void DebugState()
    {
        string state = IsCarryingPlant() ? "Доставка" : "Сбор";
        string targetInfo = currentTarget != null ? currentTarget.name : "нет";
        string portInfo = targetPort != null ? targetPort.name : "нет";

        Debug.Log($"Состояние: {state} | Грядка: {targetInfo} | Порт: {portInfo} | Растение: {(carriedPlant != null ? "есть" : "нет")}");
    }

    #region Логика сбора
    private void HandleHarvesting()
    {
        if (ShouldScanForTarget())
        {
            Debug.Log("-- Поиск грядок для сбора --");
            ScanForTarget();
        }

        if (ShouldStartHarvesting())
        {
            Debug.Log($"-- Начинаю сбор с {currentTarget.name} --");
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
            Debug.Log($"Найдена подходящая грядка: {bestTarget.name}");
            SetTarget(bestTarget);
        }
        else
        {
            Debug.LogWarning("Готовых к сбору грядок не найдено!");
        }
    }

    private GardenBed FindBestTarget()
    {
        GardenBed[] allBeds = FindObjectsOfType<GardenBed>();
        Debug.Log($"Всего грядок на сцене: {allBeds.Length}");

        GardenBed bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var bed in allBeds)
        {
            if (!bed.IsReadyToHarvest) continue;
            if (bed.IsReserved) continue;

            float distance = Vector3.Distance(transform.position, bed.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = bed;
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
        if (currentTarget != null)
        {
            currentTarget.IsReserved = true;
            agent.SetDestination(currentTarget.transform.position);
        }
    }

    private bool ShouldStartHarvesting()
    {
        if (currentTarget == null) return false;

        return !isHarvesting &&
               !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance + 0.1f &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    private IEnumerator HarvestCoroutine()
    {
        isHarvesting = true;

        yield return RotateTowardsTarget();
        yield return new WaitForSeconds(harvestTime);

        if (currentTarget != null && currentTarget.IsReadyToHarvest)
        {
            // Забираем растение БЕЗ уничтожения
            carriedPlant = currentTarget.TakePlant();

            if (carriedPlant != null)
            {
                // Переносим растение к роботу
                carriedPlant.transform.SetParent(transform);
                SetPlantVisibility(false);

                // Отключаем физику на всякий случай
                if (carriedPlant.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                FindNearestPort();
                if (targetPort != null)
                {
                    agent.SetDestination(targetPort.transform.position);
                }
            }
        }

        isHarvesting = false;
    }

    private void SetPlantVisibility(bool visible)
    {
        if (carriedPlant == null) return;

        carriedPlant.layer = visible ? defaultLayer : hiddenLayer;
        foreach (Transform child in carriedPlant.transform)
        {
            child.gameObject.layer = visible ? defaultLayer : hiddenLayer;
        }

        var renderers = carriedPlant.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    private IEnumerator RotateTowardsTarget()
    {
        if (currentTarget == null) yield break;

        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        float time = 0;

        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time);
            time += Time.deltaTime * rotationSpeed;
            yield return null;
        }
    }
    #endregion

    #region Логика доставки
    private void HandlePlantDelivery()
    {
        if (targetPort == null || !targetPort.gameObject.activeInHierarchy)
        {
            Debug.Log("Порт не назначен или неактивен, ищу новый...");
            FindNearestPort();

            if (targetPort != null)
            {
                agent.SetDestination(targetPort.transform.position);
            }
        }

        if (targetPort != null)
        {
            // Проверяем, нужно ли обновить путь
            if (agent.destination != targetPort.transform.position)
            {
                agent.SetDestination(targetPort.transform.position);
            }

            float distance = Vector3.Distance(transform.position, targetPort.transform.position);
            Debug.Log($"Расстояние до порта: {distance:F1} (нужно: {portApproachDistance})");

            if (distance <= portApproachDistance)
            {
                DeliverToPort();
            }
        }
        else
        {
            Debug.LogError("Не могу найти порт для разгрузки!");
            StartCoroutine(WaitAndSearchAgain());
        }
    }

    private IEnumerator WaitAndSearchAgain()
    {
        yield return new WaitForSeconds(2f);
        FindNearestPort();
    }

    private void FindNearestPort()
    {
        if (allPorts.Count == 0)
        {
            Debug.Log("Портов в списке нет, повторный поиск...");
            allPorts = FindObjectsOfType<HarvestorPortOut>().ToList();
        }

        var activePorts = allPorts
            .Where(p => p != null &&
                   p.gameObject.activeInHierarchy &&
                   Vector3.Distance(transform.position, p.transform.position) <= portSearchRadius)
            .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
            .ToList();

        Debug.Log($"Найдено активных портов: {activePorts.Count}");

        if (activePorts.Count > 0)
        {
            targetPort = activePorts.First();
            Debug.Log($"Выбран порт: {targetPort.name}");
        }
        else
        {
            Debug.LogError("Нет доступных портов в радиусе!");
            targetPort = null;
        }
    }

    private void DeliverToPort()
    {
        if (carriedPlant == null || targetPort == null)
        {
            Debug.LogError("Ошибка доставки: нет растения или порта!");
            return;
        }

        SetPlantVisibility(true);

        Debug.Log($"Доставляю растение {carriedPlant.name} на порт {targetPort.name}");

        carriedPlant = null;
        targetPort = null;
    }
    #endregion

    #region Публичные методы
    public bool IsCarryingPlant() => carriedPlant != null;
    public GameObject GetCarriedPlant() => carriedPlant;
    public void ClearCarriedPlant() => carriedPlant = null;
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, portSearchRadius);

        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, agent.destination);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.TransformPoint(carryOffset), 0.1f);
    }
}