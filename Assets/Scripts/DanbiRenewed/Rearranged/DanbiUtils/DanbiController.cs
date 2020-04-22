using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

public enum EDanbiPrewarperType {
  None,
  HalfSphereMirror6cm_CubeScreen,
  HalfSphereMirror7cm_CubeScreen,
  HalfSphereMirror8cm_CubeScreen,
  UFOHalfSphereMirror_CubeScreen,
  SmallParaboloidMirror_CubeScreen,
  BigParaboloidMirror_CubeScreen
};

/// <summary>
/// 
/// </summary>
/// 
public class DanbiController : EditorWindow {
  /// <summary>
  /// 
  /// </summary>
  static Camera[] Cams;
  /// <summary>
  /// 
  /// </summary>
  static Texture2D TargetTex;
  /// <summary>
  /// 
  /// </summary>
  static RayTracingMaster Master;
  /// <summary>
  /// 
  /// </summary>
  static Action<EDanbiPrewarperType> PrewarperActivatorAction;
  /// <summary>
  /// 
  /// </summary>
  static GameObject[] PrewarpersList;
  /// <summary>
  /// 
  /// </summary>
  static float UpdateCounter = 0.0f;
  public static bool bWindowOpened { get; set; }

  static Vector2 ScrollPosition;

  DanbiController thisWnd;

  public delegate void DanbiOnNewTargetTextureChanged(ref Texture2D newTargetTex);

  public static DanbiOnNewTargetTextureChanged OnNewTargetTexChanged;

  public static EDanbiPrewarperType CurrentPrewarperType { get; set; } = EDanbiPrewarperType.HalfSphereMirror8cm_CubeScreen;
  [MenuItem("Danbi/Open Controller")]
  public static void OpenWindow() {
    bWindowOpened = true;

    var thisWnd = GetWindow<DanbiController>();
    thisWnd.Show();
    // Set the title name of current window.
    thisWnd.titleContent = new GUIContent(text: "Danbi Controller!");
    PreparePrerequisites();
  }

  void OnDestroy() {
    bWindowOpened = false;
  }

  void Update() {
    UpdateCounter += Time.deltaTime;
    UpdateCounter %= 60.0f;
    int sec = (int)UpdateCounter;

    if (sec >= 10.0f) {
      UpdateCounter = 0.0f;
      PreparePrerequisites();
    }
  }

  void OnGUI() {
    //if (!ReferenceEquals(thisWnd, null)) {
    //  ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition,
    //                                                 true,
    //                                                 true,
    //                                                 GUILayout.Width(thisWnd.position.xMax * 0.32f),
    //                                                 GUILayout.Height(thisWnd.position.yMax - 80f));
    //}
    bWindowOpened = true;

    #region Main Title
    var styleLabel = new GUIStyle(GUI.skin.label);
    //styleForResoultionLabel.alignment = TextAnchor.MiddleCenter;
    styleLabel.fontSize = 60;
    styleLabel.alignment = TextAnchor.UpperCenter;
    styleLabel.fixedHeight = 600;

    DanbiController.PushSpace(3);
    EditorGUILayout.LabelField("Danbi Controller", styleLabel);
    DanbiController.PushSpace(12);
    #endregion

    #region Choose Prewarper
    EditorGUILayout.BeginVertical();
    EditorGUI.BeginChangeCheck();
    DanbiController.PushSpace(6);
    CurrentPrewarperType = (EDanbiPrewarperType)EditorGUILayout.EnumPopup("Prewarper Type ", CurrentPrewarperType);
    DanbiController.PushSpace(6);
    if (EditorGUI.EndChangeCheck()) {
      Debug.Log($"Prewarper is selected!");
      switch (CurrentPrewarperType) {
        case EDanbiPrewarperType.HalfSphereMirror6cm_CubeScreen:
        PrewarperActivatorAction(EDanbiPrewarperType.HalfSphereMirror6cm_CubeScreen);
        break;

        case EDanbiPrewarperType.HalfSphereMirror7cm_CubeScreen:
        PrewarperActivatorAction(EDanbiPrewarperType.HalfSphereMirror7cm_CubeScreen);
        break;

        case EDanbiPrewarperType.HalfSphereMirror8cm_CubeScreen:
        PrewarperActivatorAction(EDanbiPrewarperType.HalfSphereMirror8cm_CubeScreen);
        break;

        case EDanbiPrewarperType.UFOHalfSphereMirror_CubeScreen:
        PrewarperActivatorAction(EDanbiPrewarperType.UFOHalfSphereMirror_CubeScreen);
        break;

        case EDanbiPrewarperType.SmallParaboloidMirror_CubeScreen:
        PrewarperActivatorAction(EDanbiPrewarperType.SmallParaboloidMirror_CubeScreen);
        break;

        case EDanbiPrewarperType.BigParaboloidMirror_CubeScreen:
        PrewarperActivatorAction(EDanbiPrewarperType.BigParaboloidMirror_CubeScreen);
        break;
      }      
    }
    EditorGUILayout.EndVertical();
    #endregion

    #region Camera Control

    if (GUILayout.Button("Get All Camera References")) {
      Cams = FindObjectsOfType<Camera>();
      foreach (var cam in Cams) {
        Debug.Log($"{cam.name} is selected! number of cameras : {Cams.Length}.");
      }
    }

    EditorGUILayout.Space();
    if (!ReferenceEquals(Cams, null)) {
      EditorGUI.BeginChangeCheck();
      foreach (var cam in Cams) {
        cam.usePhysicalProperties = EditorGUILayout.BeginToggleGroup("Is Using Physical Camera?", cam.usePhysicalProperties);
        EditorGUILayout.Space();

        cam.focalLength = float.Parse(EditorGUILayout.TextField("Current Focal Length", cam.focalLength.ToString()));
        EditorGUILayout.LabelField("Max Focal Length = 13,1 (height to Camera : 2.04513m)");
        EditorGUILayout.LabelField("Min Focal Length = 11,1 (height to Camera : 2.04896m)");
        EditorGUILayout.Space();

        EditorGUILayout.TextField("Current Field Of View", cam.fieldOfView.ToString());
        EditorGUILayout.LabelField("Calculated by Physical Camera Props");
        EditorGUILayout.Space();

        float x, y;
        x = float.Parse(EditorGUILayout.TextField("Current Sensor Size X", cam.sensorSize.x.ToString()));
        EditorGUILayout.LabelField("(16:10 일때 = 16,36976)");
        EditorGUILayout.Space();

        y = float.Parse(EditorGUILayout.TextField("Current Sensor Size Y", cam.sensorSize.y.ToString()));
        EditorGUILayout.LabelField("(16:10 일떄 = 10,02311)");
        EditorGUILayout.LabelField("(16:9 일때 = 9,020799)");
        EditorGUILayout.Space();

        EditorGUILayout.LabelField($"Current GateFit Mode is {cam.gateFit.ToString()}");
        EditorGUILayout.EndToggleGroup();

        if (EditorGUI.EndChangeCheck()) {
          cam.sensorSize = new UnityEngine.Vector2(x, y);
          cam.gateFit = Camera.GateFitMode.None;

          if (cam.focalLength == 11.1f) {
            cam.transform.position = new Vector3(0.0f, 2.04896f, 0.0f);
          }
          else if (cam.focalLength == 13.1f) {
            cam.transform.position = new Vector3(0.0f, 2.04513f, 0.0f);
          }
          EditorUtility.SetDirty(cam);
        }
      }
    }
    #endregion

    #region Ray Tracer Master
    EditorGUILayout.BeginVertical();
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    EditorGUILayout.Space();
    EditorGUILayout.EndVertical();

    EditorGUI.BeginChangeCheck();

    EditorGUILayout.BeginHorizontal();
    TextureField(name: "Image", tex: ref TargetTex);
    EditorGUILayout.Space();
    EditorGUILayout.EndHorizontal();

    if (EditorGUI.EndChangeCheck()) {
      var fwd = FindObjectsOfType<RayTracingMaster>();
      if (fwd.Length != 1) {
        Debug.LogError("Only One Prewarper is allowed at the same time!");
        return;
      }
      Master = fwd[0];
      Master.targetPanoramaTex = TargetTex;
      Master.ApplyNewTargetTexture(bCalledOnValidate: false,newTargetTex: ref TargetTex);
      EditorUtility.SetDirty(Master);
      EditorUtility.SetDirty(TargetTex);
    }

    EditorGUILayout.BeginVertical();
    styleLabel.fontSize = 40;
    styleLabel.alignment = TextAnchor.UpperCenter;
    styleLabel.fixedHeight = 300;
    var errorStr = "Select Texture!!!!!";
    var tempStr = TargetTex ? TargetTex.name : errorStr;
    EditorGUILayout.LabelField($"{tempStr}", styleLabel);
    DanbiController.PushSpace(6);

    styleLabel.fontSize = 30;

    tempStr = TargetTex ? $"Actual Resolution : {TargetTex.width} x {TargetTex.height}" : errorStr;
    EditorGUILayout.LabelField(tempStr, styleLabel);
    DanbiController.PushSpace(6);

    styleLabel.fontSize = 13;
    tempStr = TargetTex ? $"Location : {AssetDatabase.GetAssetPath(TargetTex)}" : errorStr;
    EditorGUILayout.LabelField(tempStr, styleLabel);
    DanbiController.PushSpace(6);

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
    EditorGUILayout.Space();
    EditorGUILayout.EndVertical();
    #endregion

    #region Image Maker

    #endregion

    //if (!ReferenceEquals(thisWnd, null)) {
    //  EditorGUILayout.EndScrollView();
    //}     
  }

  void TextureField(string name, ref Texture2D tex) {
    GUILayout.BeginVertical();
    var style = new GUIStyle(GUI.skin.label);
    style.alignment = TextAnchor.UpperCenter;
    style.fixedWidth = 250;
    style.fontSize = 20;
    GUILayout.Label(name, style);

    style = new GUIStyle(GUI.skin.box);
    style.alignment = TextAnchor.UpperCenter;
    style.fixedWidth = 150;

    tex = EditorGUILayout.ObjectField(tex,
                                      typeof(Texture2D),
                                      false,
                                      new GUILayoutOption[] { GUILayout.Width(150), GUILayout.Height(150) })
                            as Texture2D;
    GUILayout.EndVertical();
  }

  static void PushSpace(int time = 1) {
    for (int i = 0; i < time; ++i) { EditorGUILayout.Space(); }
  }

  static void PreparePrerequisites() {
    var origin = default(GameObject);
    if (!ReferenceEquals(origin, null)) {
      return;
    }

    // 1. Find the RT Mesh Object Game Object to retrieve all the prewarper sets.
    origin = GameObject.Find("-------RT Mesh Objects------");

    // 2. Add all the prewarpers sets.
    PrewarpersList = new GameObject[origin.transform.childCount];
    for (int i = 0; i < origin.transform.childCount; ++i) {
      PrewarpersList[i] = origin.transform.GetChild(i).gameObject;
    }

    // 3. Bind the Action Lambda that compare to the current warper type.
    PrewarperActivatorAction = (comparer) => {
      for (int i = 0; i < PrewarpersList.Length; ++i) {
        if (PrewarpersList[i].GetComponent<DanbiPrewarperSet>().CurrentPrewarperType == comparer) {
          PrewarpersList[i].SetActive(true);
        }
        else {
          PrewarpersList[i].SetActive(false);
        }
        EditorUtility.SetDirty(PrewarpersList[i]);
      }
    };

    // 4. Bind the function for Texture Changed from the editor.
    if (OnNewTargetTexChanged == null) {
      OnNewTargetTexChanged += OnNewTargetTextureAssignedFromEditor;
    }
  }

  static void OnNewTargetTextureAssignedFromEditor(ref Texture2D newTargetTex) {
    TargetTex = newTargetTex;
  }
};
#endif
