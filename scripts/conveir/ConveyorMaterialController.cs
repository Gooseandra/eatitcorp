using UnityEngine;

public class ConveyorMaterialController : MonoBehaviour
{
    private Material conveyorMaterial;
    private float currentSpeed = 0f;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            conveyorMaterial = renderer.material;
        }
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        if (conveyorMaterial != null)
        {
            conveyorMaterial.SetFloat("_Speed", currentSpeed);
        }
    }
}
