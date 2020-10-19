using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIInteractionPanelControl : DanbiUIPanelControl
    {
        protected override void SaveValues()
        {
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // 1. bind the detect anything text
            
            // 2. bind the gesture walking text

            // 3. bind the gesture swipe Left to Right text

            // 4. bind the gesture swipe Right to Left text
        }
    };
};
