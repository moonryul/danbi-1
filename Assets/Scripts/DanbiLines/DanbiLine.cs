using UnityEngine;
public class DanbiLine : MonoBehaviour {
  public enum ePosition {
    START = 0, END = 1
  };

  public enum eColor {
    RED, BLUE, BLACK, PURPLE, GREEN, YELLOW
  };

  public static readonly float sMutliplier = 1f;
  public UnityEngine.LineRenderer line;
  public int idx;
  public ePosition ePos;
  public static int sLINE_NUM = 57;
  public static int sLINE_COUNTER = 0;
  public Material Mat;

  public static void SetLine(ref DanbiLine[] lines, DanbiLine.ePosition ePos, float x, float y) {
    lines[DanbiLine.sLINE_COUNTER++].line.SetPosition((int)ePos,
                                                     new UnityEngine.Vector3(x, y) * sMutliplier);
  }

  public static void SetLine(ref DanbiLine[] lines, DanbiLine.ePosition ePos, float x, float y, float z) {
    lines[DanbiLine.sLINE_COUNTER++].line.SetPosition((int)ePos,
                                                     new UnityEngine.Vector3(x, y, z) * sMutliplier);
  }

  public static void SetLine(ref DanbiLine[] lines, int idx, DanbiLine.ePosition ePos, float x, float y) {
    lines[UnityEngine.Mathf.Max(0, idx)].line.SetPosition((int)ePos,
                                                          new UnityEngine.Vector3(x, y) * sMutliplier);
  }

  public static void SetLine(ref DanbiLine[] lines, int idx, DanbiLine.ePosition ePos, float x, float y, float z) {
    lines[UnityEngine.Mathf.Max(0, idx)].line.SetPosition((int)ePos,
                                                     new UnityEngine.Vector3(x, y, z) * sMutliplier);
  }

  public void SetLine(DanbiLine.ePosition ePos, float x, float y) {
    line.SetPosition((int)ePos,
                     new UnityEngine.Vector3(x, y) * sMutliplier);
  }

  public void SetLine(DanbiLine.ePosition ePos, float x, float y, float z) {
    line.SetPosition((int)ePos,
                     new UnityEngine.Vector3(x, y, z) * sMutliplier);
  }

  public void SetLine(DanbiLine.ePosition ePos, Vector3 vec) {
    line.SetPosition((int)ePos, vec);
  }

  public void SetLineColor(eColor col) {
    switch (col) {
      case eColor.RED:
      line.material.EnableKeyword("RED_ON");
      line.material.DisableKeyword("BLUE_ON");
      line.material.DisableKeyword("BLACK_ON");
      line.material.DisableKeyword("PURPLE_ON");
      line.material.DisableKeyword("GREEN_ON");
      line.material.DisableKeyword("YELLOW_ON");
      break;

      case eColor.BLUE:
      line.material.DisableKeyword("RED_ON");
      line.material.EnableKeyword("BLUE_ON");
      line.material.DisableKeyword("BLACK_ON");
      line.material.DisableKeyword("PURPLE_ON");
      line.material.DisableKeyword("GREEN_ON");
      line.material.DisableKeyword("YELLOW_ON");
      break;

      case eColor.BLACK:
      line.material.DisableKeyword("RED_ON");
      line.material.DisableKeyword("BLUE_ON");
      line.material.EnableKeyword("BLACK_ON");
      line.material.DisableKeyword("PURPLE_ON");
      line.material.DisableKeyword("GREEN_ON");
      line.material.DisableKeyword("YELLOW_ON");
      break;

      case eColor.PURPLE:
      line.material.DisableKeyword("RED_ON");
      line.material.DisableKeyword("BLUE_ON");
      line.material.DisableKeyword("BLACK_ON");
      line.material.EnableKeyword("PURPLE_ON");
      line.material.DisableKeyword("GREEN_ON");
      line.material.DisableKeyword("YELLOW_ON");
      break;

      case eColor.GREEN:
      line.material.DisableKeyword("RED_ON");
      line.material.DisableKeyword("BLUE_ON");
      line.material.DisableKeyword("BLACK_ON");
      line.material.DisableKeyword("PURPLE_ON");
      line.material.EnableKeyword("GREEN_ON");
      line.material.DisableKeyword("YELLOW_ON");
      break;

      case eColor.YELLOW:
      line.material.DisableKeyword("RED_ON");
      line.material.DisableKeyword("BLUE_ON");
      line.material.DisableKeyword("BLACK_ON");
      line.material.DisableKeyword("PURPLE_ON");
      line.material.DisableKeyword("GREEN_ON");
      line.material.EnableKeyword("YELLOW_ON");
      break;
    }
  }

};