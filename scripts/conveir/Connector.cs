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
        if (other.GetComponent<ItemPickup>() != null)
        {
            Mixer.HandleIngredient(other);
        }
    }
}
