using UnityEngine;

namespace Danbi {
  [System.Serializable]
  public class DanbiScreen : MonoBehaviour {
    [SerializeField, Header("16:9 or 16:10")]
    EDanbiScreenAspects TargetScreenAspect = EDanbiScreenAspects.E_16_9;
    public EDanbiScreenAspects targetScreenAspect { get => TargetScreenAspect; set => TargetScreenAspect = value; }

    [SerializeField, Header("2K(2560 x 1440), 4K(3840 x 2160) or 8K(7680 x 4320)")]
    EDanbiScreenResolutions TargetScreenResolution = EDanbiScreenResolutions.E_4K;
    public EDanbiScreenResolutions targetScreenResolution { get => TargetScreenResolution; set => TargetScreenResolution = value; }

    // TODO: Change this variable with Readonly Attribute.
    [SerializeField, Readonly, Header("Current Resolution of the target distorted image")]
    Vector2Int ScreenResolution;
    public Vector2Int screenResolution { get => ScreenResolution; set => ScreenResolution = value; }

    [SerializeField, Header("Resolution Scaler")]
    int ResolutionScaler = 0;
    public int resolutionScaler { get => ResolutionScaler; set => ResolutionScaler = value; }

    void Start() {
      // it exists for the script enabling so far.  
    }
    void OnValidate() {
      ScreenResolution = DanbiScreenHelper.GetScreenResolution(eScreenAspects: TargetScreenAspect,
                                                         eScreenResolution: TargetScreenResolution);
      ScreenResolution *= ResolutionScaler;
    }
  };
};
