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

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            // DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.RoomShapePanel, this);
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
        }
    };
};
