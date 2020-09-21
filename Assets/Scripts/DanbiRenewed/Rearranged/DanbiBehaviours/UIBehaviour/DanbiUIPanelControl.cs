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

        [HideInInspector]
        protected GameObject Panel;

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
            //DanbiUIControl.Call_OnPanelDataUpdated(this);
        }

        protected virtual void AddListenerForPanelFields()
        {
            if (Panel.Null())
            {
                return;
            }
        }

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
