using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiUIImageGeneratorTexturePanelControl : DanbiUIPanelControl
    {
        EDanbiTextureType TextureType = EDanbiTextureType.Normal;
        EDanbiTextureType updateTextureType
        {
            set
            {
                TextureType = value;
                wipeOutPreview();
            }
        }

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

        void wipeOutPreview()
        {
            texturePreviewRawImage.texture = default;
            textureResolutionText.text = default;
            textureNameText.text = default;
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ImageGenerator-textureType", (int)TextureType);
            PlayerPrefs.SetString("ImageGenerator-texturePath", texturePath);
        }
        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            TextureType = (EDanbiTextureType)PlayerPrefs.GetInt("ImageGenerator-textureType", default);
            string prevTexturePath = PlayerPrefs.GetString("ImageGenerator-texturePath", default);
            loadedTex = Resources.Load<Texture2D>(prevTexturePath);
            updatePreview(loadedTex);
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var textureTypeDropdown = panel.GetChild(0).GetComponent<TMP_Dropdown>();
            textureTypeDropdown.AddOptions(new List<string> { "Normal", "Panorama" });
            textureTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    updateTextureType = (EDanbiTextureType)option;
                    // textureTypeDropdown.refresh
                }
            );

            var selectTextureButton = panel.GetChild(1).GetComponent<Button>();
            selectTextureButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetTexture()));

            texturePreviewRawImage = panel.GetChild(2).GetComponent<RawImage>();
            textureResolutionText = panel.GetChild(3).GetComponent<TextMeshProUGUI>();
            textureNameText = panel.GetChild(4).GetComponent<TextMeshProUGUI>();

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SelectTargetTexture()
        {
            var filters = new string[] { ".png", ".jpg" };
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
            // yield return new WaitUntil(() => !tex.Null());

            // Update the texture inspector.
            updatePreview(loadedTex);

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
