[System.Serializable]
public class DanbiProjectorAttr {
  public float Width = 50.0f / 2.0f;
  public float Height = 50.0f;
  public float Depth = 50.0f;
  public float DistanceBetweenProjector = 5.0f;

  public float W { get { return Width; } set { Width = value; } }
  public float H { get { return Height; } set { Height = value; } }
  public float D { get { return Depth; } set { Depth = value; } }
  public float Dist { get { return DistanceBetweenProjector; } set { DistanceBetweenProjector = value; } }
  public UnityEngine.Vector3 BottomMiddle { get; set; }
};