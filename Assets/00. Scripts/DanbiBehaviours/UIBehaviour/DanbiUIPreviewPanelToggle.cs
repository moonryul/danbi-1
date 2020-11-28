using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Danbi
{
    public class DanbiUIPreviewPanelToggle : MonoBehaviour
    {
        Button m_toggleButton;
        TMP_Text m_toggleText;
        bool m_collapsed = true;
        RectTransform m_previewPanelRectTransform;

        void Start()
        {
            m_toggleButton = GetComponent<Button>();
            m_toggleText = m_toggleButton.transform.GetChild(0).GetComponent<TMP_Text>();
            m_previewPanelRectTransform = transform.parent.GetComponent<RectTransform>();
            m_toggleButton.onClick.AddListener(
                () =>
                {
                    if (m_collapsed)
                    {
                        m_collapsed = false;
                        m_toggleText.text = "<";
                        m_previewPanelRectTransform.anchoredPosition += new Vector2(m_previewPanelRectTransform.sizeDelta.x, 0.0f);
                    }
                    else
                    {
                        m_collapsed = true;
                        m_toggleText.text = ">";
                        m_previewPanelRectTransform.anchoredPosition -= new Vector2(m_previewPanelRectTransform.sizeDelta.x, 0.0f);
                    }
                }
            );
        }
    };
};
