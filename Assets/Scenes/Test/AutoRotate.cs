using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {

    float timer;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
       
        transform.Rotate(new Vector3(Time.deltaTime, Time.deltaTime * 1.5f, Time.deltaTime * 2f) * 10f);
	}
}
