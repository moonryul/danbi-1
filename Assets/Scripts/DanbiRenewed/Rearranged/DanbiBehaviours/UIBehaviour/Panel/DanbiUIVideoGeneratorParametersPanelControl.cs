using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVideoGeneratorParametersPanelControl : DanbiUIPanelControl
    {
        int MaximumBoundCount, SamplingThreshold;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.VideoGeneratorParameters, this);

            var panel = Panel.transform;

            var maxBoundCountInputField = panel.GetChild(0).GetComponent<InputField>();
            maxBoundCountInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        MaximumBoundCount = asInt;
                    }
                }
            );

            var samplingThresholdInputField = panel.GetChild(1).GetComponent<InputField>();
            samplingThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        SamplingThreshold = asInt;
                    }
                }
            );

            var selectTargetTextureButton = panel.GetChild(2).GetComponent<Button>();
            selectTargetTextureButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SelectTargetVideo(panel)); }
            );
        }

        IEnumerator Coroutine_SelectTargetVideo(Transform panel)
        {
            var filters = new string[] { ".mp4", ".MP4" };
            string startingPath = Application.dataPath + "/Resources/";

            yield return DanbiFileBrowser.OpenLoadDialog(startingPath,
                                                         filters,
                                                         "Load Target Texture",
                                                         "Select");

            DanbiFileBrowser.getActualResourcePath(out var actualPath, out var resourceName);

            // Load the texture.
            var targetVid = Resources.Load<Texture2D>(actualPath);
            yield return new WaitUntil(() => !targetVid.Null());

            // Update the texture inspector.

            var videoNameText = panel.GetChild(3).GetComponent<Text>();
            videoNameText.text = $"Name: {resourceName}";

            var frameCountText = panel.GetChild(4).GetComponent<Text>();
            frameCountText.text = $"Frame Count: ";

            var lengthText = panel.GetChild(5).GetComponent<Text>();
            lengthText.text = $"Name: ";            

            // TODO: Sync the texture to the simulator.
        }
    };
};
