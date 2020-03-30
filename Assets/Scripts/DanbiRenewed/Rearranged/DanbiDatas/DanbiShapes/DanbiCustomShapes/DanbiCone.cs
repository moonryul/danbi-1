using UnityEngine;

namespace Danbi {
  public class DanbiCone : DanbiCustomShape {
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    internal struct DanbiConeParams {
      public float DistanceFromCamera;
      public float Height;
      public float Radius;
      public float MaskingRatio;
    };

    [SerializeField] DanbiConeParams ConeShapeParam;

    public DanbiCone(string newShapeName) : base(newShapeName) { /**/ }

    protected override void Start() {
      base.Start();
      ConeShapeParam = new DanbiConeParams {
        DistanceFromCamera = 0.1f,
        Height = 0.05f,
        Radius = 0.03f,
        MaskingRatio = 0.1f
      };
    }


    protected override void OnCustomShapeChanged() {
      base.OnCustomShapeChanged();
      var CameraOriginLocation = new Vector3(0.0f, -(ConeShapeParam.DistanceFromCamera + ConeShapeParam.Height), 0.0f);
      var CameraLocation = MainCamRef.transform.position;
      transform.position = CameraOriginLocation + CameraOriginLocation;
    }
  };
};