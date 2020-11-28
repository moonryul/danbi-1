using UnityEngine;
using System.Collections.Generic;

namespace Danbi
{
    public static class DanbiMaterialManager
    {
        public enum EDanbiMaterialID
        {
            room,
            reflector,
            panorama_regular,
            panorama_panorama,
            panorama_4faces
        };

        public static Dictionary<EDanbiMaterialID, Material> m_preloadedSceneMaterialDict = new Dictionary<EDanbiMaterialID, Material>();

        public static void Init()
        {
            // string materialLocationRoot = "Scene Materials/";
            // we create a material so we don't need to load it.
            m_preloadedSceneMaterialDict.Add(EDanbiMaterialID.room, new Material(Shader.Find("danbi/Room")));
            m_preloadedSceneMaterialDict.Add(EDanbiMaterialID.reflector, new Material(Shader.Find("danbi/Panorama-Regular")));
            m_preloadedSceneMaterialDict.Add(EDanbiMaterialID.panorama_regular, new Material(Shader.Find("danbi/Panorama-Regular")));
            m_preloadedSceneMaterialDict.Add(EDanbiMaterialID.panorama_panorama, new Material(Shader.Find("danbi/Panorama-Panorama")));
            m_preloadedSceneMaterialDict.Add(EDanbiMaterialID.panorama_4faces, new Material(Shader.Find("danbi/Panorama-4faces")));
        }

        public static Material GetMaterial(EDanbiMaterialID id)
        {
            return m_preloadedSceneMaterialDict[id];
        }
    };
};