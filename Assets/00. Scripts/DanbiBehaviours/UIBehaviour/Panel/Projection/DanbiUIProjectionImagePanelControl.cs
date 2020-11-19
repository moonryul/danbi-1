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
                WipeOutTexturePreview();
            }
        }

        [SerializeField, Readonly, Space(10)]
        Texture2D m_loadedTex;
        string m_texturePath;
        RawImage m_texturePreviewRawImage;
        TextMeshProUGUI m_textureResolutionText;
        TextMeshProUGUI m_textureNameText;

        public delegate void OnProjectionImageUpdate(Texture2D tex);
        public static OnProjectionImageUpdate onProjectionImageUpdate;

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ImageProjection-textureType", (int)m_textureType);
            PlayerPrefs.SetString("ImageProjection-texturePath", m_texturePath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            m_textureType = (EDanbiTextureType)PlayerPrefs.GetInt("ImageProjection-textureType", default);
            string prevTexturePath = PlayerPrefs.GetString("ImageProjection-texturePath", default);
            m_loadedTex = Resources.Load<Texture2D>(prevTexturePath);
            UpdateTexturePreview(m_loadedTex);
            DanbiUISync.onPanelUpdate?.Invoke(this);
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
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            var selectTextureButton = panel.GetChild(1).GetComponent<Button>();
            selectTextureButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetTexture()));

            m_texturePreviewRawImage = panel.GetChild(2).GetComponent<RawImage>();
            m_textureResolutionText = panel.GetChild(3).GetComponent<TextMeshProUGUI>();
            m_textureNameText = panel.GetChild(4).GetComponent<TextMeshProUGUI>();

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SelectTargetTexture()
        {
            var filters = new string[] { ".png", ".jpg" };
            string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filters,
                                                     "Load Texture",
                                                     "Select");

            DanbiFileSys.GetResourcePathIntact(out m_texturePath, out _);
            byte[] texBytes = System.IO.File.ReadAllBytes(m_texturePath);
            m_loadedTex = new Texture2D(2, 2);
            m_loadedTex.LoadImage(texBytes, false);

            yield return new WaitUntil(() => !m_loadedTex.Null());

            // Update the texture inspector.
            UpdateTexturePreview(m_loadedTex);
            onProjectionImageUpdate?.Invoke(m_loadedTex);
            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        void UpdateTexturePreview(Texture tex)
        {
            if (tex.Null())
            {
                return;
            }

            m_texturePreviewRawImage.texture = tex;
            m_textureResolutionText.text = $"Resolution: {tex.width} X {tex.height}";
            m_textureNameText.text = $"Name: {tex.name}";
        }

        void WipeOutTexturePreview()
        {
            m_texturePreviewRawImage.texture = default;
            m_textureResolutionText.text = default;
            m_textureNameText.text = default;
        }
    };
};
