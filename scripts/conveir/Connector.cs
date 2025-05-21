using UnityEngine;

public class Connector : MonoBehaviour
{
    public Mixer Mixer;
    void Start()
    {
        Mixer = GetComponentInParent<Mixer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Entered");
        if (other.GetComponent<ItemPickup>() != null)
        {
            Debug.Log("Item Pickup Detected");
            Mixer.HandleIngredient(other);
        }
    }
}
