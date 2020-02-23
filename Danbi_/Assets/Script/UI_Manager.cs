using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{

    public struct _MeshOpticalProperty
    {
        public Vector3 albedo;
        public Vector3 Specular;
        public float Smoothness;
        public Vector3 emission;
    }

    [System.Serializable]
    public struct _In_MeshOpticalProperty
    {
        public Text[] albedo;
        public Text[] Specular;
        public Text Smoothness;
        public Text[] emission;
    }


    [Header("RayTracingObject")]
    #region RayTracingObject
    public Text                    In_name;
	string                  name;

    [SerializeField]
    public _In_MeshOpticalProperty In_MeshOpticalProperty;
    _MeshOpticalProperty    MeshOpticalProperty;
    #endregion


    [Header("Triangular")]
    #region TriangularConeMirrorObject
    public Text                    In_TriangularConeName;
    string                  _name;
     
    [Range(0, 1)]
    int                     mirrortype;
    public Slider                  In_Mirrortype;

    [SerializeField]
    public _In_MeshOpticalProperty TriangularCone_In_MeshOpticalProperty;
    _MeshOpticalProperty    TriangularCone_MeshOpticalProperty;
    #endregion


    [Header("RayTracerMonitor")]
    #region RayTracerMonitor
    Texture2D           SkyboxTexture;
    Texture2D           ProjectedTexture;
    RenderTexture       MainScreenRT;
     
     
    uint                MaxNumOfBounce;
     
    public Text                In_ScreenWidth;
    float               ScreenWidth;
    public Text                In_ScreenHeight;
    float               ScreenHeight;
    #endregion


    [Header("Kinect")]
    #region Kinect

    public Text    InDistance;
    float   DIstance;
    public Text    InRange;
    float   Range;
    public Text    InRotateSpeed;
    float   RotateSpeed;
     
     
    public Toggle InChange;
    bool    ChangeImage;
    public Toggle InRotate;
    bool    DoRotate;
     
    public Texture2D Images;

    #endregion


    [Header("Images")]
    public Text[] ImageName;
    public Texture2D[] LoadedImage;
    float tm;

    File_Manager m_FileManager;

	// Start is called before the first frame update
	void Start()
    {
        m_FileManager = File_Manager.m_FileManager;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ChangeValue()
    {

        InputRayTraceMonitor();
        InputTriangularConeMirrorObject();
        InputRayTracingObject();
        InputKinect();
        BindImage();

    }


    void InputRayTraceMonitor()
    {
        float.TryParse(In_ScreenWidth.text, out ScreenWidth);
        float.TryParse(In_ScreenHeight.text, out ScreenHeight);
    }
    void InputTriangularConeMirrorObject()
    {
        //if (In_Mirrortype.value > 0) In_Mirrortype.value = 1;
        //else In_Mirrortype.value = 0;

        mirrortype = (int)In_Mirrortype.value;

        _name = In_TriangularConeName.text;

        equal(out TriangularCone_MeshOpticalProperty, TriangularCone_In_MeshOpticalProperty);
    }
    void InputRayTracingObject()
    {
        name = In_name.text;
        equal(out MeshOpticalProperty, In_MeshOpticalProperty);
    }
    void InputKinect()
    {
        float.TryParse(InDistance.text, out DIstance);
        float.TryParse(InRange.text, out Range);
        float.TryParse(InRotateSpeed.text, out RotateSpeed);

        ChangeImage = InChange.isOn;
        DoRotate = InChange.isOn;
    }

    Vector3 ConvertToVector3(string Value,string Value2,string Value3)
    {
        float x, y, z;
        float.TryParse(Value, out x);
        float.TryParse(Value2, out y);
        float.TryParse(Value3, out z);
        return new Vector3(x, y, z);
    }

    _MeshOpticalProperty equal(out _MeshOpticalProperty Value1,_In_MeshOpticalProperty Value2) 
    {
        float smooth;
        float.TryParse(Value2.Smoothness.text, out smooth);
        Value1.albedo = ConvertToVector3(Value2.albedo[0].text, Value2.albedo[1].text, Value2.albedo[2].text);
        Value1.emission = ConvertToVector3(Value2.emission[0].text, Value2.emission[1].text, Value2.emission[2].text);
        Value1.Smoothness = smooth;
        Value1.Specular = ConvertToVector3(Value2.Specular[0].text, Value2.Specular[1].text, Value2.Specular[2].text);

        return Value1;
    }

    void BindImage()
    {
        if (m_FileManager.ImagesName.Count == 0)
            LoadedImage = null;

        else
            LoadedImage = new Texture2D[m_FileManager.ImagesName.Count];

        for (int i = 0; i < ImageName.Length; i++)
        {
            if (i < m_FileManager.ImagesName.Count)
            {
                ImageName[i].text = m_FileManager.ImagesName[i];
                byte[] byteTexture = System.IO.File.ReadAllBytes(m_FileManager.FilePath + "/" + m_FileManager.ImagesName[i]);
                LoadedImage[i] = new Texture2D(0, 0);
                LoadedImage[i].LoadImage(byteTexture);
                
            }
            else
            {
                ImageName[i].text = "이미지";
            }

        }
    }

}
