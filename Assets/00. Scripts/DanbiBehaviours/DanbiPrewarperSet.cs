using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiPrewarperSet : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> m_prewarperSet = new List<GameObject>();        

        void Awake()
        {            
            // Fill out the prewarper settings list.
            for (var i = 0; i < transform.childCount; ++i)
            {
                m_prewarperSet.Add(transform.GetChild(i).gameObject);
                // Turn on the Dome Reflector / Cube Panorama at first.
                // and turn off the others.
                m_prewarperSet[i].SetActive(0 == i);
            }

            DanbiUIPanoramaScreenShapePanel.onPanoramaScreenShapeChange += this.SelectPrewarperSettings;
        }        

        void SelectPrewarperSettings(int index)
        {
            for (var i = 0; i < m_prewarperSet.Count; ++i)
            {
                m_prewarperSet[i].SetActive(i == index);
            }
        }
    };
};
