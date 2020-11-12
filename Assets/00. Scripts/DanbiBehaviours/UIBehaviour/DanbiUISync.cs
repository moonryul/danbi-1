using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public static class DanbiUISync
    {
        public delegate void OnPanelUpdate(DanbiUIPanelControl control);
        public static OnPanelUpdate onPanelUpdate;

        public static void InvokeOnPanelUpdate(DanbiUIPanelControl control)
        {
            onPanelUpdate?.Invoke(control);
        }

        public static void UnbindAll()
        {
            if (onPanelUpdate != null)
            {
                onPanelUpdate = null;
            }
        }

        // TODO:
    };
};
