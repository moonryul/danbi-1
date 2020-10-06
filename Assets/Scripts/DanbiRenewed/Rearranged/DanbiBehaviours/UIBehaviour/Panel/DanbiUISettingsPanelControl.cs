using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUISettingsPanelControl : MonoBehaviour
    {
        public static bool useAutoSave { get; set; } = true;

        void OnDisable()
        {
            PlayerPrefs.SetInt("SettingsPanel-useAutoSave", useAutoSave ? 1 : 0);
        }

        void Start()
        {
            var autoSaveToggle = transform.GetChild(0).GetComponent<Toggle>();
            autoSaveToggle.onValueChanged.AddListener((bool isOn) => useAutoSave = isOn);
            LoadPreviousValue(autoSaveToggle);
        }

        void LoadPreviousValue(params Selectable[] uiElements)
        {
            int prevUseAutoSave = PlayerPrefs.GetInt("SettingsPanel-useAutoSave", default);
            useAutoSave = prevUseAutoSave == 1;
            (uiElements[0] as Toggle).isOn = useAutoSave;
        }
    };
};
