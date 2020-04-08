using UnityEngine;
public static class DanbiScreenHelper {
  /// <summary>
  /// Calculate the actual screen resolution by the screen aspects and the target resolutions.
  /// </summary>
  /// <param name="eScreenAspects"></param>
  /// <param name="eScreenResolution"></param>
  /// <returns></returns>
  public static Vector2Int GetScreenResolution(EDanbiScreenAspects eScreenAspects, 
                                            EDanbiScreenResolutions eScreenResolution) {
    var result = default(Vector2Int);
    switch (eScreenResolution) {
      case EDanbiScreenResolutions.E_2K:
      result = new Vector2Int(2560, 2560);
      break;

      case EDanbiScreenResolutions.E_4K:
      result = new Vector2Int(3840, 3840);
      break;

      case EDanbiScreenResolutions.E_8K:
      result = new Vector2Int(7680, 7680);
      break;
    }

    switch (eScreenAspects) {
      case EDanbiScreenAspects.E_16_9:
      result.y = Mathf.FloorToInt(result.y * 9 / 16);        
      break;

      case EDanbiScreenAspects.E_16_10:
      result.y = Mathf.FloorToInt(result.y * 10 / 16);
      break;
    }
    return result;
  }
};