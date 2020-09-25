using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIRoomShapePanelControl : DanbiUIPanelControl
    {
        public float width { get; set; }
        public float height { get; set; }
        public float depth { get; set; }

        void OnDisable()
        {
            PlayerPrefs.SetFloat("RoomShapePanel-width", width);
            PlayerPrefs.SetFloat("RoomShapePanel-height", height);
            PlayerPrefs.SetFloat("RoomShapePanel-depth", depth);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            // DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.RoomShapePanel, this);
            var panel = Panel.transform;

            // bind the width inputfield.
            var widthInputField = panel.GetChild(0).GetComponent<InputField>();
            var prevWidth = PlayerPrefs.GetFloat("RoomShapePanel-width", default);
            widthInputField.text = prevWidth.ToString();
            width = prevWidth;
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
            var prevHeight = PlayerPrefs.GetFloat("RoomShapePanel-height", default);
            heightInputField.text = prevHeight.ToString();
            height = prevHeight;
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
            var prevDepth = PlayerPrefs.GetFloat("RoomShapePanel-depth", default);
            depthInputField.text = prevDepth.ToString();
            depth = prevDepth;
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
        }
    };
};
