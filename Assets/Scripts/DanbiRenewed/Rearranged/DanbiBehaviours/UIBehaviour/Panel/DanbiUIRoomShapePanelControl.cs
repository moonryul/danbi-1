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
        public float startingHeight { get; set; }

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
                    }
                }
            );

            // bind the starting height InputField.
            var startingHeightInputField = panel.GetChild(3).GetComponent<InputField>();
            startingHeightInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        startingHeight = asFloat;
                    }
                }
            );
        }
    };
};
