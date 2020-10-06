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
        public Texture2D loadedTex;
        string texturePath;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ImageGeneratorParameters-maximunBoundCount", maxBoundCount);
            PlayerPrefs.SetInt("ImageGeneratorParameters-samplingThreshold", samplingThreshold);
            PlayerPrefs.SetString("ImageGeneratorParameters-loadedTex", texturePath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            int prevMaxBoundCount = PlayerPrefs.GetInt("ImageGeneratorParameters-maximunBoundCount", default);
            maxBoundCount = prevMaxBoundCount;
            (uiElements[0] as InputField).text = prevMaxBoundCount.ToString();

            int prevSamplingThreshold = PlayerPrefs.GetInt("ImageGeneratorParameters-samplingThreshold", default);
            samplingThreshold = prevSamplingThreshold;
            (uiElements[1] as InputField).text = prevSamplingThreshold.ToString();

            string prevTargetTexture = PlayerPrefs.GetString("ImageGeneratorParameters-loadedTex", default);
            if (!string.IsNullOrEmpty(prevTargetTexture))
            {
                loadedTex = Resources.Load<Texture2D>(prevTargetTexture);
                // Update the texture inspector.
                var texturePreviewRawImage = Panel.transform.GetChild(3).GetComponent<RawImage>();
                texturePreviewRawImage.texture = loadedTex;

                var resolutionText = Panel.transform.GetChild(4).GetComponent<Text>();
                resolutionText.text = $"Resolution: {loadedTex.width} X {loadedTex.height}";

                var textureNameText = Panel.transform.GetChild(5).GetComponent<Text>();
                textureNameText.text = $"Name: {loadedTex}";                
            }

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

            var selectTargetTextureButton = panel.GetChild(2).GetComponent<Button>();
            selectTargetTextureButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetTexture(panel)));

            LoadPreviousValues(maxBoundCountInputField, samplingThresholdInputField);
        }

        IEnumerator Coroutine_SelectTargetTexture(Transform panel)
        {
            var filters = new string[] { ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG" };
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

            DanbiFileSys.GetResourcePathForResources(out texturePath, out var resourceName);

            // Load the texture.
            loadedTex = Resources.Load<Texture2D>(texturePath);
            yield return new WaitUntil(() => !loadedTex.Null());

            // Update the texture inspector.
            var texturePreviewRawImage = panel.GetChild(3).GetComponent<RawImage>();
            texturePreviewRawImage.texture = loadedTex;

            var resolutionText = panel.GetChild(4).GetComponent<Text>();
            resolutionText.text = $"Resolution: {loadedTex.width} X {loadedTex.height}";

            var textureNameText = panel.GetChild(5).GetComponent<Text>();
            textureNameText.text = $"Name: {resourceName}";

            // DanbiUISync.Call_OnPanelUpdate?.Invoke(this);                        
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
