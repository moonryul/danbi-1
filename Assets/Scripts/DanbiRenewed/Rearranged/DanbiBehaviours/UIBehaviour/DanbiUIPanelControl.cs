using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    /// <summary>
    /// 
    /// </summary>
    public class DanbiUIPanelControl : MonoBehaviour
    {
        [SerializeField, Readonly]
        protected bool isPanelOpened = false;
        protected GameObject Panel;        

        public delegate void OnDisplayLanguageChanged(EDanbiDisplayLanguage lang);
        public static OnDisplayLanguageChanged Call_OnDisplayLanguageChanged;

        void Start()
        {
            Panel = transform.GetChild(1).gameObject;
            if (!Panel.name.Contains("Panel"))
            {
                Panel = null;
            }
            else
            {
                var parentSize = transform.parent.GetComponent<RectTransform>().rect;
                Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 0);
            }
            AddListenerForPanelFields();
        }

        void OnDisable()
        {
            if (DanbiUISettingsPanelControl.useAutoSave)
            {
                SaveValues();
            }
        }

        protected virtual void SaveValues() { /**/ }

        protected virtual void LoadPreviousValues(params Selectable[] uiElements) { /**/ }

        protected virtual void AddListenerForPanelFields() { if (Panel.Null()) { return; } }

        public virtual void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            if (isPanelOpened)
            {
                if (lastClicked.Count > 0)
                {
                    lastClicked.Pop();
                }
            }

            isPanelOpened = !isPanelOpened;
            Panel.SetActive(isPanelOpened);
        }
    };
};
