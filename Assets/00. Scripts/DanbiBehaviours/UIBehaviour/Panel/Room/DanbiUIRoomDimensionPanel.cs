using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiUIRoomDimensionPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        float m_width;

        [SerializeField, Readonly]
        float m_height;

        [SerializeField, Readonly]
        float m_depth;

        public delegate void OnWidthUpdate(float width);
        public static OnWidthUpdate onWidthUpdate;

        public delegate void OnHeightUpdate(float height);
        public static OnHeightUpdate onHeightUpdate;

        public delegate void OnDepthUpdate(float depth);
        public static OnDepthUpdate onDepthUpdate;

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("RoomShapePanel-width", m_width);
            PlayerPrefs.SetFloat("RoomShapePanel-height", m_height);
            PlayerPrefs.SetFloat("RoomShapePanel-depth", m_depth);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevWidth = PlayerPrefs.GetFloat("RoomShapePanel-width", default);
            m_width = prevWidth;
            (uiElements[0] as InputField).text = prevWidth.ToString();
            onWidthUpdate?.Invoke(m_width);

            var prevHeight = PlayerPrefs.GetFloat("RoomShapePanel-height", default);
            m_height = prevHeight;
            (uiElements[1] as InputField).text = prevHeight.ToString();
            onHeightUpdate?.Invoke(m_height);

            var prevDepth = PlayerPrefs.GetFloat("RoomShapePanel-depth", default);
            m_depth = prevDepth;
            (uiElements[2] as InputField).text = prevDepth.ToString();
            onDepthUpdate?.Invoke(m_depth);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // bind the width inputfield.
            var widthInputField = panel.GetChild(0).GetComponent<InputField>();
            widthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_width = res;
                        onWidthUpdate?.Invoke(res);
                    }
                }
            );

            // bind the height inputfield.
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_height = res;
                        onHeightUpdate?.Invoke(res);
                    }
                }
            );

            // bind the depth inputfield.
            var depthInputField = panel.GetChild(2).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_depth = res;
                        onDepthUpdate?.Invoke(res);
                    }
                }
            );

            LoadPreviousValues(widthInputField, heightInputField, depthInputField);
        }
    };
};
