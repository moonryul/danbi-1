[System.Serializable]
public class DanbiSpaceAttr {
  public float Width = 875.0f / 2.0f;
  public float Height = 260.0f;
  public float Depth = 875.0f / 2.0f;

  public float W { get { return Width; } set { Width = value; } }
  public float H { get { return Height; } set { Height = value; } }
  public float D { get { return Depth; } set { Depth = value; } }
  public UnityEngine.Vector3 TopMiddle { get; set; }
};