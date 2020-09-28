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
        [Readonly]
        public Texture2D targetTex;
        string actualPath;

        void OnDisable()
        {
            PlayerPrefs.SetInt("ImageGeneratorParameters-maximunBoundCount", maxBoundCount);
            PlayerPrefs.SetInt("ImageGeneratorParameters-samplingThreshold", samplingThreshold);
            PlayerPrefs.SetString("ImageGeneratorParameters-targetTex", actualPath);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();
            var panel = Panel.transform;

            var maxBoundCountInputField = panel.GetChild(0).GetComponent<InputField>();
            int prevMaxBoundCount = PlayerPrefs.GetInt("ImageGeneratorParameters-maximunBoundCount", default);
            maxBoundCountInputField.text = prevMaxBoundCount.ToString();
            maxBoundCount = prevMaxBoundCount;
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

            var samplingThresholdInputField = panel.GetChild(1).GetComponent<InputField>();
            int prevSamplingThreshold = PlayerPrefs.GetInt("ImageGeneratorParameters-samplingThreshold", default);
            samplingThresholdInputField.text = prevMaxBoundCount.ToString();
            samplingThreshold = prevSamplingThreshold;
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

            var selectTargetTextureButton = panel.GetChild(2).GetComponent<Button>();
            string prevTargetTexture = PlayerPrefs.GetString("ImageGeneratorParameters-targetTex", default);
            if (!string.IsNullOrEmpty(prevTargetTexture))
            {
                targetTex = Resources.Load<Texture2D>(prevTargetTexture);
                // Update the texture inspector.
                var texturePreviewRawImage = panel.GetChild(3).GetComponent<RawImage>();
                texturePreviewRawImage.texture = targetTex;

                var resolutionText = panel.GetChild(4).GetComponent<Text>();
                resolutionText.text = $"Resolution: {targetTex.width} X {targetTex.height}";

                var textureNameText = panel.GetChild(5).GetComponent<Text>();
                // textureNameText.text = $"Name: {resourceName}";
                DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
            }
            selectTargetTextureButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SelectTargetTexture(panel)); }
            );
        }

        IEnumerator Coroutine_SelectTargetTexture(Transform panel)
        {
            var filters = new string[] { ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG" };
            string startingPath = Application.dataPath + "/Resources/";

            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                         filters,
                                                         "Load Target Texture",
                                                         "Select");

            DanbiFileSys.GetResourcePathForResources(out actualPath, out var resourceName);

            // Load the texture.
            targetTex = Resources.Load<Texture2D>(actualPath);
            yield return new WaitUntil(() => !targetTex.Null());

            // Update the texture inspector.
            var texturePreviewRawImage = panel.GetChild(3).GetComponent<RawImage>();
            texturePreviewRawImage.texture = targetTex;

            var resolutionText = panel.GetChild(4).GetComponent<Text>();
            resolutionText.text = $"Resolution: {targetTex.width} X {targetTex.height}";

            var textureNameText = panel.GetChild(5).GetComponent<Text>();
            textureNameText.text = $"Name: {resourceName}";

            // DanbiUISync.Call_OnPanelUpdate?.Invoke(this);                        
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
