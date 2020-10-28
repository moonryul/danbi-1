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
        public string gdbPath;

        TMP_Text GestureIDs;

        public delegate void OnGestureIDAdded(string newGestureID);
        public static OnGestureIDAdded Call_OnGestureIDAdded;

        int newGesturesIDIndex = 0;

        protected override void SaveValues()
        {
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Call_OnGestureIDAdded -= Caller_OnGestureIDAdded;
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            Call_OnGestureIDAdded += Caller_OnGestureIDAdded;

            var panel = Panel.transform;

            // 1. bind the select kinect gdb file
            var selectGDBFileButton = panel.GetChild(0).GetComponent<Button>();
            selectGDBFileButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectGDBFile()));
            // 2. bind the updating gdb file location
            var gdbFileLocationText = panel.GetChild(1).GetComponent<TMP_Text>();

            // 3. bind the updating gesture informations
            GestureIDs = panel.GetChild(2).GetComponent<TMP_Text>();
        }

        IEnumerator Coroutine_SelectGDBFile()
        {
            var filters = new string[] { ".gdb" };
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

            DanbiFileSys.GetResourcePathForResources(out gdbPath, out _);
            // TODO: Load GDB and Init Kinect Control!.
        }

        void Caller_OnGestureIDAdded(string newGestureID)
        {
            var tm = GestureIDs.transform.GetChild(newGesturesIDIndex++).GetComponent<TMP_Text>();
            tm.text = $"ID: <b><color=#FF0000>{newGestureID}";
        }
    };
};
