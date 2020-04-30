using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseCamera : MonoBehaviour
{
    public Transform Camera;
    public Transform Surface;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Camera.transform.position.x, Surface.transform.position.y * 2 - Camera.transform.position.y, Camera.transform.position.z);
        transform.eulerAngles = new Vector3(-Camera.transform.eulerAngles.x, Camera.transform.eulerAngles.y, Camera.transform.eulerAngles.z);
    }
}
