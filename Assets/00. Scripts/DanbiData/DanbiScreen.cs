using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    [System.Serializable]
    public class DanbiScreen : MonoBehaviour
    {
        [SerializeField, Readonly]
        Vector2Int ScreenResolution = new Vector2Int(1920, 1080);
        public Vector2Int screenResolution => ScreenResolution;

        void Awake()
        {
            DanbiUIProjectorInfoPanel.onResolutionWidthUpdate +=
                (int width) => ScreenResolution.x = width;

            DanbiUIProjectorInfoPanel.onResolutionHeightUpdate +=
                (int height) => ScreenResolution.y = height;

        }
    };
};
