using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVideoGeneratorParametersPanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public int maxBoundCount;
        [Readonly]
        public int samplingThreshold;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("VideoGeneratorParameters-maxBoundCount", maxBoundCount);
            PlayerPrefs.SetInt("VideoGeneratorParameters-samplingThreshold", samplingThreshold);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            int prevMaxBoundCount = PlayerPrefs.GetInt("VideoGeneratorParameters-maxBoundCount", default);
            maxBoundCount = prevMaxBoundCount;
            (uiElements[0] as InputField).text = prevMaxBoundCount.ToString();

            int prevSamplingThreshold = PlayerPrefs.GetInt("VideoGeneratorParamsters-samplingThreshold", default);
            samplingThreshold = prevSamplingThreshold;
            (uiElements[1] as InputField).text = prevSamplingThreshold.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
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
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
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
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );

            LoadPreviousValues(maxBoundCountInputField, samplingThresholdInputField);
        }        
    };
};
