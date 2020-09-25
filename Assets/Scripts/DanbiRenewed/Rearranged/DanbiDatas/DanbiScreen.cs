using UnityEngine;

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
            TargetScreenAspect = screenPanel.aspectRatio;

            if (!string.IsNullOrEmpty(screenPanel.resolution))
            {
                var res = screenPanel.resolution.Split('x');
                ScreenResolution.x = int.Parse(res[0]);
                ScreenResolution.y = int.Parse(res[1]);
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
