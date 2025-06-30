using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] Transform s;
    // Update is called once per frame
    void Update()
    {
        this.transform.RotateAround(new Vector3(s.position.x, s.position.y, s.position.z), new Vector3(1,0,0), 10);
    }
}
