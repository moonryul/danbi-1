using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    // [ExecuteInEditMode]
    public class DanbiPanoramaScreenChanger : MonoBehaviour
    {
        Renderer m_panoramaRegularRenderer;
        Renderer m_panorama360Renderer;
        Renderer m_panorama4facesRenderer;

        GameObject m_panoramaRegular;
        GameObject m_panorama360;
        GameObject m_panorama4faces;

        DanbiCubePanorama m_panoramaRegularInfo;
        DanbiCubePanorama m_panorama360Info;
        DanbiCubePanorama m_panorama4facesInfo;

        public delegate void OnCenterPosOfMeshUpdate_Panorama(Vector3 new_centerPosOfMesh);
        public static OnCenterPosOfMeshUpdate_Panorama onCenterPosOfMeshUpdate_Panorama;

        void Awake()
        {
            DanbiTextureManager.Init();

            m_panoramaRegular = transform.GetChild(1).gameObject;
            m_panoramaRegularRenderer = m_panoramaRegular.GetComponent<Renderer>();

            m_panorama4faces = transform.GetChild(2).gameObject;
            m_panorama4facesRenderer = m_panorama4faces.GetComponent<Renderer>();

            m_panorama360 = transform.GetChild(3).gameObject;
            m_panorama360Renderer = m_panorama360.GetComponent<Renderer>();


            // cache it for high, low.  
            m_panoramaRegularInfo = m_panoramaRegular.GetComponent<DanbiCubePanorama>();
            m_panorama360Info = m_panorama360.GetComponent<DanbiCubePanorama>();
            m_panorama4facesInfo = m_panorama360.GetComponent<DanbiCubePanorama>();

            DanbiUIImageGeneratorTexturePanel.onTexturesChange +=
                (List<Texture2D> tex, EDanbiTextureType type) =>
                {
                    switch (type)
                    {
                        case EDanbiTextureType.Regular:
                            m_panoramaRegularRenderer.material.SetTexture("_MainTex", tex[0]);
                            break;

                        case EDanbiTextureType.Panorama:
                            m_panorama360Renderer.material.SetTexture("_MainTex", tex[0]);
                            break;

                        case EDanbiTextureType.Faces4:
                            int idx = 0;
                            foreach (var i in tex)
                            {
                                m_panorama4facesRenderer.material.SetTexture($"_MainTex{idx++}", i);
                            }
                            m_panorama4facesRenderer.material.SetInt("_NumOfTargetTex", idx);
                            break;
                    }
                };
        }

        void Start()
        {
            // 1. Set Panorama Regular
            m_panoramaRegularRenderer.material.SetTexture("_MainTex", null);

            // 2. Set Panorama 360
            Vector3 panoramaOrigin = m_panoramaRegular.transform.localPosition;
            // height is the half of the total of high and low.
            Vector3 centerPosOfPanorama360Mesh = panoramaOrigin + new Vector3(0.0f,
                                                                     (m_panorama4facesInfo.shapeData.high * 0.01f - m_panorama4facesInfo.shapeData.low * 0.01f) * 0.5f,
                                                                     0.0f);

            m_panorama360Renderer.material.SetVector("_CenterOfMesh", centerPosOfPanorama360Mesh);
            onCenterPosOfMeshUpdate_Panorama?.Invoke(centerPosOfPanorama360Mesh);
            m_panorama360Renderer.material.SetTexture("_MainTex", null);

            // 3. Set Panorama 4faces
            m_panorama4facesRenderer.material.SetInt("_NumOfTargetTex", 1);
            m_panorama4facesRenderer.material.SetTexture("_MainTex0", null);
            m_panorama4facesRenderer.material.SetTexture("_MainTex1", null);
            m_panorama4facesRenderer.material.SetTexture("_MainTex2", null);
            m_panorama4facesRenderer.material.SetTexture("_MainTex3", null);
        }
    };
};
