using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Danbi {
  [RequireComponent(typeof(Camera))]
  public class DanbiCameraControl : MonoBehaviour {

    [SerializeField, Space(20)]
    DanbiCameraExternalData CamAdditionalData;

    #region Internal    

    [SerializeField]
    Camera MainCamRef;
    public DanbiCameraExternalData camAdditionData { get => CamAdditionalData; }

    public bool useCalibration { get; set; }

    public bool usePhysicalCamera { get; set; }

    public EDanbiCalibrationMode undistortMode { get; set; }

    public float thresholdIterative { get; set; }

    public int safetyCounter { get; set; }

    public float thresholdNewton { get; set; }

    public float fov { get; set; }

    public Vector2 nearFar { get; set; }

    public float focalLength { get; set; }

    public Vector2 sensorSize { get; set; }

    #endregion
    

    void Awake() {

      // 1. Transfer CamAdditionalData to the PrewarperSetting 
      // to Rebuild the object (stride, CamAdditionalData for ComputeShader).
      transform.parent.parent
        .GetComponent<DanbiPrewarperSetting>()
        .camAdditionalData = this.CamAdditionalData;
    }

    void Start() {

    }

    void Update() {
      if (MainCamRef.Null()) {
        MainCamRef = GetComponent<Camera>();
      }
      MainCamRef.fieldOfView = fov;
      MainCamRef.nearClipPlane = nearFar.x;
      MainCamRef.farClipPlane = nearFar.y;
      MainCamRef.usePhysicalProperties = usePhysicalCamera;
      MainCamRef.focalLength = focalLength;
      MainCamRef.sensorSize = sensorSize;
    }
  };
};
