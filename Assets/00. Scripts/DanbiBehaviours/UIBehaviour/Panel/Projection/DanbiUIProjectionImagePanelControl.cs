using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIProjectionImagePanelControl : DanbiUIPanelControl
    {
        EDanbiTextureType m_textureType = EDanbiTextureType.Regular;
        public EDanbiTextureType textureType => m_textureType;
        EDanbiTextureType updateTextureType
        {
            set
            {
                m_textureType = value;
                wipeOutPreview();
            }
        }

        [SerializeField, Readonly, Space(10)]
        Texture2D loadedTex;
        string texturePath;
        RawImage texturePreviewRawImage;
        TextMeshProUGUI textureResolutionText;
        TextMeshProUGUI textureNameText;

        public delegate void OnProjectionImageUpdate(Texture2D tex);
        public static OnProjectionImageUpdate onProjectionImageUpdate;

        void updatePreview(Texture tex)
        {
            if (tex.Null())
            {
                return;
            }

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
            PlayerPrefs.SetInt("ImageProjection-textureType", (int)m_textureType);
            PlayerPrefs.SetString("ImageProjection-texturePath", texturePath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            m_textureType = (EDanbiTextureType)PlayerPrefs.GetInt("ImageProjection-textureType", default);
            string prevTexturePath = PlayerPrefs.GetString("ImageProjection-texturePath", default);
            loadedTex = Resources.Load<Texture2D>(prevTexturePath);
            updatePreview(loadedTex);
            DanbiUISync.onPanelUpdated?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var textureTypeDropdown = panel.GetChild(0).GetComponent<TMP_Dropdown>();
            textureTypeDropdown.AddOptions(new List<string> { "Regular", "Panorama" });
            textureTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    updateTextureType = (EDanbiTextureType)option;
                    DanbiUISync.onPanelUpdated?.Invoke(this);
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
            onProjectionImageUpdate?.Invoke(loadedTex);
            DanbiUISync.onPanelUpdated?.Invoke(this);
        }
    };
};
