using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test_control : MonoBehaviour
{
    void LateUpdate()
    {
            transform.Rotate(new Vector3(0.0f, 0.01f, 0.0f), Space.Self);
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        // }
    }
}
