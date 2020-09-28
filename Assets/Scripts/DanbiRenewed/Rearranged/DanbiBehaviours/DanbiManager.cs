using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi
{
    public class DanbiManager : PremenantSingletonAsComponent<DanbiManager>
    {

        public static DanbiManager Instance => instance as DanbiManager;

        void OnDestroy()
        {
            PlayerPrefs.Save();
        }
    };
};
