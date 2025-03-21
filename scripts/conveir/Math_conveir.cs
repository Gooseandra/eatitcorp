using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Math_conveir : MonoBehaviour
{
    [SerializeField] Transform grab_obj_spawn_point;
    [SerializeField] GameObject grab_obj;
    [SerializeField] float grab_obj_spawn_delay;
    [SerializeField] float grab_obj_speed;
    [SerializeField] int max_grab_objs;

    [SerializeField] GameObject[] grab_objs;

    float time_after_spawn = 0;
    bool ready_to_spawn = true;

    private void FixedUpdate()
    {
        if (ready_to_spawn)
        {
            if (grab_objs.Length < max_grab_objs)
                Instantiate(grab_obj, grab_obj_spawn_point);
            grab_objs.Append<GameObject>(grab_obj);

            ready_to_spawn = false;
            time_after_spawn = 0;
        }
        
        time_after_spawn += Time.deltaTime;

        if (time_after_spawn > grab_obj_spawn_delay)
        {
            ready_to_spawn = true;
        }

        foreach (GameObject grab_obj in grab_objs)
        {
            grab_obj.transform.Translate(Vector3.forward * grab_obj_speed * Time.deltaTime);
        }
    }

}
