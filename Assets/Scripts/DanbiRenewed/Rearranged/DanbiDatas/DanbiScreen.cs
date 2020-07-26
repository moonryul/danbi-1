using UnityEngine;

namespace Danbi {
  public class DanbiScreen : MonoBehaviour {
    [Header("16:9 or 16:10")]
    public EDanbiScreenAspects TargetScreenAspect = EDanbiScreenAspects.E_16_9;

    [Header("2K(2560 x 1440), 4K(3840 x 2160) or 8K(7680 x 4320)")]
    public EDanbiScreenResolutions TargetScreenResolution = EDanbiScreenResolutions.E_4K;

    // TODO: Change this variable with Readonly Attribute.
    [Header("Read-only || Current Resolution of the target distorted image")]
    public Vector2Int ScreenResolutions;

    [Header("Resolution Scaler")]
    public int ResolutionScaler = 0;
  };
};
