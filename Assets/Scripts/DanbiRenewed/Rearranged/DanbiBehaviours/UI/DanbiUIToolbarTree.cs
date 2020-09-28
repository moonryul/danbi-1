// using UnityEngine;
// using System.Collections;
// using System.Data.Odbc;
// using UnityEngine.Events;
// using Boo.Lang;

// namespace Danbi {
//   public static class DanbiUIToolbarTree {
//     public static DanbiUIToolbarNode Root { get; set; }
//     public static DanbiUIToolbarNode Current { get; set; }

//     public delegate void Register(DanbiUIToolbarNode node);
//     public static Register OnRegister;

//     static DanbiUIToolbarTree() {
//       OnRegister += InsertNode;
//     }

//     static void InsertNode(DanbiUIToolbarNode node) {
//       if (Root == null) {
//         Root = node;
//         return;
//       }

//       // If there's overlapped data inside, then failed to insert!
//       if (Search(Root, node.elem) != null) {
//         Debug.LogError("<color=red>Failed to insert the new node! this node is overlapped!</color>");
//         return;
//       }

//       DanbiUIToolbarNode parent = null, cursor = Root;
//       // forward the farthest node.
//       while (cursor != null) {
//         parent = cursor;

//         // find the next node by comparison of the each element.
//         if (node.elem < parent.elem) {
//           cursor = cursor.left;
//         } else {
//           cursor = cursor.right;
//         }
//       }

//       // decide the position by the farthest node.
//       if (node.elem < parent.elem) {
//         parent.left = node;
//       } else {
//         parent.right = node;
//       }
//       // Set the parent of the cursor node.
//       node.parent = parent;
//     }

//     public static DanbiUIToolbarNode Search(DanbiUIToolbarNode cursor, string name) {
//       return Search(cursor, GameObject.Find(name).GetComponent<DanbiUIToolbarElement>());
//     }

//     public static DanbiUIToolbarNode Search(DanbiUIToolbarNode cursor, DanbiUIToolbarElement elem) {
//       if (cursor == null) {
//         return null;
//       }

//       if (elem == cursor.elem) {
//         return cursor;
//       }

//       if (elem < cursor.elem) {
//         return Search(cursor.left, elem);
//       }

//       if (elem > cursor.elem) {
//         return Search(cursor.right, elem);
//       }

//       Debug.LogError("No results after Searching!");
//       return null;
//     }

//     public enum EDanbiTreeTraverseOrder { Preorder, Inorder, Postorder };

//     static List<DanbiUIToolbarNode> Traverse_Result = new List<DanbiUIToolbarNode>();
//     public static List<DanbiUIToolbarNode> Traverse(DanbiUIToolbarNode cursor,
//                                                     EDanbiTreeTraverseOrder order = EDanbiTreeTraverseOrder.Preorder,
//                                                     System.Predicate<DanbiUIToolbarNode> pred = null) {

      
//       switch (order) {
//         case EDanbiTreeTraverseOrder.Preorder:
//           // 3. Bind as a back button or a submenu button.
//           // if cursor is a submenu button, then it's returned.
//           if (cursor != null && !pred.Invoke(cursor)) {
//             Traverse_Result.Add(cursor);
//           }

//           if (cursor.left != null) {
//             Traverse(cursor.left, order, pred);
//           }

//           if (cursor.right != null) {
//             Traverse(cursor.right, order, pred);
//           }

//           break;

//         case EDanbiTreeTraverseOrder.Inorder:
//           if (cursor.left != null) {
//             Traverse(cursor.left, order, pred);
//           }

//           if (cursor != null && !pred.Invoke(cursor)) {
//             Traverse_Result.Add(cursor);
//           }

//           if (cursor.right != null) {
//             Traverse(cursor.right, order, pred);
//           }

//           break;

//         case EDanbiTreeTraverseOrder.Postorder:
//           if (cursor != null && !pred.Invoke(cursor)) {
//             Traverse_Result.Add(cursor);
//           }

//           if (cursor.right != null) {
//             Traverse(cursor.right, order, pred);
//           }

//           if (cursor.left != null) {
//             Traverse(cursor.left, order, pred);
//           }
//           break;
//       }      
//       return Traverse_Result;
//     }

//     public static void TraverseOnButtonClicked(DanbiUIToolbarNode cursor,
//                                         UnityAction backButtonClicked,
//                                         UnityAction SubmenuButtonClicked,
//                                         out DanbiUIToolbarNode clicked,
//                                         EDanbiTreeTraverseOrder order = EDanbiTreeTraverseOrder.Preorder) {

//       // 1. Check it has button.
//       bool bHasButton = cursor.elem.hasButton;
//       // 2. Check it's back button.
//       bool isBackButton = false;
//       if (cursor.elem.name.Contains("Back")) {
//         isBackButton = true;
//       }

//       switch (order) {
//         case EDanbiTreeTraverseOrder.Preorder:
//           // 3. Bind as a back button or a submenu button.
//           // if cursor is a submenu button, then it's returned.
//           if (bHasButton) {
//             if (isBackButton) {
//               cursor.elem.CurrentButton.onClick.AddListener(backButtonClicked);
//             } else {
//               cursor.elem.CurrentButton.onClick.AddListener(SubmenuButtonClicked);
//             }
//           }
//           if (cursor.left != null) {
//             TraverseOnButtonClicked(cursor.left, backButtonClicked, SubmenuButtonClicked, out clicked, order);
//           }

//           if (cursor.right != null) {
//             TraverseOnButtonClicked(cursor.right, backButtonClicked, SubmenuButtonClicked, out clicked, order);
//           }

//           break;

//         case EDanbiTreeTraverseOrder.Inorder:
//           if (cursor.left != null) {
//             TraverseOnButtonClicked(cursor.left, backButtonClicked, SubmenuButtonClicked, out clicked, order);
//           }

//           if (bHasButton) {
//             if (isBackButton) {
//               cursor.elem.CurrentButton.onClick.AddListener(backButtonClicked);
//             } else {
//               cursor.elem.CurrentButton.onClick.AddListener(SubmenuButtonClicked);
//               clicked = cursor;
//             }
//           }

//           if (cursor.right != null) {
//             TraverseOnButtonClicked(cursor.right, backButtonClicked, SubmenuButtonClicked, out clicked, order);
//           }

//           break;

//         case EDanbiTreeTraverseOrder.Postorder:
//           if (bHasButton) {
//             if (isBackButton) {
//               cursor.elem.CurrentButton.onClick.AddListener(backButtonClicked);
//             } else {
//               cursor.elem.CurrentButton.onClick.AddListener(SubmenuButtonClicked);
//               clicked = cursor;
//             }
//           }

//           if (cursor.right != null) {
//             TraverseOnButtonClicked(cursor.right, backButtonClicked, SubmenuButtonClicked, out clicked, order);
//           }

//           if (cursor.left != null) {
//             TraverseOnButtonClicked(cursor.left, backButtonClicked, SubmenuButtonClicked, out clicked, order);
//           }
//           break;
//       }
//       clicked = null;
//     }
//   };
// };