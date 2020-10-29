using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Danbi
{
    public static class DanbiUIControl
    {
        public static void GenerateImage(Texture2D overrdingTex = default)
        {
            DanbiControl.Call_OnGenerateImage?.Invoke(overrdingTex);
        }

        public static void GenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            DanbiControl.Call_OnGenerateVideo?.Invoke(progressDisplay, statusDisplay);
        }

        public static void SaveImage()
        {
            DanbiControl.Call_OnSaveImage?.Invoke();
        }

        public static void SaveVideo(string ffmpegExecutableLocation)
        {
            DanbiControl.Call_OnSaveVideo?.Invoke(ffmpegExecutableLocation);
        }
    };

}; // namespace Danbi.
