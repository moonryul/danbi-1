using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public static class DanbiTextureManager
    {
        public enum EDanbiTextureID
        {
            lab509_360 = 0,
            calib,
            lib360_1,
            lib360_2,
            lib360_3,
            rainbow,
            sinchon360_1,
            sinchon360_2,
            street360_1,
            street360_2,
            univ360,
            vertical,
            zigzag_thick,
            zigzag_thin
        };

        public static Dictionary<EDanbiTextureID, Texture2D> m_preloadedSceneTexturesDict = new Dictionary<EDanbiTextureID, Texture2D>();
        
        public static void Init()
        {
            const string textureLocationRoot = "Scene Textures/";
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.lab509_360, Resources.Load<Texture2D>($"{textureLocationRoot}509 lab 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.calib, Resources.Load<Texture2D>($"{textureLocationRoot}calib 1k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.lib360_1, Resources.Load<Texture2D>($"{textureLocationRoot}library 360 1 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.lib360_2, Resources.Load<Texture2D>($"{textureLocationRoot}library 360 2 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.lib360_3, Resources.Load<Texture2D>($"{textureLocationRoot}library 360 3 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.rainbow, Resources.Load<Texture2D>($"{textureLocationRoot}rainbow 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.sinchon360_1, Resources.Load<Texture2D>($"{textureLocationRoot}sinchon 360 1 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.sinchon360_2, Resources.Load<Texture2D>($"{textureLocationRoot}sinchon 360 2 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.street360_1, Resources.Load<Texture2D>($"{textureLocationRoot}street 360 1 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.street360_2, Resources.Load<Texture2D>($"{textureLocationRoot}street 360 2 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.univ360, Resources.Load<Texture2D>($"{textureLocationRoot}univ entrance 360 8k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.vertical, Resources.Load<Texture2D>($"{textureLocationRoot}vertical 4k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.zigzag_thick, Resources.Load<Texture2D>($"{textureLocationRoot}zigzag thick 4k"));
            m_preloadedSceneTexturesDict.Add(EDanbiTextureID.zigzag_thin, Resources.Load<Texture2D>($"{textureLocationRoot}zigzag thin 8k"));
        }

        public static Texture2D GetTexture(EDanbiTextureID id)
        {
            return m_preloadedSceneTexturesDict[id];
        }
    };
};