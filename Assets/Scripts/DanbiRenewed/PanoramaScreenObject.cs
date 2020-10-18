﻿using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PanoramaScreenObject : MonoBehaviour {
  Camera MainCamera;
  /// <summary>
  /// Object Name for readability to debugging.
  /// </summary>
  public string ObjectName;

  /// <summary>
  /// 
  /// </summary>
  public float OriginalHeightOfParnoramaMesh;

  /// <summary>
  /// 
  /// </summary>
  [SerializeField, Header("Mesh Optical Properties")]
  MeshMaterialProperty MeshMaterialProp;

  public MeshMaterialProperty meshMaterialProp { get => MeshMaterialProp; set => MeshMaterialProp = value; }

  /// <summary>
  /// 
  /// </summary>
  [SerializeField, Header("Panorama Mesh Parameters")]
  PanoramaParametre PanoramaParams;

  public PanoramaParametre panoramaParams { get => PanoramaParams; set => PanoramaParams = value; }

  public PanoramaScreenObject() {
    OriginalHeightOfParnoramaMesh = 0.6748f;
    MeshMaterialProp = new MeshMaterialProperty {
      albedo = new Vector3(0.9f, 0.9f, 0.9f),
      specular = new Vector3(0.1f, 0.1f, 0.1f),
      smoothness = 0.9f,
      emission = new Vector3(-1.0f, -1.0f, -1.0f)
    };
  }

  void Awake() {
    if (string.IsNullOrWhiteSpace(ObjectName)) {
      ObjectName = gameObject.name;
    }
    RayTracingMaster.RegisterPanoramaMesh(this);
  }

  void OnDisable() { RayTracingMaster.UnregisterPanoramaMesh(this); }

  void OnValidate() {
    var transFromCameraOrigin = new Vector3(0.0f, PanoramaParams.lowRangeFromCamera, 0.0f);
    //var mainCamPos = Camera.main.transform.position;
    var mainCamPos = transform.parent.transform.position;
    mainCamPos.z = 0.0f;
    transform.position = mainCamPos + transFromCameraOrigin;
    float scaleY = (PanoramaParams.highRangeFromCamera - PanoramaParams.lowRangeFromCamera) / OriginalHeightOfParnoramaMesh;
    transform.localScale = new Vector3(transform.localScale.x, scaleY, transform.localScale.z);
  }
};