using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIRoomShapePanelControl : DanbiUIPanelControl
    {
        public float height { get; set; }
        public float width { get; set; }
        public float depth { get; set; }
        public float startingHeight { get; set; }

        protected override void BindPanelFields()
        {
            base.BindPanelFields();
            var panel = Panel.transform;

            // bind the height inputfield.
            var heightInputField = panel.GetChild(1).GetComponent<InputField>();
            heightInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    height = float.Parse(val);
                }
            );

            // bind the width inputfield.
            var widthInputField = panel.GetChild(3).GetComponent<InputField>();
            widthInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    width = float.Parse(val);
                }
            );

            // bind the depth inputfield.
            var depthInputField = panel.GetChild(3).GetComponent<InputField>();
            depthInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    depth = float.Parse(val);
                }
            );
            
            // bind the starting height InputField.
            var startingHeightInputField = panel.GetChild(7).GetComponent<InputField>();
            startingHeightInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    startingHeight = float.Parse(val);
                }
            );            
        }
    };
};
