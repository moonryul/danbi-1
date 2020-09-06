using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    /// <summary>
    /// 
    /// </summary>
    public class DanbiUIBaseElement : MonoBehaviour
    {
        [SerializeField, Readonly]
        bool isPanelOpened = false;

        GameObject Panel;

        void Start()
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
