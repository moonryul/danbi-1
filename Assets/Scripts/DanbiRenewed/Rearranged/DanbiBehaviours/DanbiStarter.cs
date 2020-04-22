using UnityEngine;
using System.Collections.Generic;

namespace Danbi {
  [System.Serializable, RequireComponent(typeof(DanbiScreen))]
  public class DanbiStarter : MonoBehaviour {
    /// <summary>
    /// When this is true, then current renderTexture is transferred into the frame buffer.  
    /// </summary>
    [SerializeField, Header("It toggled off to false after the image is saved.")]
    bool bStopRender = false;

    DanbiKernelHelper KernelHelper;

    DanbiScreen CurrentScreen;

    [SerializeField, Header("It affects to the Scene at editor-time and at run-time")]
    Texture2D TargetPanoramaTex;

    public Texture2D targetPanoramaTex { get { return TargetPanoramaTex; } set { TargetPanoramaTex = value; } }

    List<PanoramaScreenObject> CurrentPanoramaList = new List<PanoramaScreenObject>();

    /// <summary>
    /// used to raytracing to obtain  distorted image and to project the distorted image onto the scene
    /// </summary>
    Camera CurrentCamera;

    DanbiComputeShaderHelper ComputeShaderHelper;


    [SerializeField, Header("NONE, CATPTURE, PROJECTION, VIEW"), Space(20)]
    EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.CAPTURE;

    DanbiUI UIControl;


    void Start() {
      CurrentScreen = GetComponent<DanbiScreen>();
      CurrentCamera = Camera.main;
    }

  };
};
