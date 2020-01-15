[System.Serializable]
public class DanbiPyramidAttr {
  public float Width = 8.0f;
  public float Height = 8.4f;
  public float Depth = 8.0f;

  public float W { get { return Width; } set { Width = value; } }
  public float H { get { return Height; } set { Height = value; } }
  public float D { get { return Depth; } set { Depth = value; } }
  public UnityEngine.Vector3 Origin { get; set; }
  public UnityEngine.Vector3 Apex { get; set; }
  
  public UnityEngine.Vector3 Bottom1 { get; set; }
  public UnityEngine.Vector3 Bottom2 { get; set; }
  public UnityEngine.Vector3 Bottom3 { get; set; }
  public UnityEngine.Vector3 Bottom4 { get; set; }
};
