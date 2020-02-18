using UnityEngine;
#if RAY_VALIDATOR_ON
public sealed class RayValidatorLine {
  public enum ePosition : uint {
    START = 0, END = 1
  };

  public static readonly float sMultiplier = 1.0f;
  public LineRenderer Line;
  public int idx;
  public ePosition LinePos;
  public Material Mat;

  public void SetLineColor(eRayValidatorLineColor col) {
    switch (col) {
      case eRayValidatorLineColor.RED:
      Line.material.EnableKeyword("RED_ON");
      Line.material.DisableKeyword("BLUE_ON");
      Line.material.DisableKeyword("BLACK_ON");
      Line.material.DisableKeyword("PURPLE_ON");
      Line.material.DisableKeyword("GREEN_ON");
      Line.material.DisableKeyword("YELLOW_ON");
      break;

      case eRayValidatorLineColor.BLUE:
      Line.material.DisableKeyword("RED_ON");
      Line.material.EnableKeyword("BLUE_ON");
      Line.material.DisableKeyword("BLACK_ON");
      Line.material.DisableKeyword("PURPLE_ON");
      Line.material.DisableKeyword("GREEN_ON");
      Line.material.DisableKeyword("YELLOW_ON");
      break;

      case eRayValidatorLineColor.BLACK:
      Line.material.DisableKeyword("RED_ON");
      Line.material.DisableKeyword("BLUE_ON");
      Line.material.EnableKeyword("BLACK_ON");
      Line.material.DisableKeyword("PURPLE_ON");
      Line.material.DisableKeyword("GREEN_ON");
      Line.material.DisableKeyword("YELLOW_ON");
      break;

      case eRayValidatorLineColor.PURPLE:
      Line.material.DisableKeyword("RED_ON");
      Line.material.DisableKeyword("BLUE_ON");
      Line.material.DisableKeyword("BLACK_ON");
      Line.material.EnableKeyword("PURPLE_ON");
      Line.material.DisableKeyword("GREEN_ON");
      Line.material.DisableKeyword("YELLOW_ON");
      break;

      case eRayValidatorLineColor.GREEN:
      Line.material.DisableKeyword("RED_ON");
      Line.material.DisableKeyword("BLUE_ON");
      Line.material.DisableKeyword("BLACK_ON");
      Line.material.DisableKeyword("PURPLE_ON");
      Line.material.EnableKeyword("GREEN_ON");
      Line.material.DisableKeyword("YELLOW_ON");
      break;

      case eRayValidatorLineColor.YELLOW:
      Line.material.DisableKeyword("RED_ON");
      Line.material.DisableKeyword("BLUE_ON");
      Line.material.DisableKeyword("BLACK_ON");
      Line.material.DisableKeyword("PURPLE_ON");
      Line.material.DisableKeyword("GREEN_ON");
      Line.material.EnableKeyword("YELLOW_ON");
      break;
    }
  }
};
#endif