// using System.Collections;
// using System.Collections.Generic;

// using UnityEngine;

// namespace Danbi {
//   public class DanbiUIToolbarControl2 : MonoBehaviour {

//     DanbiUIToolbarNode Clicked;

//     public delegate void OnUpdateClickedNode(DanbiUIToolbarNode clicked);
//     public static OnUpdateClickedNode Call_OnUpdateClickedNode;

//     void Start() {
//       DanbiUIToolbarTree.TraverseOnButtonClicked(DanbiUIToolbarTree.Root,
//                                                  OnBackButtonClicked,
//                                                  OnSubmenuButtonClicked,
//                                                  out Clicked);

//       Call_OnUpdateClickedNode += (clicked) => {
//         Clicked = clicked;
//       };
//     }

//     public void OnBackButtonClicked() {
//       var cur = DanbiUIToolbarTree.Current;
//       // 1. If there's opened panel, then turn it off.
//       if (cur.elem.hasPanel) {
//         cur.elem.TogglePanel(false);
//       }

//       // 2. turn off the current button.
//       // its children is automatically turned off.
//       cur.elem.gameObject.SetActive(false);
//       // set current as a parent.
//       cur = cur.parent;
//     }

//     public void OnSubmenuButtonClicked() {
//       var cur = Clicked;
//       var temp = Clicked;
//       var ignoredNodes = new List<DanbiUIToolbarNode>();
//       while (temp != null) {
//         ignoredNodes.Add(temp);
//         temp = temp.parent;
//       }

//       var allNodes = DanbiUIToolbarTree.Traverse(DanbiUIToolbarTree.Root,
//                                                  DanbiUIToolbarTree.EDanbiTreeTraverseOrder.Preorder,
//                                                  (node) => {
//                                                    foreach (var i in ignoredNodes) {
//                                                      if (node == i) {
//                                                        return true;
//                                                      }
//                                                    }
//                                                    return false;
//                                                  });
//       foreach (var i in allNodes) {
//         if (i.elem.hasPanel) {
//           i.elem.TogglePanel(false);
//         }
//         i.elem.gameObject.SetActive(false);
//       }

//       // 1. loop till the next parent is topmenu button (level 1)
//       while (cur.parent.elem.level > 1) {
//         // 2. If there's opened panel, then turn it off.
//         if (cur.elem.hasPanel) {
//           cur.elem.TogglePanel(false);
//         }

//         if (cur.elem.hasButton) {
//           cur.elem.ToggleButton(false);
//         }

//         // 3. Turn off the current panel.
//         cur.elem.gameObject.SetActive(true);
//         // 4. Set current as a parent. move upward.
//         cur = cur.parent;
//       }
//       // 5. Move current to the clicked node.
//       cur = DanbiUIToolbarTree.Search(cur, Clicked.elem);
//       // 6. 
//       if (cur.elem.hasPanel) {
//         cur.elem.TogglePanel(true);
//       }

//       if (cur.elem.hasButton) {
//         cur.elem.ToggleButton(true);
//         //cur.elem.ToggleSubmenuAll(true);
//       }
//     }
//   };
// };
