using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiPrewarperSettingsControl : MonoBehaviour
    {
        [SerializeField]
        List<GameObject> PrewarperSettings = new List<GameObject>();

        void Awake()
        {
            // 1. bind the delegates
            DanbiUISync.onPanelUpdated += OnPanelUpdate;

            // 2. Fill out the prewarper settings list.
            for (var i = 0; i < transform.childCount; ++i)
            {
                PrewarperSettings.Add(transform.GetChild(i).gameObject);
                // Turn on the Dome Reflector / Cube Panorama at first.
                // and turn off the others.
                PrewarperSettings[i].SetActive(0 == i);
            }
        }

        void OnDisable()
        {
            DanbiUISync.onPanelUpdated -= OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIPanoramaScreenShapePanelControl)
            {
                var shapePanel = control as DanbiUIPanoramaScreenShapePanelControl;
                SelectPrewarperSettings(shapePanel.selectedPrewarperSettingIndex);
            }
        }

        void SelectPrewarperSettings(int index)
        {
            for (var i = 0; i < PrewarperSettings.Count; ++i)
            {
                PrewarperSettings[i].SetActive(i == index);
            }
        }
    };
};
