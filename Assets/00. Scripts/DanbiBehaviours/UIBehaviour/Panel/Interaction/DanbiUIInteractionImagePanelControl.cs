using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIInteractionImagePanelControl : DanbiUIPanelControl
    {
        [Readonly]
        public Texture2D loadedTex;
        string texturePath;
        RawImage texturePreviewRawImage;
        TextMeshProUGUI textureResolutionText;
        TextMeshProUGUI textureNameText;

        void updatePreview(Texture tex)
        {
            if (tex.Null())
                return;

            texturePreviewRawImage.texture = tex;
            textureResolutionText.text = $"Resolution: {tex.width} X {tex.height}";
            textureNameText.text = $"Name: {tex.name}";
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("ImageGenerator-texturePath", texturePath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevTexturePath = PlayerPrefs.GetString("ImageGenerator-texturePath", default);
            loadedTex = Resources.Load<Texture2D>(prevTexturePath);
            updatePreview(loadedTex);
            DanbiUISync.onPanelUpdated?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var selectTextureButton = panel.GetChild(0).GetComponent<Button>();
            selectTextureButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetTexture()));

            texturePreviewRawImage = panel.GetChild(1).GetComponent<RawImage>();
            textureResolutionText = panel.GetChild(2).GetComponent<TextMeshProUGUI>();
            textureNameText = panel.GetChild(3).GetComponent<TextMeshProUGUI>();

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SelectTargetTexture()
        {
            var filters = new string[] { ".jpg", ".png" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else            
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif

            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filters,
                                                     "Load Panorama Texture",
                                                     "Select");

            DanbiFileSys.GetResourcePathForResources(out texturePath, out _);

            // Load the texture.
            loadedTex = Resources.Load<Texture2D>(texturePath);
            yield return new WaitUntil(() => !loadedTex.Null());

            // Update the texture inspector.
            updatePreview(loadedTex);

            DanbiUISync.onPanelUpdated?.Invoke(this);
        }
    };
};
