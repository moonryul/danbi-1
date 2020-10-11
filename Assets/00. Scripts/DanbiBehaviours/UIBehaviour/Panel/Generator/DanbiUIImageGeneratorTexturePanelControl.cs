using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIImageGeneratorTexturePanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public Texture2D loadedTex;
        string texturePath;

        RawImage texturePreviewRawImage;
        Text textureResolutionText;
        Text textureNameText;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("ImageGeneratorParameters-loadedTex", texturePath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevTargetTexture = PlayerPrefs.GetString("ImageGeneratorParameters-loadedTex", default);
            if (!string.IsNullOrEmpty(prevTargetTexture))
            {
                loadedTex = Resources.Load<Texture2D>(prevTargetTexture);

                // Update the texture inspector.
                texturePreviewRawImage.texture = loadedTex;
                textureResolutionText.text = $"Resolution: {loadedTex.width} X {loadedTex.height}";
                textureNameText.text = $"Name: {loadedTex.name}";
            }

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var selectTextureButton = panel.GetChild(0).GetComponent<Button>();
            selectTextureButton.onClick.AddListener(()
                => StartCoroutine(
                       Coroutine_SelectTargetTexture(texturePreviewRawImage, textureResolutionText, textureNameText)));

            texturePreviewRawImage = panel.GetChild(1).GetComponent<RawImage>();
            textureResolutionText = panel.GetChild(2).GetComponent<Text>();
            textureNameText = panel.GetChild(3).GetComponent<Text>();

            LoadPreviousValues(selectTextureButton);
        }

        IEnumerator Coroutine_SelectTargetTexture(RawImage preview, Text resolution, Text name)
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

            DanbiFileSys.GetResourcePathForResources(out texturePath, out _);

            // Load the texture.
            loadedTex = Resources.Load<Texture2D>(texturePath);
            yield return new WaitUntil(() => !loadedTex.Null());

            // Update the texture inspector.
            preview.texture = loadedTex;
            resolution.text = $"Resolution: {loadedTex.width} X {loadedTex.height}";
            name.text = $"Name: {loadedTex.name}";

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
