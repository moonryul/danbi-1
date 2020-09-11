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

        protected virtual void Start()
        {
            Panel = transform.GetChild(1).GetChild(0).gameObject;
            if (!Panel.name.Contains("Panel"))
            {
                Panel = null;
            }
            else
            {
                Panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            BindPanelFields();
            //DanbiUIControl.Call_OnPanelDataUpdated(this);
        }

        protected virtual void BindPanelFields()
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
