using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Danbi
{
    public static class DanbiUIControl
    {
        public static void GenerateImage(Texture2D overrdingTex = default) => DanbiControl.Call_OnGenerateImage?.Invoke(overrdingTex);

        public static void GenerateVideo() => DanbiControl.Call_OnGenerateVideo?.Invoke();

        public static void SaveImage() => DanbiControl.Call_OnSaveImage?.Invoke();

        public static void SaveVideo() => DanbiControl.Call_OnSaveVideo?.Invoke();
    };

}; // namespace Danbi.
