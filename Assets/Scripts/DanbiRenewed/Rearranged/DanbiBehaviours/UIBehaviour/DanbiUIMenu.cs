using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
  public static class DanbiUIMenu
  {
    public static void ToggleSubMenus(Transform parent, bool flag)
    {
      // child index : 0 -> embedded text, 1 -> vertical layout group.
      parent.GetChild(1).gameObject.SetActive(flag);
    }
  };
};
