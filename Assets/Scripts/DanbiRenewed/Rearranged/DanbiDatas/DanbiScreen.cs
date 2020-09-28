﻿using UnityEngine;

namespace Danbi
{
    [System.Serializable]
    public class DanbiScreen : MonoBehaviour
    {
        #region Exposed        
        [SerializeField, Readonly]
        string TargetScreenAspect = "16 : 9";

        [SerializeField, Readonly]
        Vector2Int ScreenResolution = new Vector2Int();

        #endregion Exposed


        #region Internal
        public Vector2Int screenResolution => ScreenResolution;

        #endregion Internal    

        void Start()
        {
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
        }

        void OnDisable()
        {
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (!(control is DanbiUIProjectorScreenPanelControl)) { return; }

            var screenPanel = control as DanbiUIProjectorScreenPanelControl;

            TargetScreenAspect = $"{screenPanel.aspectRatioWidth} : {screenPanel.aspectRatioHeight}";

            if (screenPanel.resolutionWidth != 0.0f || screenPanel.resolutionHeight != 0.0f)
            {                
                ScreenResolution.x = (int)screenPanel.resolutionWidth;
                ScreenResolution.y = (int)screenPanel.resolutionHeight;
            }
        }

        // /// <summary>
        // /// Calculate the actual screen resolution by the screen aspects and the target resolutions.
        // /// </summary>
        // /// <param name="eScreenAspects"></param>
        // /// <param name="eScreenResolution"></param>
        // /// <returns></returns>
        // public static Vector2Int GetScreenResolution(EDanbiScreenAspects eScreenAspects,
        //                                              EDanbiScreenResolutions eScreenResolution)
        // {
        //     var result = default(Vector2Int);
        //     switch (eScreenResolution)
        //     {
        //         case EDanbiScreenResolutions.E_1K:
        //             result = new Vector2Int(1920, 1920);
        //             break;

        //         case EDanbiScreenResolutions.E_2K:
        //             result = new Vector2Int(2560, 2560);
        //             break;

        //         case EDanbiScreenResolutions.E_4K:
        //             result = new Vector2Int(3840, 3840);
        //             break;

        //         case EDanbiScreenResolutions.E_8K:
        //             result = new Vector2Int(7680, 7680);
        //             break;
        //     }

        //     switch (eScreenAspects)
        //     {
        //         case EDanbiScreenAspects.E_16_9:
        //             result.y = Mathf.FloorToInt(result.y * 9 / 16);
        //             break;

        //         case EDanbiScreenAspects.E_16_10:
        //             result.y = Mathf.FloorToInt(result.y * 10 / 16);
        //             break;
        //     }
        //     return result;
        // }
    };
};
