using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grab_obj : MonoBehaviour
{
    private Transform gabbed_obj_transform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "grabable" && gabbed_obj_transform == null)
        {
            gabbed_obj_transform = other.gameObject.GetComponent<Transform>();
        }
        else if (other.gameObject.tag == "grab_obj")
        {
            Destroy(this.gameObject);
        }
    }
}
