using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Danbi {
  /// <summary>
  /// 
  /// </summary>
  public class DanbiUIToolbarElement : MonoBehaviour {

    public static int uniqueOrderCounter = 0;
    public static int initUniqueOrder => uniqueOrderCounter++;

    /// <summary>
    /// Show the current level in the tree.
    /// </summary>
    [SerializeField]
    int Level;
    public int level => Level;

    /// <summary>
    /// Need to balance the tree.
    /// </summary>
    [SerializeField, Readonly]
    int order;

    public Button CurrentButton { get; set; }

    public bool hasButton => !CurrentButton.Null();

    public void ToggleButton(bool flag) {
      CurrentButton.gameObject.SetActive(flag);
    }

    public List<Button> SubmenuButton { get; set; }

    public void ToggleSubmenu(bool flag, int index) {
      SubmenuButton[index].gameObject.SetActive(flag);
    }

    public void ToggleSubmenuAll(bool flag) {
      for (int i = 0; i < SubmenuButton.Count; ++i) {
        SubmenuButton[i].gameObject.SetActive(flag);
      }
    }

    [SerializeField]
    bool HasPanel = true;

    GameObject Panel;

    public bool hasPanel => !Panel.Null() && HasPanel;

    public void TogglePanel(bool flag) {
      Panel.SetActive(flag);
    }

    DanbiUIToolbarNode node;

    public DanbiUIToolbarElement() {
      order = initUniqueOrder;
    }

    void Start() {
      node = new DanbiUIToolbarNode(this);
      DanbiUIToolbarTree.OnRegister?.Invoke(node);

      CurrentButton = GetComponent<Button>();
      CurrentButton?.onClick.AddListener(OnSubmenuButtonClicked);
      if (level > 1) {
        CurrentButton.gameObject.SetActive(false);
      }


      if (HasPanel) {
        Panel = transform.GetChild(1)?.gameObject;
        Panel?.SetActive(false);
      }      
    }

    void OnDisable() {
      DanbiUIToolbarElement.uniqueOrderCounter = 0;
    }

    void OnSubmenuButtonClicked() {
      DanbiUIToolbarControl2.Call_OnUpdateClickedNode?.Invoke(node);
    }

    public static bool operator <(DanbiUIToolbarElement elem, DanbiUIToolbarElement other) {
      return elem.level < other.level ?
        elem.level < other.level : elem.order < other.order;
    }

    public static bool operator >(DanbiUIToolbarElement elem, DanbiUIToolbarElement other) {
      return elem.level > other.level ? 
        elem.level > other.level : elem.order > other.order;
    }
  };
};