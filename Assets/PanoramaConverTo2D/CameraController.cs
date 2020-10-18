using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotateSpeed = 1.0f;

    bool changeScreen = false;

    public GameObject sphereScreen;
    public GameObject cylinderScreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * rotateSpeed, 0));

        if(Input.GetKeyDown(KeyCode.Space))
        {
            changeScreen = true;
        }
        if(changeScreen)
        {
           
            sphereScreen.SetActive(sphereScreen.activeSelf ^ true);
            cylinderScreen.SetActive(cylinderScreen.activeSelf ^ true);
            changeScreen = false;
        }
    }
}
