using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public static class DanbiUISync
    {
        public delegate void OnPanelUpdate(DanbiUIPanelControl control);
        public static OnPanelUpdate onPanelUpdated;

        public static void UnbindAll()
        {
            if (onPanelUpdated != null)
            {
                onPanelUpdated = null;
            }
        }
    };
};
