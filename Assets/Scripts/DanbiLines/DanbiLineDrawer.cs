using LinesList_t = System.Collections.Generic.Dictionary<int, DanbiLine>;

public static class DanbiLineDrawer {
  public static LinesList_t Lines;
  public static int sLINE_COUNTER;
  public static bool isPenDown { get; set; }
  public static bool penUp { set { isPenDown = false; } }
  public static bool penDown { set { isPenDown = true; } }

  static DanbiLineDrawer() {
    sLINE_COUNTER = 0;
  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  public static void Draw(float x, float y) {
    if (!isPenDown)
      return;


  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="z"></param>
  public static void Draw(float x, float y, float z) {
    if (!isPenDown)
      return;


  }
  /// <summary>
  /// 
  /// </summary>
  /// <param name="color"></param>
  public static void SetColor(UnityEngine.Color color) {
    Lines[sLINE_COUNTER].line.startColor = color;
    Lines[sLINE_COUNTER].line.endColor = color;
  }  
};
