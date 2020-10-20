using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    [System.Serializable]
    public class DanbiScreen : MonoBehaviour
    {
        #region Exposed        
        [SerializeField, Readonly]
        string TargetScreenAspect = "16 : 9";

        [SerializeField, Readonly]
        Vector2Int ScreenResolution = new Vector2Int(3840, 2160);

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
            if (control is DanbiUIProjectorScreenPanelControl)
            {
                var screenPanel = control as DanbiUIProjectorScreenPanelControl;

                (int width, int height) = (screenPanel.aspectRatioWidth == 0 ? 16 : screenPanel.aspectRatioWidth,
                                           screenPanel.aspectRatioHeight == 0 ? 9 : screenPanel.aspectRatioHeight);
                TargetScreenAspect = $"{width} : {height}";

                if (screenPanel.resolutionWidth != 0.0f || screenPanel.resolutionHeight != 0.0f)
                {
                    ScreenResolution.x = screenPanel.resolutionWidth;
                    ScreenResolution.y = screenPanel.resolutionHeight;
                }
                else
                {
                    ScreenResolution.x = 3840;
                    ScreenResolution.y = 2160;
                }
            }
        }
    };
};
