using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIImageGeneratorParametersPanelControl : DanbiUIPanelControl
    {
        public int MaximumBoundCount;
        public int SamplingThreshold;
        public Texture2D targetTex;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.ImageGeneratorParameters, this);

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
                () => { StartCoroutine(Coroutine_SelectTargetTexture(panel)); }
            );
        }

        IEnumerator Coroutine_SelectTargetTexture(Transform panel)
        {
            var filters = new string[] { ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG" };
            string startingPath = Application.dataPath + "/Resources/";

            yield return DanbiFileBrowser.OpenLoadDialog(startingPath,
                                                         filters,
                                                         "Load Target Texture",
                                                         "Select");

            DanbiFileBrowser.getActualResourcePath(out var actualPath, out var resourceName);

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

            // TODO: Sync the texture to the simulator.
        }
    };
};
