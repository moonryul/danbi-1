using System.Collections;
using System.Collections.Generic;

using UnityEngine;
namespace Danbi {
  [RequireComponent(typeof(Camera))]
  public class DanbiCameraControl : MonoBehaviour {

    Camera MainCamRef;
    Camera mainCamRef {
      get {
        if (MainCamRef.Null()) {
          MainCamRef = Camera.main;
          Debug.Log($"Main Cam Ref is assigned initially!", this);
        }
        return MainCamRef;
      }
    }

    [SerializeField]
    bool IsPhysicalCameraUsed = false;

    [SerializeField]
    float FieldOfView = 33.3f;
    float Original_fov;

    [SerializeField]
    float FocalLength = 50.0f;
    float calculated_fov {
      get {
        float vFOVInRads = mainCamRef.fieldOfView * Mathf.Deg2Rad;
        float hFOVInRads = 2 * Mathf.Atan(Mathf.Tan(vFOVInRads / 2) * mainCamRef.aspect);
        float hFOV = hFOVInRads * Mathf.Rad2Deg;
        Debug.Log($"horizontal FOV : {hFOV}");
        return FocalLength;
      }
    }
    float Original_FocalLength;

    [SerializeField]
    Vector2 SensorSize = new Vector2(36, 24);
    Vector2 Original_SensorSize;

    DanbiCameraControl() {
      // Cache the original value.
      Original_fov = FieldOfView;
      Original_FocalLength = FocalLength;
      Original_SensorSize = SensorSize;
    }

    void Reset() {
      mainCamRef.usePhysicalProperties = IsPhysicalCameraUsed;
      mainCamRef.fieldOfView = Original_fov;
      mainCamRef.focalLength = Original_FocalLength;
      mainCamRef.sensorSize = Original_SensorSize;
    }

    void OnValidate() {
      
    }

    private void Update() {
      Debug.Log($"{Camera.main.fieldOfView}", this);
    }

  };
};
