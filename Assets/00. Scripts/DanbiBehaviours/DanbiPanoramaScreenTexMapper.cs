using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    // [ExecuteInEditMode]
    public class DanbiPanoramaScreenTexMapper : MonoBehaviour
    {
        [SerializeField, Readonly]
        Texture[] m_regularPanoramaTexArr = new Texture[2];

        [SerializeField, Readonly]
        Material[] m_regularPanoramaMatArr = new Material[2];
        Renderer m_panoramaRenderer;
        DanbiCubePanorama m_panoramaInfo;
        Transform m_panorama;

        public delegate void OnCenterPosOfMeshUpdate_Panorama(Vector3 new_centerPosOfMesh);
        public static OnCenterPosOfMeshUpdate_Panorama onCenterPosOfMeshUpdate_Panorama;

        void Awake()
        {
            DanbiUISync.onPanelUpdate += OnPanelUpdate;

            m_regularPanoramaTexArr[0] = Resources.Load<Texture2D>("Textures/calib_test");
            m_regularPanoramaTexArr[1] = Resources.Load<Texture2D>("Textures/pano_test");
            // Load regular shader and  panorama mapping shader
            m_regularPanoramaMatArr[0] = new Material(Shader.Find("danbi/SimpleTextureMappingCullOff"));
            m_regularPanoramaMatArr[1] = new Material(Shader.Find("danbi/PanoramaCube-Panorama"));

            // panorama is the second child of the dome / cube which is (this.gameObject).
            m_panorama = this.gameObject.transform.GetChild(2);
            m_panoramaRenderer = m_panorama.GetComponent<Renderer>();
            // cache it for high, low.  
            m_panoramaInfo = m_panorama.GetComponent<DanbiCubePanorama>();
        }

        void Start()
        {
            int materialIdx = 0;
            SetMaterial(materialIdx);
        }

        void SetMaterial(int materialIdx)
        {
            // material Idx == 0 -> Regular shader
            // == 1 -> 360 panorama shader
            if (materialIdx != 0 || materialIdx != 1)
            {
                return;
            }

            // change material.
            m_panoramaRenderer.material = m_regularPanoramaMatArr[materialIdx];

            // regular
            if (materialIdx == 0)
            {
                m_panoramaRenderer.material.SetTexture("_MainTex", m_regularPanoramaTexArr[materialIdx]);
            }
            else
            // panorama
            if (materialIdx == 1)
            {
                Vector3 panoramaOrigin = m_panorama.localPosition;
                // height is the half of the total of high and low.
                Vector3 centerPosOfPanoramaMesh = panoramaOrigin + new Vector3(0.0f,
                                                                         (m_panoramaInfo.shapeData.high * 0.01f - m_panoramaInfo.shapeData.low * 0.01f) * 0.5f,
                                                                         0.0f);

                m_panoramaRenderer.material.SetVector("_CenterOfMesh", centerPosOfPanoramaMesh);
                onCenterPosOfMeshUpdate_Panorama?.Invoke(centerPosOfPanoramaMesh);
                m_panoramaRenderer.material.SetTexture("_MainTex", m_regularPanoramaTexArr[materialIdx]);
            }
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var texControl = control as DanbiUIImageGeneratorTexturePanelControl;

                SetMaterial((int)texControl.textureType);
            }

            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var vidControl = control as DanbiUIVideoGeneratorVideoPanelControl;

                SetMaterial((int)vidControl.vidType);
            }
        }
    };
};
