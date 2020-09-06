using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIControl : MonoBehaviour
    {
        [Readonly, SerializeField, Header("Used for the result name.")]
        InputField InputField_SaveFile;

        [Readonly, SerializeField, Header("Used for creating the result.")]
        Button Button_CreateResult;

        public delegate void OnPanelDataUpdated(DanbiUIPanelControl panelControl);
        public static OnPanelDataUpdated Call_OnPanelDataUpdated;

        void Start()
        {
            Call_OnPanelDataUpdated += Caller_OnPanelDataUpdated;
            // 1. Acquire the resources of the UI control buttons.      
        }

        void OnDisable()
        {
            Call_OnPanelDataUpdated -= Caller_OnPanelDataUpdated;
        }

        void Caller_OnPanelDataUpdated(DanbiUIPanelControl panelControl)
        {
            // TODO:
            // 1. Update Preview
            // 2. Update Shader Parameter
        }

        /// <summary>
        /// Called when the Save File button clicked!
        /// </summary>
        /// <param name="call"></param>
        void OnSaveFile(string call)
        {
            // TODO: 윈도우즈 익스플로러를 연결하여 사용해야함.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                DanbiControl.Call_OnSaveImage?.Invoke();
            }
        }

        void OnCreateResult()
        {

        }
    };

}; // namespace Danbi.
