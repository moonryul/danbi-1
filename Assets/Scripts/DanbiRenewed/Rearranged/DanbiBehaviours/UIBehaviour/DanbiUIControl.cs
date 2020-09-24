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
            DanbiControl.Call_OnGenerateVideo?.Invoke();
        }

        public static void OnSaveImage(string call)
        {
            // TODO: 윈도우즈 익스플로러를 연결하여 사용해야함.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                DanbiControl.Call_OnSaveImage?.Invoke();
            }
        }

        public static void OnSaveVideo(string call)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                DanbiControl.Call_OnSaveVideo?.Invoke();
            }
        }
    };

}; // namespace Danbi.
