using UnityEngine;
using System.Collections;

namespace Danbi {
  public class DanbiUIToolbarNode : MonoBehaviour {
    public DanbiUIToolbarNode parent;
    public DanbiUIToolbarNode left;
    public DanbiUIToolbarNode right;
    public DanbiUIToolbarElement elem;

    public DanbiUIToolbarNode(DanbiUIToolbarElement elem) {
      left = null;
      right = null;
      this.elem = elem;
    }

    public void OnToggleOff() {
      var allTransform = GetComponentsInChildren<Transform>();
      for (int i = 0; i < allTransform.Length; ++i) {
        allTransform[i].gameObject.SetActive(false);
      }
    }

    public void OnToggleOn() {
      var allTransform = GetComponentsInChildren<Transform>();
      for (int i = 0; i < allTransform.Length; ++i) {
        allTransform[i].gameObject.SetActive(true);
      }
    }
  }
}