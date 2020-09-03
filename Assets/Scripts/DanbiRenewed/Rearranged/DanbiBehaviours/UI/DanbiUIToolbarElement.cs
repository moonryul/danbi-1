using UnityEngine;
using System.Collections;

namespace Danbi {
  [RequireComponent(typeof(DanbiUIToolbarNode))]
  /// <summary>
  /// 
  /// </summary>
  public class DanbiUIToolbarElement : MonoBehaviour {

    public static int uniqueOrderCounter = 0;
    public static int initUniqueOrder => uniqueOrderCounter++;

    [SerializeField, Readonly]
    int order;

    DanbiUIToolbarNode node;
    
    public System.Action Call_OnCalled;

    public DanbiUIToolbarElement() {
      order = initUniqueOrder;
    }

    void Start() {
      node = new DanbiUIToolbarNode(this);
      DanbiUIToolbarTree.OnRegister?.Invoke(node);
    }

    void OnDisable() {
      DanbiUIToolbarElement.uniqueOrderCounter = 0;
    }

    public static bool operator <(DanbiUIToolbarElement elem, DanbiUIToolbarElement other) {
      return elem.order < other.order;
    }

    public static bool operator >(DanbiUIToolbarElement elem, DanbiUIToolbarElement other) {
      return elem.order > other.order;
    }
  };
};