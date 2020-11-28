using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiImageWriter : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        string m_imageSavePathAndName;
        public string imageSavePathAndName { get => m_imageSavePathAndName; private set => m_imageSavePathAndName = value; }

        string m_imageSavePathOnly;
        public string imageSavePathOnly { get => m_imageSavePathOnly; private set => m_imageSavePathOnly = value; }

        [SerializeField, Readonly]
        Texture2D[] m_textures;
        public Texture2D[] tex => m_textures;

        EDanbiImageExt m_imageExt = EDanbiImageExt.png;
        public EDanbiImageExt imageExt => m_imageExt;

        void Awake()
        {
            DanbiUIImageGeneratorTexturePanel.onTexturesChange +=
                (List<Texture2D> tex, EDanbiTextureType type) => m_textures = tex.ToArray();

            DanbiUIImageGeneratorFilePathPanel.onFilePathChanged +=
                (string filePath) => m_imageSavePathOnly = filePath;

            DanbiUIImageGeneratorFilePathPanel.onFileSavePathAndNameChanged +=
                (string savePathAndName) => m_imageSavePathAndName = savePathAndName;
                
            DanbiUIImageGeneratorFilePathPanel.onFileExtChanged +=
                (EDanbiImageExt imgExt) => m_imageExt = imgExt;
        }
    };
};
