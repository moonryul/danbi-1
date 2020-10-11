using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Danbi
{
    public static class DanbiUIControl
    {
        public static void GenerateImage()
        {
            DanbiControl.Call_OnGenerateImage?.Invoke();
        }

        public static void GenerateVideo()
        {
            // DanbiControl.Call_OnGenerateVideo?.Invoke();
        }

        public static void SaveImage()
        {
            // TODO: 윈도우즈 익스플로러를 연결하여 사용해야함.
            DanbiControl.Call_OnSaveImage?.Invoke();
        }

        public static void SaveVideo()
        {
            // DanbiControl.Call_OnSaveVideo?.Invoke();
        }
    };

}; // namespace Danbi.
