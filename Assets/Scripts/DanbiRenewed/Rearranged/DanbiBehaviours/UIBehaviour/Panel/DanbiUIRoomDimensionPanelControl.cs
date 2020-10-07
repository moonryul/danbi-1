using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIRoomDimensionPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public float width;

        [Readonly]
        public float height;
        
        [Readonly]
        public float depth;        

        protected override void SaveValues()
        {
            PlayerPrefs.SetFloat("RoomShapePanel-width", width);
            PlayerPrefs.SetFloat("RoomShapePanel-height", height);
            PlayerPrefs.SetFloat("RoomShapePanel-depth", depth);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevWidth = PlayerPrefs.GetFloat("RoomShapePanel-width", default);
            width = prevWidth;
            (uiElements[0] as InputField).text = prevWidth.ToString();

            var prevHeight = PlayerPrefs.GetFloat("RoomShapePanel-height", default);
            height = prevHeight;
            (uiElements[1] as InputField).text = prevHeight.ToString();

            var prevDepth = PlayerPrefs.GetFloat("RoomShapePanel-depth", default);
            depth = prevDepth;
            (uiElements[2] as InputField).text = prevDepth.ToString();
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
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
                    if (float.TryParse(val, out var asFloat))
                    {
                        width = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // bind the height inputfield.
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        height = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            // bind the depth inputfield.
            var depthInputField = panel.GetChild(2).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        depth = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(widthInputField, heightInputField, depthInputField);
        }
    };
};
