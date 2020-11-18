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
        EDanbiTextureType m_textureType = EDanbiTextureType.Regular;
        public EDanbiTextureType textureType => m_textureType;
        EDanbiTextureType updateTextureType
        {
            set
            {
                m_textureType = value;
                wipeOutPreviews();
            }
        }

        [Readonly]
        public List<Texture2D> m_loadedTextures;
        string[] m_texturePaths = new string[4];
        RawImage[] m_texturePreviewRawImages = new RawImage[4];
        TMP_Text[] m_textureResolutionsTexts = new TMP_Text[4];
        TMP_Text[] m_textureNamesTexts = new TMP_Text[4];
        Button[] m_selectTextureButtons = new Button[4];

        Vector2 m_originalSize;
        Vector2 m_4facesSize;

        [Readonly]
        public int m_usedTextures = 0;


        //  4 faces images
        // 1. textureType 에서 4 faces 선택.
        // 2. 기존 레이아웃을 1 x 4 로 변경.        

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt("ImageGenerator-textureType", (int)m_textureType);
            // TODO:
        }
        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            // TODO:
            // DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var panelRect = Panel.GetComponent<RectTransform>();
            m_loadedTextures = new List<Texture2D>();

            // locate upward a little bit.
            panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, panelRect.anchoredPosition.y + 30.0f);

            // change the panel size.
            m_originalSize = panelRect.sizeDelta;
            m_4facesSize = new Vector2(panelRect.sizeDelta.x * 2.0f, panelRect.sizeDelta.y * 2.0f);

            // 1. bind Texture Type Dropdown
            var textureTypeDropdown = panel.GetChild(0).GetComponent<TMP_Dropdown>();
            textureTypeDropdown.AddOptions(new List<string> { "Regular", "Panorama", "4 Faces" });
            textureTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    updateTextureType = (EDanbiTextureType)option;
                    m_loadedTextures.Clear();

                    if ((EDanbiTextureType)option == EDanbiTextureType.Faces4)
                    {
                        TogglePreviewsOn4FacesTextureType(true);
                        SetUILayoutOnTextureType(true, panelRect);
                    }
                    else
                    {
                        TogglePreviewsOn4FacesTextureType(false);
                        SetUILayoutOnTextureType(false, panelRect);
                    }
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // 2. bind first texture.
            Transform firstTexTf = panel.GetChild(1);
            m_selectTextureButtons[0] = firstTexTf.GetChild(0).GetComponent<Button>();
            m_selectTextureButtons[0].onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_SelectTargetTexture(m_texturePaths[0], 0));
                });
            m_texturePreviewRawImages[0] = firstTexTf.GetChild(1).GetComponent<RawImage>();
            m_textureResolutionsTexts[0] = firstTexTf.GetChild(2).GetComponent<TMP_Text>();
            m_textureNamesTexts[0] = firstTexTf.GetChild(3).GetComponent<TMP_Text>();

            Vector2 firstSelectTextureButtonPos = m_selectTextureButtons[0].GetComponent<RectTransform>().anchoredPosition;
            Vector2 firstPrevieRawImagePos = m_texturePreviewRawImages[0].rectTransform.anchoredPosition;
            Vector2 firstResolutionText = m_textureResolutionsTexts[0].rectTransform.anchoredPosition;
            Vector2 firstNameText = m_textureNamesTexts[0].rectTransform.anchoredPosition;

            float positionOffsetX = 220.0f;
            float positionOffsetY = -260.0f;

            // 3. bind second texture
            Transform secondTexTf = panel.GetChild(2);
            m_selectTextureButtons[1] = secondTexTf.GetChild(0).GetComponent<Button>();
            m_selectTextureButtons[1].onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_SelectTargetTexture(m_texturePaths[1], 1));
                });
            m_texturePreviewRawImages[1] = secondTexTf.GetChild(1).GetComponent<RawImage>();
            m_textureResolutionsTexts[1] = secondTexTf.GetChild(2).GetComponent<TMP_Text>();
            m_textureNamesTexts[1] = secondTexTf.GetChild(3).GetComponent<TMP_Text>();

            m_selectTextureButtons[1].GetComponent<RectTransform>().anchoredPosition = firstSelectTextureButtonPos + new Vector2(positionOffsetX, 0.0f);
            m_texturePreviewRawImages[1].rectTransform.anchoredPosition = firstPrevieRawImagePos + new Vector2(positionOffsetX, 0.0f);
            m_textureResolutionsTexts[1].rectTransform.anchoredPosition = firstResolutionText + new Vector2(positionOffsetX, 0.0f);
            m_textureNamesTexts[1].rectTransform.anchoredPosition = firstNameText + new Vector2(positionOffsetX, 0.0f);

            // 4. bind third texture
            Transform thirdTexTf = panel.GetChild(3);
            m_selectTextureButtons[2] = thirdTexTf.GetChild(0).GetComponent<Button>();
            m_selectTextureButtons[2].onClick.AddListener(
                () => 
                {
                    StartCoroutine(Coroutine_SelectTargetTexture(m_texturePaths[2], 2));
                });
            m_texturePreviewRawImages[2] = thirdTexTf.GetChild(1).GetComponent<RawImage>();
            m_textureResolutionsTexts[2] = thirdTexTf.GetChild(2).GetComponent<TMP_Text>();
            m_textureNamesTexts[2] = thirdTexTf.GetChild(3).GetComponent<TMP_Text>();

            m_selectTextureButtons[2].GetComponent<RectTransform>().anchoredPosition = firstSelectTextureButtonPos + new Vector2(firstPrevieRawImagePos.x - 30.0f, positionOffsetY);
            m_texturePreviewRawImages[2].rectTransform.anchoredPosition = firstPrevieRawImagePos + new Vector2(firstPrevieRawImagePos.x - 30.0f, positionOffsetY);
            m_textureResolutionsTexts[2].rectTransform.anchoredPosition = firstResolutionText + new Vector2(firstPrevieRawImagePos.x - 30.0f, positionOffsetY);
            m_textureNamesTexts[2].rectTransform.anchoredPosition = firstNameText + new Vector2(firstPrevieRawImagePos.x - 30.0f, positionOffsetY);

            // 5. bind fourth texture
            Transform fourthTexTf = panel.GetChild(4);
            m_selectTextureButtons[3] = fourthTexTf.GetChild(0).GetComponent<Button>();
            m_selectTextureButtons[3].onClick.AddListener(
                () => 
                {
                    StartCoroutine(Coroutine_SelectTargetTexture(m_texturePaths[3], 3));
                });
            m_texturePreviewRawImages[3] = fourthTexTf.GetChild(1).GetComponent<RawImage>();
            m_textureResolutionsTexts[3] = fourthTexTf.GetChild(2).GetComponent<TMP_Text>();
            m_textureNamesTexts[3] = fourthTexTf.GetChild(3).GetComponent<TMP_Text>();

            m_selectTextureButtons[3].GetComponent<RectTransform>().anchoredPosition = firstSelectTextureButtonPos + new Vector2(positionOffsetX, positionOffsetY);
            m_texturePreviewRawImages[3].rectTransform.anchoredPosition = firstPrevieRawImagePos + new Vector2(positionOffsetX, positionOffsetY);
            m_textureResolutionsTexts[3].rectTransform.anchoredPosition = firstResolutionText + new Vector2(positionOffsetX, positionOffsetY);
            m_textureNamesTexts[3].rectTransform.anchoredPosition = firstNameText + new Vector2(positionOffsetX, positionOffsetY);

            // 6. init textures.
            for (int i = 0; i < 4; ++i)
            {
                m_textureResolutionsTexts[i].text = "Resolution : ---";
                m_textureNamesTexts[i].text = "Texture Name : ---";
            }

            // 7. these 4faces textures are turned on only when the type is "4faces", so turned them all off by default.
            TogglePreviewsOn4FacesTextureType(false);

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SelectTargetTexture(string loadedTexResultPath, int idx)
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

            DanbiFileSys.GetResourcePathForResources(out loadedTexResultPath, out _);

            // Load the texture.
            var loadedTex = Resources.Load<Texture2D>(loadedTexResultPath);

            yield return new WaitUntil(() => !loadedTex.Null());

            if (m_textureType == EDanbiTextureType.Faces4)
            {
                if (m_loadedTextures.Count > 4)
                {
                    m_loadedTextures.Clear();
                }
                m_loadedTextures.Add(loadedTex);
            }
            else
            {
                m_loadedTextures.Clear();
                m_loadedTextures.Add(loadedTex);
            }
            
            updatePreview(loadedTex, idx);

            ++m_usedTextures;

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        void SetUILayoutOnTextureType(bool is4Faces, RectTransform panel)
        {
            wipeOutPreviews();

            if (is4Faces)
            {
                panel.sizeDelta = m_4facesSize;
            }
            else
            {
                panel.sizeDelta = m_originalSize;
            }
        }

        void updatePreview(Texture2D tex, int idx)
        {
            m_texturePreviewRawImages[idx].texture = tex;
            m_textureResolutionsTexts[idx].text = $"Resolution : {tex.width} X {tex.height}";
            m_textureNamesTexts[idx].text = $"Texture Name : {tex.name}";
        }

        void wipeOutPreviews()
        {
            for (var i = 0; i < 4; ++i)
            {
                m_texturePreviewRawImages[i].texture = null;
                m_textureResolutionsTexts[i].text = "Resolution : ---";
                m_textureNamesTexts[i].text = "Texture Name : ---";
            }
            m_usedTextures = 0;
        }

        void TogglePreviewsOn4FacesTextureType(bool turnOnUI)
        {
            if (!turnOnUI)
            {
                m_usedTextures = 0;
            }

            for (int i = 1; i < 4; ++i)
            {
                m_selectTextureButtons[i].gameObject.SetActive(turnOnUI);
                m_texturePreviewRawImages[i].gameObject.SetActive(turnOnUI);
                m_textureResolutionsTexts[i].gameObject.SetActive(turnOnUI);
                m_textureNamesTexts[i].gameObject.SetActive(turnOnUI);
            }
        }
    };
};
