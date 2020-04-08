using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour {

  public struct _MeshOpticalProperty {
    public Vector3 albedo;
    public Vector3 Specular;
    public float Smoothness;
    public Vector3 emission;
  }

  [System.Serializable]
  public struct _In_MeshOpticalProperty {
    public Text[] albedo;
    public Text[] Specular;
    public Text Smoothness;
    public Text[] emission;
  }


  [Header("RayTracingObject")]
  #region RayTracingObject
  public Text In_name;
  string name;

  [SerializeField]
  public _In_MeshOpticalProperty In_MeshOpticalProperty;
  _MeshOpticalProperty MeshOpticalProperty;
  #endregion

  [Header("Triangular")]
  #region TriangularConeMirrorObject
  public Text In_TriangularConeName;
  string _name;

  [Range(0, 1)]
  int mirrortype;
  public Slider In_Mirrortype;

  [SerializeField]
  public _In_MeshOpticalProperty TriangularCone_In_MeshOpticalProperty;
  _MeshOpticalProperty TriangularCone_MeshOpticalProperty;
  #endregion

  [Header("RayTracerMonitor")]
  #region RayTracerMonitor
  Texture2D SkyboxTexture;
  Texture2D ProjectedTexture;
  RenderTexture MainScreenRT;


  uint MaxNumOfBounce;

  public Text In_ScreenWidth;
  float ScreenWidth;
  public Text In_ScreenHeight;
  float ScreenHeight;
  #endregion

  [Header("Kinect")]
  #region Kinect

  public Text InDistance;
  float Distance;
  public Text InRange;
  float Range;
  public Text InRotateSpeed;
  float RotateSpeed;


  public Toggle InChange;
  bool ChangeImage;
  public Toggle InRotate;
  bool DoRotate;

  public Texture2D Images;

  #endregion

  [Header("Images")]
  public Text[] ImageName;
  public Texture2D[] LoadedImage;
  float tm;

  File_Manager m_FileManager;
  public static UI_Manager m_UI_Manager;
  [SerializeField]
  GameObject[] UIs;
  [SerializeField]
  Button ActiveButton;
  // Start is called before the first frame update

  private void Awake() {
    m_UI_Manager = this;
  }
  void Start() {
    m_FileManager = File_Manager.m_FileManager;
  }

  public void ChangeValue() {
    InputRayTraceMonitor();
    InputTriangularConeMirrorObject();
    InputRayTracingObject();
    InputKinect();
    BindImage();
  }

  void InputRayTraceMonitor() {
    float.TryParse(In_ScreenWidth.text, out ScreenWidth);
    float.TryParse(In_ScreenHeight.text, out ScreenHeight);
  }

  void InputTriangularConeMirrorObject() {
    //if (In_Mirrortype.value > 0) In_Mirrortype.value = 1;
    //else In_Mirrortype.value = 0;

    mirrortype = (int)In_Mirrortype.value;
    _name = In_TriangularConeName.text;
    equal(out TriangularCone_MeshOpticalProperty, TriangularCone_In_MeshOpticalProperty);
  }

  void InputRayTracingObject() {
    name = In_name.text;
    equal(out MeshOpticalProperty, In_MeshOpticalProperty);
  }

  void InputKinect() {
    float.TryParse(InDistance.text, out Distance);
    float.TryParse(InRange.text, out Range);
    float.TryParse(InRotateSpeed.text, out RotateSpeed);

    ChangeImage = InChange.isOn;
    DoRotate = InChange.isOn;
  }

  Vector3 ConvertToVector3(string Value, string Value2, string Value3) {
    float.TryParse(Value, out float x);
    float.TryParse(Value2, out float y);
    float.TryParse(Value3, out float z);
    return new Vector3(x, y, z);
  }

  _MeshOpticalProperty equal(out _MeshOpticalProperty Value1, _In_MeshOpticalProperty Value2) {
    float.TryParse(Value2.Smoothness.text, out float smooth);
    Value1.albedo = ConvertToVector3(Value2.albedo[0].text, Value2.albedo[1].text, Value2.albedo[2].text);
    Value1.emission = ConvertToVector3(Value2.emission[0].text, Value2.emission[1].text, Value2.emission[2].text);
    Value1.Smoothness = smooth;
    Value1.Specular = ConvertToVector3(Value2.Specular[0].text, Value2.Specular[1].text, Value2.Specular[2].text);

    return Value1;
  }

  void BindImage() {
    if (m_FileManager.ImagesName.Count == 0) {
      LoadedImage = null;
    }
    else {
      LoadedImage = new Texture2D[m_FileManager.ImagesName.Count];
    }

    for (int i = 0; i < ImageName.Length; i++) {
      if (i < m_FileManager.ImagesName.Count) {
        ImageName[i].text = m_FileManager.ImagesName[i];
        byte[] byteTexture = System.IO.File.ReadAllBytes(m_FileManager.FilePath + "/" + m_FileManager.ImagesName[i]);
        LoadedImage[i] = new Texture2D(0, 0);
        LoadedImage[i].LoadImage(byteTexture);

      }
      else {
        ImageName[i].text = "이미지";
      }

    }
  }


  #region Kinect Community
  public float Get_Kinect_Distance() {
    InputKinect();
    return Distance;
  }
  public void Set_Kinect_Distance(float _Dis) {
    Distance = _Dis;
    InDistance.text = Distance.ToString();
  }

  public float Get_Kinect_Range() {
    InputKinect();
    return Range;
  }
  public void Set_Kinect_Range(float _Range) {
    Range = _Range;
    InRange.text = Range.ToString();
  }

  public float Get_Kinect_RotateSpeed() {
    InputKinect();
    return RotateSpeed;
  }
  public void Set_Kinect_RotateSpeed(float _RotateSpeed) {
    RotateSpeed = _RotateSpeed;
    InRotateSpeed.text = RotateSpeed.ToString();
  }

  #endregion

  #region RayTraceMonitor Community
  public float Get_RayTraceMonitor_ScreenWidth() { return ScreenWidth; }
  public void Set_RayTraceMonitor_ScreenWidth(float Width) { ScreenWidth = Width; }

  public float Get_RayTraceMonitor_ScreenHeight() { return ScreenHeight; }
  public void Set_RayTraceMonitor_ScreenHeight(float Height) { ScreenHeight = Height; }

  #endregion

  #region TriangularConeMirrorObject Community

  public int Get_MirrorTYpe() { return mirrortype; }
  public void Set_MirrorType(int type) { mirrortype = type; }

  public _MeshOpticalProperty Get_TriangularConeMirrorObject_MeshOpticalProperty() { return TriangularCone_MeshOpticalProperty; }
  public void Set_TriangularConeMirrorObejct_MeshOpticalProperty(_MeshOpticalProperty _Mesh) { TriangularCone_MeshOpticalProperty = _Mesh; }

  public string Get_TriangularConeMirrorObject_Name() { return _name; }
  public void Set_TriangularConeMirrorObejct_Name(string n) { _name = n; }
  #endregion

  #region RayTracingObject Community

  public string Get_RayTracingObject_Name() { return name; }
  public void Set_RayTracingObject_Name(string n) { name = n; }

  public _MeshOpticalProperty Get_RayTracingObject_MeshOpticalProperty() { return MeshOpticalProperty; }
  public void Set_RayTracingObject_MeshOpticalProperty(_MeshOpticalProperty _Mesh) { MeshOpticalProperty = _Mesh; }

  #endregion

  public void HideUI() {
    for (int i = 0; i < UIs.Length; i++) {
      UIs[i].SetActive(false);
    }
    ActiveButton.gameObject.SetActive(true);
  }

  public void OnUIs() {
    for (int i = 0; i < UIs.Length; i++) {
      UIs[i].SetActive(true);
    }
    ActiveButton.gameObject.SetActive(false);
  }
};