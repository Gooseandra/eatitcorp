using System.Collections;
using UnityEngine;

public class HarvestorPortOut : MonoBehaviour
{
    public Transform spawnPoint;
    public float unloadDelay = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.layer == 7)
        {
            Debug.Log("7");
            Debug.Log(other.transform.parent.GetComponent<HarvestRobot>());
            other.transform.parent.GetComponent<HarvestRobot>().carriedPlant = null;
            other.transform.parent = null;

            other.transform.position = spawnPoint.position;
            other.GetComponent<CapsuleCollider>().isTrigger = false;
            other.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(spawnPoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }
    }
}