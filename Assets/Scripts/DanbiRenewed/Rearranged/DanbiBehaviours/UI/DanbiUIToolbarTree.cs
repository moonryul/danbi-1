using UnityEngine;
using System.Collections;
using System.Data.Odbc;

namespace Danbi {
  public static class DanbiUIToolbarTree {
    static DanbiUIToolbarNode Root;

    public delegate void Register(DanbiUIToolbarNode node);
    public static Register OnRegister;

    static DanbiUIToolbarTree() {
      OnRegister += InsertNode;
    }

    static void InsertNode(DanbiUIToolbarNode node) {
      // If there's overlapped data inside, then failed  to insert!
      if (Search(Root, node.elem) != null) {
        Debug.LogError("<color=red>Failed to insert the new node! this node is overlapped!</color>");
        return;
      }

      DanbiUIToolbarNode parent = null, current = Root;
      // forward the farthest node.
      while (current != null) {
        parent = current;

        // find the next node by comparison of the each element.
        if (node.elem < parent.elem) {
          current = current.left;
        } else {
          current = current.right;
        }
      }

      // decide the position by the farthest node.
      if (node.elem < parent.elem) {
        parent.left = node;
      } else {
        parent.right = node;
      }
      // Set the parent of the current node.
      node.parent = parent;
    }

    public static DanbiUIToolbarNode Search(DanbiUIToolbarNode current, string name) {
      return Search(current, GameObject.Find(name).GetComponent<DanbiUIToolbarElement>());
    }

    public static DanbiUIToolbarNode Search(DanbiUIToolbarNode current, DanbiUIToolbarElement elem) {
      if (current == null) {
        return null;
      }

      if (elem == current.elem) {
        return current;
      }

      if (elem < current.elem) {
        return Search(current.left, elem);
      }

      if (elem > current.elem) {
        return Search(current.right, elem);
      }

      return null;
    }

    public enum EDanbiTreeTraverseOrder { Preorder, Inorder, Postorder };

    static void Apply(DanbiUIToolbarNode current, System.Action action) {
      if (current == null) {
        return;
      }
      current.elem.Call_OnCalled += action;
    }

    static void TraverseAll(EDanbiTreeTraverseOrder order, DanbiUIToolbarNode current, System.Action action) {
      switch (order) {
        case EDanbiTreeTraverseOrder.Preorder:
          Apply(current, action);
          TraverseAll(EDanbiTreeTraverseOrder.Preorder, current.left, action);
          TraverseAll(EDanbiTreeTraverseOrder.Preorder, current.right, action);
          break;

        case EDanbiTreeTraverseOrder.Inorder:
          TraverseAll(EDanbiTreeTraverseOrder.Preorder, current.left, action);
          Apply(current, action);
          TraverseAll(EDanbiTreeTraverseOrder.Preorder, current.left, action);
          break;

        case EDanbiTreeTraverseOrder.Postorder:
          Apply(current, action);
          TraverseAll(EDanbiTreeTraverseOrder.Preorder, current.right, action);
          TraverseAll(EDanbiTreeTraverseOrder.Preorder, current.left, action);
          break;
      }
    }

    public static void ForEach(System.Action action) {
      TraverseAll(EDanbiTreeTraverseOrder.Preorder, Root, action);
    }
  };
};