using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  [System.Serializable, RequireComponent(typeof(DanbiScreen), typeof(DanbiShaderControl), typeof(DanbiUIControl))]
  public class DanbiControl_Internal : MonoBehaviour {
    /// <summary>
    /// When this is true, then current renderTexture is transferred into the frame buffer.  
    /// </summary>
    [Readonly, SerializeField, Header("It toggled off to false after the image is saved.")]
    bool bStopRender = false;

    [Readonly, SerializeField, Header("When this is true, then the current RenderTexture is used for render.")]
    bool bDistortionReady = false;

    DanbiScreen CurrentScreen;

    [SerializeField, Header("It affects to the Scene at editor-time and at run-time")]
    Texture2D TargetPanoramaTex;

    public Texture2D targetPanoramaTex { get => TargetPanoramaTex; set => TargetPanoramaTex = value; }

    List<PanoramaScreenObject> CurrentPanoramaList = new List<PanoramaScreenObject>();

    /// <summary>
    /// used to raytracing to obtain  distorted image and to project the distorted image onto the scene
    /// </summary>
    Camera CurrentCamera;

    DanbiShaderControl ComputeShaderHelper;


    [Readonly, SerializeField, Space(20)]
    EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.CAPTURE;

    DanbiUIControl UIControl;


    void Start() {
      CurrentScreen = GetComponent<DanbiScreen>();
      CurrentCamera = Camera.main;
    }

  };
};
