using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIVideoGeneratorParametersPanelControl : DanbiUIPanelControl
    {
        public int MaximumBoundCount;
        public int SamplingThreshold;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var maxBoundCountInputField = panel.GetChild(0).GetComponent<InputField>();
            maxBoundCountInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        MaximumBoundCount = asInt;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
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
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
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
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif

            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                         filters,
                                                         "Load Target Texture",
                                                         "Select");

            DanbiFileSys.GetResourcePathForResources(out var actualPath, out var resourceName);

            // Load the texture.
            //TODO: re-write to video!!
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
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
