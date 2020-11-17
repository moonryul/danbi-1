using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    [System.Serializable]
    public class DanbiScreen : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        string TargetScreenAspect = "16 : 9";

        [SerializeField, Readonly]
        Vector2Int ScreenResolution = new Vector2Int(1920, 1080);
        public Vector2Int screenResolution => ScreenResolution;

        void Awake()
        {
            DanbiUISync.onPanelUpdate += OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIProjectorInfoPanelControl)
            {
                var screenPanel = control as DanbiUIProjectorInfoPanelControl;

                (int width, int height) = (screenPanel.m_aspectRatioWidth == 0 ? 16 : screenPanel.m_aspectRatioWidth,
                                           screenPanel.m_aspectRatioHeight == 0 ? 9 : screenPanel.m_aspectRatioHeight);
                TargetScreenAspect = $"{width} : {height}";

                if (screenPanel.m_resolutionWidth != 0.0f || screenPanel.m_resolutionHeight != 0.0f)
                {
                    ScreenResolution.x = screenPanel.m_resolutionWidth;
                    ScreenResolution.y = screenPanel.m_resolutionHeight;
                }
                else
                {
                    ScreenResolution.x = 1920;
                    ScreenResolution.y = 1080;
                }
            }
        }
    };
};
