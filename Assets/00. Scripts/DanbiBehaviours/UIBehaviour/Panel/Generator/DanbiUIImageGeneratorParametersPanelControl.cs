using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIImageGeneratorParametersPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public int maxBoundCount;
        [Readonly]
        public int samplingThreshold;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ImageGeneratorParameters-maximunBoundCount", maxBoundCount);
            PlayerPrefs.SetInt("ImageGeneratorParameters-samplingThreshold", samplingThreshold);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            int prevMaxBoundCount = PlayerPrefs.GetInt("ImageGeneratorParameters-maximunBoundCount", default);
            maxBoundCount = prevMaxBoundCount;
            (uiElements[0] as InputField).text = prevMaxBoundCount.ToString();

            int prevSamplingThreshold = PlayerPrefs.GetInt("ImageGeneratorParameters-samplingThreshold", default);
            samplingThreshold = prevSamplingThreshold;
            (uiElements[1] as InputField).text = prevSamplingThreshold.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            // 1. bind the max bound count
            var maxBoundCountInputField = panel.GetChild(0).GetComponent<InputField>();
            maxBoundCountInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        maxBoundCount = asInt;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            // 2. bind the sampling threshold
            var samplingThresholdInputField = panel.GetChild(1).GetComponent<InputField>();
            samplingThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        samplingThreshold = asInt;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(maxBoundCountInputField, samplingThresholdInputField);
        }
    };
};
