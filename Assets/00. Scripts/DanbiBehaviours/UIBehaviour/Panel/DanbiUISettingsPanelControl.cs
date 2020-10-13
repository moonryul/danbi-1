using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUISettingsPanelControl : MonoBehaviour
    {
        public static bool useAutoSave { get; set; } = true;

        // public static EDanbiDisplayLanguage displayLanguage = EDanbiDisplayLanguage.English;

        void OnDisable()
        {
            PlayerPrefs.SetInt("SettingsPanel-useAutoSave", useAutoSave ? 1 : 0);
            // PlayerPrefs.SetInt("SettingsPanel-displayLanguage", (int)displayLanguage);
        }

        void Start()
        {
            // bind the auto save toggle.
            var autoSaveToggle = transform.GetChild(0).GetComponent<Toggle>();
            autoSaveToggle.onValueChanged.AddListener((bool isOn) => useAutoSave = isOn);
            autoSaveToggle.isOn = true;

            // TODO: priority -> low
            // bind the language dropdown.
            var languageDropdown = transform.GetChild(1).GetComponent<Dropdown>();
            languageDropdown.AddOptions(new List<string> { "English", "한국어" });
            // languageDropdown.onValueChanged.AddListener(())


            LoadPreviousValue(autoSaveToggle);
        }

        void LoadPreviousValue(params Selectable[] uiElements)
        {
            int prevUseAutoSave = PlayerPrefs.GetInt("SettingsPanel-useAutoSave", default);
            useAutoSave = prevUseAutoSave == 1;
            (uiElements[0] as Toggle).isOn = useAutoSave;

            // int prevDisplayLanguage = PlayerPrefs.GetInt("SettingsPanel-displayLanguage", default);
            // displayLanguage = (EDanbiDisplayLanguage)prevDisplayLanguage;            
        }
    };
};
