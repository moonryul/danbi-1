using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIMenuScript : MonoBehaviour {

    [SerializeField, Readonly]
    Transform lastClickedButtonTransform;

    //[SerializeField, Readonly]
    //Button SpaceDesign;

    //[SerializeField, Readonly]
    //Button ImageGenerator;

    //[SerializeField, Readonly]
    //Button PanoramaProjection;

    [SerializeField, Readonly]
    Transform CurrentSubmenuUITransform;

    void Start() {
      var toolbar = GameObject.Find("Toolbar (Panel)").transform;

      GetTopbarMenuElement(toolbar, 0);
      GetTopbarMenuElement(toolbar, 1);
      GetTopbarMenuElement(toolbar, 2);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="toolbar"></param>
    /// <param name="childIndex">0 => Space Design | 1 => Generator | 2 => Realtime</param>
    void GetTopbarMenuElement(Transform toolbar, int childIndex) {
      // Get the Button first.
      var toolbarButton = toolbar.GetChild(childIndex).GetComponent<Button>();
      // Bind the toolbar button.
      this.BindOnClickListener(toolbarButton,
        () => {
          lastClickedButtonTransform = toolbarButton.transform;
          ToggleSubMenus(lastClickedButtonTransform, true);
        });

      // Get the vertical group.
      var verticalGroup = toolbarButton.transform.GetChild(1);

      for (int i = 0; i < verticalGroup.childCount; ++i) {
        // forward the submenu element.
        var submenuElement = verticalGroup.GetChild(i);
        var submenuButton = submenuElement.GetComponent<Button>();

        // Bind if the button is Back button.
        if (submenuButton.name.Equals("Back (Button)")) {
          this.BindOnClickListener(submenuButton,
            () => {
              ToggleSubMenus(lastClickedButtonTransform, false);
            });
        } else {
          // Bind the submenu onClick listeners. each listeners are same as verticalGroup.childCount.
          Transform child = submenuElement.GetChild(1);
          switch (childIndex) {
            case 0:                            
              BindOnClickListener(submenuButton,
                                  child.GetComponent<DanbiInitialDetail>().OnMenuButtonSelected);
              child.gameObject.SetActive(false);
              break;

            case 1:
              BindOnClickListener(submenuButton,
                                  child.GetComponent<DanbiGeneratorDetail>().OnMenuButtonSelected);
              child.gameObject.SetActive(false);
              break;

            case 2:
              BindOnClickListener(submenuButton,
                                  child.GetComponent<DanbiRealtimeDetail>().OnMenuButtonSelected);
              child.gameObject.SetActive(false);
              break;
          }
        }
      }

      // Close the submenus.
      ToggleSubMenus(toolbarButton.transform, false);
    }

    void BindOnClickListener(Button button, UnityEngine.Events.UnityAction func) {
      button?.onClick.AddListener(func);
    }

    void ToggleSubMenus(Transform parent, bool flag) {
      // child index : 0 -> embedded text, 1 -> vertical layout group.
      parent.GetChild(1).gameObject.SetActive(flag);
    }

  };
};
