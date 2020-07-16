//using UnityEngine;

//using System;

//#if UNITY_EDITOR
//using UnityEditor;

//public enum EDanbiPrewarperType : uint {
//  None,
//  HalfSphereMirror_h6cm_Cube,
//  HalfSphereMirror_h7cm_Cube,
//  HalfSphereMirror_h8cm_Cube,
//  UFOSphereMirror_Cube,
//  SmallParaboloidMirror_Cube,
//  BigParaboloidMirror_Cube,
//  ConeMirror_h6cm_r8cm_Cube,
//  ConeMirror_h7cm_r8cm_Cube,
//  ConeMirror_h8cm_r8cm_Cube,
//  ConeMirror_h10cm_r8cm_Cube,
//  ConeMirror_h13cm_r8cm_Cube,
//  ConeMirror_h16cm_r8cm_Cube,
//  Video_HalfSphereMirror_hr8cm_Cube,
//  Video_HalfSphereMirror_h10cm_r8cm_Cube
//};

///// <summary>
///// 
///// </summary>
///// 
//public class DanbiController : EditorWindow {
//  /// <summary>
//  /// 
//  /// </summary>
//  public static Camera CurrentCam;
//  /// <summary>
//  /// 
//  /// </summary>
//  public static Texture2D TargetTex;
//  /// <summary>
//  /// 
//  /// </summary>
//  public static RayTracingMaster RTMaster;
//  /// <summary>
//  /// 
//  /// </summary>
//  public static GameObject[] Prewarpers;
//  /// <summary>
//  /// 
//  /// </summary>
//  public static DanbiController EditorWindowInstace;
//  /// <summary>
//  /// 
//  /// </summary>
//  public static Action<EDanbiPrewarperType> PrewarperActivatorAction;


//  /// <summary>
//  /// 
//  /// </summary>
//  static float UpdateCounter;
//  public static bool bWindowOpened { get; set; }

//  Vector2 ScrollPosition;

//  public delegate void TargetTextureChanged(Texture2D TargetTex);
//  public static TargetTextureChanged OnTargetTexChanged;

//  public static EDanbiPrewarperType CurrentPrewarperType { get; set; }

//  [MenuItem("Danbi/Open Controller")]
//  public static void OpenWindow() {
//    bWindowOpened = true;

//    EditorWindowInstace = GetWindow<DanbiController>();
//    EditorWindowInstace.Show();
//    // Set the title name of current window.
//    EditorWindowInstace.titleContent = new GUIContent(text: "Danbi Controller!");
//    PreparePrerequisites();
//  }

//  static DanbiController() {
//    UpdateCounter = 0.0f;
//  }

//  void OnDestroy() { bWindowOpened = false; }

//  void Update() {
//    UpdateCounter += Time.deltaTime;
//    UpdateCounter %= 60.0f;
//    int sec = (int)UpdateCounter;

//    if (sec >= 10.0f) {
//      UpdateCounter = 0.0f;
//      PreparePrerequisites();
//    }
//  }

//  void OnGUI() {
//    // Instance of current window must be alive.
//    if (ReferenceEquals(EditorWindowInstace, null)) {
//      EditorWindowInstace = GetWindow<DanbiController>();
//    }

//    /**
//     * Editor Window Scroll Begin
//     */
//    // TODO: Need to retrieve dynamic window sizes.
//    float screenXMax = EditorWindowInstace.position.xMax - 150.0f;
//    float screenYMax = EditorWindowInstace.position.yMax - 80.0f;
//    ScrollPosition = EditorGUILayout.BeginScrollView(scrollPosition: ScrollPosition,
//                                                    alwaysShowHorizontal: true,
//                                                    alwaysShowVertical: true,
//                                                    options: new GUILayoutOption[] { GUILayout.Width(screenXMax),
//                                                                                     GUILayout.Height(screenYMax) });

//    // Mark window opened so there's only one editor window instance at the same time.
//    bWindowOpened = true;

//    OnGUI_DisplayMainTitle();
//    OnGUI_ChoosePrewarper();
//    OnGUI_CameraPropControl();
//    OnGUI_MirrorPropControl();
//    //OnGUI_PanoramaPropControl();
//    OnGUI_RayTracerMasterControl();
//    //OnGUI_PrewarpedImageGenerator();

//    /**
//     * Editor Window Scroll Begin    
//     */
//    EditorGUILayout.EndScrollView();
//  }

//  /// <summary>
//  /// Push the Space into the editor window by time parameter
//  /// </summary>
//  /// <param name="time">Decide how many times to push space</param>
//  static void PushSpace(int time = 1) {
//    for (int i = 0; i < time; ++i) { EditorGUILayout.Space(); }
//  }

//  static void PreparePrerequisites() {
//    // 1. Find the RT Mesh Object Game Object to retrieve all the prewarper sets and Add all the Prewarpers sets.
//    var origin = GameObject.Find("-------RT Mesh Objects------").transform;
//    Prewarpers = new GameObject[origin.childCount];
//    int len = Prewarpers.Length;
//    for (int i = 0; i < len; ++i) {
//      Prewarpers[i] = origin.GetChild(i).gameObject;
//    }

//    // 2. Bind the Action as Lambda expression that compares to the current warper type.
//    DanbiFwdObjects.PrewarperActivatorAction = (comparer) => {
//      for (int i = 0; i < DanbiFwdObjects.Prewarpers.Length; ++i) {
//        if (DanbiFwdObjects.Prewarpers[i].GetComponent<DanbiPrewarperSet>().CurrentPrewarperType == comparer) {
//          DanbiFwdObjects.Prewarpers[i].SetActive(true);
//        } else {
//          DanbiFwdObjects.Prewarpers[i].SetActive(false);
//        }
//        // If you keep saving on the changes on the editor, you got to set dirty flag to let the editor tracks.
//        EditorUtility.SetDirty(DanbiFwdObjects.Prewarpers[i]);
//      }
//    };

//    // 3. Invoke the action initially.
//    PrewarperActivatorAction.Invoke(CurrentPrewarperType);
//    // Update all the linked references.
//    // Camera reference of the prewarper set.
//    //Cams = FindObjectsOfType<Camera>();
//    //FindObjectOfType<Camera>();

//    // 4. Bind the function for Texture Changed from the editor.
//    if (OnTargetTexChanged == null) {
//      OnTargetTexChanged += DanbiController.OnTargetTextureAssignedFromEditor;
//    }

//    // TODO: other linked references go here.
//    //
//  }

//  static void OnChoosePrewarper(EDanbiPrewarperType comparer) {
//    for (int i = 0; i < Prewarpers.Length; ++i) {
//      if (Prewarpers[i].GetComponent<DanbiPrewarperSet>().CurrentPrewarperType == comparer) {
//        Prewarpers[i].SetActive(true);
//        CurrentCam = Prewarpers[i].GetComponentInChildren<Camera>();
//      } else {
//        Prewarpers[i].SetActive(false);
//      }
//      // If you keep saving on the changes on the editor, you got to set dirty flag to let the editor tracks.
//      EditorUtility.SetDirty(Prewarpers[i]);
//    }
//  }

//  static void OnTargetTextureAssignedFromEditor(Texture2D newTargetTex) {
//    TargetTex = newTargetTex;
//  }

//  /// <summary>
//  /// TODO: 
//  /// </summary>
//  void OnGUI_DisplayMainTitle() {
//    EditorGUILayout.BeginVertical();
//    // 1. Declare GUIStyle for the main title.
//    var styleLabel = new GUIStyle(GUI.skin.label);
//    //styleForResoultionLabel.alignment = TextAnchor.MiddleCenter;
//    styleLabel.fontSize = 60;
//    styleLabel.alignment = TextAnchor.UpperCenter;
//    styleLabel.fixedHeight = 600;

//    DanbiController.PushSpace(3);
//    // 2. Draw LabelField.
//    EditorGUILayout.LabelField("Danbi Controller", styleLabel);

//    DanbiController.PushSpace(12);
//    EditorGUILayout.EndVertical();
//  }

//  /// <summary>
//  /// TODO:
//  /// </summary>
//  void OnGUI_ChoosePrewarper() {
//    EditorGUILayout.BeginVertical();
//    EditorGUI.BeginChangeCheck();
//    DanbiController.PushSpace(6);

//    // 1. Start to draw Enum Popup.
//    CurrentPrewarperType = (EDanbiPrewarperType)EditorGUILayout.EnumPopup("Prewarper Type ", CurrentPrewarperType);
//    DanbiController.PushSpace(6);

//    // 2. If there's dirty on the Enum Popup,
//    if (EditorGUI.EndChangeCheck()) {
//      // 3. Invoke the delegate that is linked to change the current prewarper.
//      PrewarperActivatorAction.Invoke(CurrentPrewarperType);
//    }
//    EditorGUILayout.EndVertical();
//  }

//  void OnGUI_CameraPropControl() {    
//    EditorGUILayout.BeginVertical();
//    EditorGUI.BeginChangeCheck();

//    CurrentCam.usePhysicalProperties = EditorGUILayout.BeginToggleGroup("Is Using Physical Camera?", CurrentCam.usePhysicalProperties);
//    EditorGUILayout.Space();

//    CurrentCam.focalLength = float.Parse(EditorGUILayout.TextField("Current Focal Length", CurrentCam.focalLength.ToString()));
//    EditorGUILayout.LabelField("Max Focal Length = 13,1 (height to Camera : 2.04513m)");
//    EditorGUILayout.LabelField("Min Focal Length = 11,1 (height to Camera : 2.04896m)");
//    EditorGUILayout.Space();

//    EditorGUILayout.TextField("Current Field Of View", CurrentCam.fieldOfView.ToString());
//    EditorGUILayout.LabelField("Calculated by Physical Camera Props");
//    EditorGUILayout.Space();

//    float x = 0.0f, y = 0.0f;
//    x = float.Parse(EditorGUILayout.TextField("Current Sensor Size X", CurrentCam.sensorSize.x.ToString()));
//    EditorGUILayout.LabelField("(16:10 일때 = 16,36976)");
//    EditorGUILayout.Space();

//    y = float.Parse(EditorGUILayout.TextField("Current Sensor Size Y", CurrentCam.sensorSize.y.ToString()));
//    EditorGUILayout.LabelField("(16:10 일떄 = 10,02311)");
//    EditorGUILayout.LabelField("(16:9 일때 = 9,020799)");
//    EditorGUILayout.Space();

//    EditorGUILayout.LabelField($"Current GateFit Mode is {CurrentCam.gateFit.ToString()}");
//    EditorGUILayout.EndToggleGroup();

//    if (EditorGUI.EndChangeCheck()) {
//      CurrentCam.sensorSize = new Vector2(x, y);
//      CurrentCam.gateFit = Camera.GateFitMode.None;

//        if (cam.focalLength == 11.1f) {
//          cam.transform.position = new Vector3(0.0f, 2.04896f, 0.0f);
//        } else if (cam.focalLength == 13.1f) {
//          cam.transform.position = new Vector3(0.0f, 2.04513f, 0.0f);
//        }
//        EditorUtility.SetDirty(cam);
//      }
//      EditorUtility.SetDirty(CurrentCam);
//    }

//    EditorGUILayout.EndVertical();
//  }

//  void OnGUI_MirrorPropControl() {
//    EditorGUILayout.BeginVertical();
//    DanbiController.PushSpace(3);



//    DanbiController.PushSpace(3);
//    EditorGUILayout.EndVertical();
//  }

//  void OnGUI_PanoramaPropControl() {
//    EditorGUILayout.BeginVertical();
//    DanbiController.PushSpace(3);



//    DanbiController.PushSpace(3);
//    EditorGUILayout.EndVertical();
//  }

//  void OnGUI_RayTracerMasterControl() {
//    #region Push line separator
//    EditorGUILayout.BeginVertical();
//    DanbiController.PushSpace(3);
//    EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
//    DanbiController.PushSpace(3);
//    EditorGUILayout.EndVertical();
//    #endregion

//    // 1. Draw texture picker
//    EditorGUI.BeginChangeCheck();
//    EditorGUILayout.BeginHorizontal();
//    GUILayout.BeginVertical();

//    var styleTexturePicker = new GUIStyle(GUI.skin.box);
//    styleTexturePicker.alignment = TextAnchor.UpperCenter;
//    styleTexturePicker.fixedWidth = 150;

//    TargetTex = (EditorGUILayout.ObjectField(obj: TargetTex,
//                                      objType: typeof(Texture2D),
//                                      allowSceneObjects: true,
//                                      options: new GUILayoutOption[] { GUILayout.Width(150), GUILayout.Height(150) }))
//                            as Texture2D;
//    GUILayout.EndVertical();
//    EditorGUILayout.EndHorizontal();

//    // 2. when the texture has been picked,
//    if (EditorGUI.EndChangeCheck()) {
//      var fwd = FindObjectsOfType<RayTracingMaster>();
//      if (fwd.Length != 1) {
//        Debug.LogError("Only One Prewarper is allowed at the same time!");
//        return;
//      }
//      RTMaster = fwd[0];
//      RTMaster.targetPanoramaTexFromImage = TargetTex;
//      RTMaster.ApplyNewTargetTexture(bCalledOnValidate: false, newTargetTex: TargetTex);
//      EditorUtility.SetDirty(RTMaster);
//      EditorUtility.SetDirty(TargetTex);
//    }

//    EditorGUILayout.BeginVertical();
//    // 3. Declare GUIStyle for the texture info indicator.
//    var styleLabelTextureInfoIndicator = new GUIStyle(GUI.skin.label);
//    styleLabelTextureInfoIndicator.fontSize = 40;
//    styleLabelTextureInfoIndicator.alignment = TextAnchor.UpperCenter;
//    styleLabelTextureInfoIndicator.fixedHeight = 300;

//    // 4. draw the texture info indicator.
//    var errorStr = "Select Texture!!!!!";
//    var tempStr = TargetTex ? TargetTex.name : errorStr;
//    EditorGUILayout.LabelField($"{tempStr}", styleLabelTextureInfoIndicator);
//    DanbiController.PushSpace(6);

//    styleLabelTextureInfoIndicator.fontSize = 30;

//    tempStr = TargetTex ? $"Actual Resolution : {TargetTex.width} x {TargetTex.height}" : errorStr;
//    EditorGUILayout.LabelField(tempStr, styleLabelTextureInfoIndicator);
//    DanbiController.PushSpace(6);

//    styleLabelTextureInfoIndicator.fontSize = 12;
//    tempStr = TargetTex ? $"Location : {AssetDatabase.GetAssetPath(TargetTex)}" : errorStr;
//    EditorGUILayout.LabelField(tempStr, styleLabelTextureInfoIndicator);
//    DanbiController.PushSpace(6);

//    #region Push line separator
//    EditorGUILayout.Space();
//    EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
//    EditorGUILayout.Space();
//    EditorGUILayout.EndVertical();
//    #endregion
//  }

//  void OnGUI_PrewarpedImageGenerator() {

//  }
//};
//#endif
