using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiImageControl : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        string m_imageSavePathAndName;
        public string imageSavePathAndName { get => m_imageSavePathAndName; private set => m_imageSavePathAndName = value; }

        string m_imageSavePathOnly;
        public string imageSavePathOnly { get => m_imageSavePathOnly; private set => m_imageSavePathOnly = value; }

        [SerializeField, Readonly]
        Texture2D m_panoramaTex;
        public Texture2D panoramaTex { get => m_panoramaTex; private set => m_panoramaTex = value; }

        EDanbiImageType m_imageType = EDanbiImageType.png;
        public EDanbiImageType imageType { get => m_imageType; private set => m_imageType = value; }

        void Start()
        {
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var texturePanel = control as DanbiUIImageGeneratorTexturePanelControl;
                // Debug.Log($"Texture is loaded!", this);
                m_panoramaTex = texturePanel.loadedTex;
            }

            // if (control is DanbiUIVideoGeneratorParametersPanelControl)
            // {
            //     //var videoPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
            //     // TargetPanoramaTex = videoPanel;                
            // }

            // Update image file paths
            if (control is DanbiUIImageGeneratorFilePathPanelControl)
            {
                var fileSavePanel = control as DanbiUIImageGeneratorFilePathPanelControl;

                m_imageSavePathAndName = fileSavePanel.fileSavePathAndName;
                m_imageSavePathOnly = fileSavePanel.filePath;
                m_imageType = fileSavePanel.imageType;
            }
        }
    };
};
