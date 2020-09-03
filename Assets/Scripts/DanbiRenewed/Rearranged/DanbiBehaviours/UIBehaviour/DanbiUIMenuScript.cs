using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIMenuScript : MonoBehaviour {
    
    Transform LastClickedButtonTransform;    

    Transform Toolbar;

    void Start() {
      Toolbar = GameObject.Find("Toolbar (Panel)").transform;

      SetupTopbarMenu(0);
      //GetTopbarMenuElement(toolbar, 1);
      //GetTopbarMenuElement(toolbar, 2);
    }

    /// <summary>
    /// Bind the toolbar buttons (Space Design, Generator, Realtime)
    ///  and 
    /// 
    /// </summary>
    /// <param name="Toolbar"></param>
    /// <param name="childIndex">0 => Space Design | 1 => Generator | 2 => Realtime</param>
    void SetupTopbarMenu(int childIndex) {
      var toolbarButton = Toolbar.GetChild(childIndex).GetComponent<Button>();
      // 1. Bind the toolbar button.
      BindOnToolbarButtonClicked(toolbarButton);

      // 2. Get the vertical group.      
      var verticalGroup = toolbarButton.transform.GetChild(1);

      // iterate all the vertical groups under the toolbar to bind submenu items.
      for (int i = 0; i < verticalGroup.childCount; ++i) {
        // forward the submenu element.
        var submenuElement = verticalGroup.GetChild(i);
        var submenuButton = submenuElement.GetComponent<Button>();

        // Bind if the button is Back button.
        if (submenuButton.name.Equals("Back (Button)")) {
          BindOnBackButtonClicked(submenuButton);
        } else {
          // Bind the submenu onClick listeners.
          // each listeners are same as verticalGroup.childCount.
          BindOnSubmenuButtonClicked(submenuButton);
        }
      }

      // Close the level 1 submenus.
      ToggleSubMenus(toolbarButton.transform, false);
    }

    void BindOnToolbarButtonClicked(Button toolbarButton) {
      toolbarButton?.onClick.AddListener(
        () => {
          LastClickedButtonTransform = toolbarButton.transform;
          ToggleSubMenus(LastClickedButtonTransform, true);
        });
    }

    void BindOnBackButtonClicked(Button backButton) {
      backButton?.onClick.AddListener(() => {
        ToggleSubMenus(LastClickedButtonTransform, false);
      });
    }

    void BindOnSubmenuButtonClicked(Button submenuButton) {
      submenuButton?.onClick.AddListener(() => {
        submenuButton.transform.GetComponent<DanbiIBaseSubmenu>().OnMenuButtonSelected();
        //Debug.Log($"<color=green>HI! :: {submenuButton.name}</color>");
      });
      // Set Submenu Attach Location as deactive.
      submenuButton.transform.GetChild(1).gameObject.SetActive(false);
    }

    void ToggleSubMenus(Transform parent, bool flag) {
      // child index : 0 -> embedded text, 1 -> vertical layout group.
      parent.GetChild(1).gameObject.SetActive(flag);
    }

  };
};
