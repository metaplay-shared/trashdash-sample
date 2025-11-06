using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAlongZ : MonoBehaviour
{
    public float speed = 1000;

    void Update()
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
    }
}
