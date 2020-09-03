using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi {
  /// <summary>
  /// 
  /// </summary>
  public class DanbiBaseSubmenu : MonoBehaviour {
    Transform LastClickedButtonTransform;

    void Start() {
      SetupSubmenu();
    }
    void SetupSubmenu() {
      var verticalGroup = transform.transform.GetChild(1);
      for (int i = 0; i < verticalGroup.childCount; ++i) {
        var submenuElement = verticalGroup.GetChild(i);
        var submenuButton = submenuElement.GetComponent<Button>();

        if (submenuButton.name.Contains("Back")) {
          BindOnBackButtonClicked(submenuButton);
        } else {
          // bind the level 1 Sub menus.
          BindOnSubmenuButtonClicked(submenuButton, verticalGroup);
          ToggleSubMenus(verticalGroup.transform, false);
        }
      }
      ToggleSubMenus(verticalGroup.transform, false);
    }
    public virtual void OnMenuButtonSelected(Transform[] otherTopbarMenus) {
      foreach (var i in otherTopbarMenus) {
        if (!i.Equals(transform.parent)) {
          ToggleSubMenus(i, false);
        }

      }
    }

    void BindOnSubmenuButtonClicked(Button submenuButton, Transform verticalGroup) {
      var anker = submenuButton.transform.GetChild(1);
      var panel = anker.GetChild(0);
      panel.position = new Vector3(0, 0, 0);

      submenuButton?.onClick.AddListener(() => {
        LastClickedButtonTransform = submenuButton.transform;
        ToggleSubMenus(LastClickedButtonTransform, true);
      });
    }

    void BindOnBackButtonClicked(Button backButton) {
      backButton?.onClick.AddListener(() => {
        ToggleSubMenus(LastClickedButtonTransform, false);
      });
    }

    void ToggleSubMenus(Transform parent, bool flag) {
      // child index : 0 -> embedded text, 1 -> vertical layout group.
      parent.GetChild(1).gameObject.SetActive(flag);
    }
  };
};
