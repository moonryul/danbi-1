using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanbiMultiDisplay : MonoBehaviour
{
    [SerializeField, Readonly]
    Camera[] m_usedCams = new Camera[3];

    void Start()
    {
        m_usedCams[0] = Camera.main;
        m_usedCams[1] = GameObject.Find("Render Camera").GetComponent<Camera>();
        m_usedCams[2] = GameObject.Find("Interaction Camera").GetComponent<Camera>();        

        for (int i = 0; i < Display.displays.Length; ++i)
        {
            m_usedCams[i].targetDisplay = i;
            Display.displays[i].Activate();
            Display.displays[i].SetRenderingResolution(Display.displays[i].systemWidth, Display.displays[i].systemHeight);
        }
    }
};
