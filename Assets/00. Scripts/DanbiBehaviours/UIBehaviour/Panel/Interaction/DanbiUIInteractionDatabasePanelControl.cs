using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionDatabasePanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public string gbdPath;

        TMP_Text GestureIDs;

        int newGesturesIDIndex = 0;

        protected override void SaveValues()
        {
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var gdbFileLocationText = default(TMP_Text);

            // 1. bind the select kinect gdb file
            var selectGDBFileButton = panel.GetChild(0).GetComponent<Button>();
            selectGDBFileButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectGDBFile(gdbFileLocationText)));

            // 2. bind the updating gdb file location
            gdbFileLocationText = panel.GetChild(1).GetComponent<TMP_Text>();
        }

        IEnumerator Coroutine_SelectGDBFile(TMP_Text locationText)
        {
            var filters = new string[] { ".gbd" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif

            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filters,
                                                     "Select Kinect .GDB file",
                                                     "Select");

            DanbiFileSys.GetResourcePathIntact(out gbdPath, out _);
            locationText.text = $"GBD Location : {gbdPath}";
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
