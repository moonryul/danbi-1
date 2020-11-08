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
            DanbiUISync.onPanelUpdated += OnPanelUpdate;

            // panorama is the second child of the dome / cube which is (this.gameObject).
            // m_panorama = this.gameObject.transform.GetChild(1);
            m_panorama = transform.GetChild(2);
            m_panoramaRenderer = m_panorama.GetComponent<Renderer>();
            OnUsedMaterialChanged(m_panoramaRenderer, 0);
            // cache it for high, low.  
            m_panoramaInfo = m_panorama.GetComponent<DanbiCubePanorama>();
        }

        void Start()
        {
            m_regularPanoramaTexArr[0] = Resources.Load<Texture2D>("Textures/calib_test");
            m_regularPanoramaTexArr[1] = Resources.Load<Texture2D>("Textures/pano_test");
            // Load regular shader and  panorama mapping shader
            m_regularPanoramaMatArr[0] = new Material(Shader.Find("danbi/SimpleTextureMappingCullOff"));
            m_regularPanoramaMatArr[1] = new Material(Shader.Find("danbi/PanoramicCustom"));


        }

        void OnUsedMaterialChanged(Renderer curRenderer, int matIdx)
        {
            // change material.
            curRenderer.material = m_regularPanoramaMatArr[matIdx];

            // regular
            if (matIdx == 0)
            {
                curRenderer.material.SetTexture("_MainTex", m_regularPanoramaTexArr[matIdx]);
            }
            else
            // panorama
            if (matIdx == 1)
            {
                Vector3 panoramaOrigin = m_panorama.localPosition;
                // height is the half of the total of high and low.
                Vector3 centerPosOfPanoramaMesh = panoramaOrigin + new Vector3(0.0f,
                                                                         (m_panoramaInfo.shapeData.high * 0.01f - m_panoramaInfo.shapeData.low * 0.01f) * 0.5f,
                                                                         0.0f);

                curRenderer.material.SetVector("_CenterOfMesh", centerPosOfPanoramaMesh);
                onCenterPosOfMeshUpdate_Panorama?.Invoke(centerPosOfPanoramaMesh);
                curRenderer.material.SetTexture("_MainTex", m_regularPanoramaTexArr[matIdx]);
            }
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var texControl = control as DanbiUIImageGeneratorTexturePanelControl;

                OnUsedMaterialChanged(m_panoramaRenderer, (int)texControl.textureType);
            }

            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var vidControl = control as DanbiUIVideoGeneratorVideoPanelControl;

                OnUsedMaterialChanged(m_panoramaRenderer, (int)vidControl.vidType);
                Debug.Log($"Video type is changed to {vidControl.vidType}");
            }
        }
    };
};
