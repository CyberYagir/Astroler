using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillRotator : MonoBehaviour
{
    public float maxSpeed, speed;
    public float speedAdd, speedSub;
    public Transform rotate;
    void Update()
    {
        if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }
        if (speed < 0) speed = 0;
        rotate.transform.Rotate(Vector3.forward * speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.Mouse0))
        {
            speed += speedAdd * Time.deltaTime;
        }
        else
        {
            speed -= speedSub * Time.deltaTime;
        }

    }
}
