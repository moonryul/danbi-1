using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIMenuScript : MonoBehaviour {

    [SerializeField, Readonly]
    Transform CurrentParentUI;

    [SerializeField, Readonly]
    Button SpaceDesign;

    [SerializeField, Readonly]
    Button ImageGenerator;

    [SerializeField, Readonly]
    Button PanoramaProjection;

    void Start() {

      // 1. Get resources.
      //
      //

      CurrentParentUI = GameObject.Find("Toolbar (Panel)").transform;

      // 1. Space Design
      SpaceDesign = GameObject.Find("Space Design (Button)").GetComponent<Button>();
      if (SpaceDesign != null) {
        SpaceDesign.onClick.AddListener(OnSpaceDesignButtonClicked);
      }

      foreach (var i in SpaceDesign.GetComponentsInChildren<Transform>()) {
        if (i.name.Equals("Back (Button)")) {
          i.gameObject.GetComponent<Button>().onClick.AddListener(OnBackButtonClicked);
        }
      }

      Toggle(SpaceDesign.transform, true);
      ToggleSubmenus(SpaceDesign.transform, false);

      // 2. Image Generator
      ImageGenerator = GameObject.Find("Image Generator (Button)").GetComponent<Button>();
      if (ImageGenerator != null) {
        ImageGenerator.onClick.AddListener(OnImageGeneratorButtonClicked);
      }

      foreach (var i in ImageGenerator.GetComponentsInChildren<Transform>()) {
        if (i.name.Equals("Back (Button)")) {
          i.gameObject.GetComponent<Button>().onClick.AddListener(OnBackButtonClicked);
        }
      }

      Toggle(ImageGenerator.transform, true);
      ToggleSubmenus(ImageGenerator.transform, false);

      // 3. Panorama Projection.
      PanoramaProjection = GameObject.Find("Panorama Projection (Button)").GetComponent<Button>();
      if (PanoramaProjection != null) {
        PanoramaProjection.onClick.AddListener(OnPanoramaProjectionButtonClicked);
      }

      foreach (var i in PanoramaProjection.GetComponentsInChildren<Transform>()) {
        if (i.name.Equals("Back (Button)")) {
          i.gameObject.GetComponent<Button>().onClick.AddListener(OnBackButtonClicked);
        }
      }

      Toggle(PanoramaProjection.transform, true);
      ToggleSubmenus(PanoramaProjection.transform, false);
    }

    void OnSpaceDesignButtonClicked() {
      // 2. Space Design 아래의 서브 메뉴를 toggle.

      Toggle(SpaceDesign.transform, true);
      ToggleSubmenus(SpaceDesign.transform, true);
      CurrentParentUI = SpaceDesign.transform;
    }

    void OnImageGeneratorButtonClicked() {
      Toggle(ImageGenerator.transform, true);
      ToggleSubmenus(ImageGenerator.transform, true);
      CurrentParentUI = ImageGenerator.transform;
    }

    void OnPanoramaProjectionButtonClicked() {
      Toggle(PanoramaProjection.transform, true);
      ToggleSubmenus(PanoramaProjection.transform, true);
      CurrentParentUI = PanoramaProjection.transform;
    }

    void OnBackButtonClicked() {
      Debug.Log($"current parent UI : {CurrentParentUI.name}");
      ToggleSubmenus(CurrentParentUI, false);
    }

    void Toggle(Transform parent, bool flag) {
      parent.gameObject.SetActive(flag);
    }

    void ToggleSubmenus(Transform parent, bool flag) {
      // child index : 0 -> embedded text, 1 -> vertical layout group.
      parent.GetChild(1).gameObject.SetActive(flag);
    }

  };
};
