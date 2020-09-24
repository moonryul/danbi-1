using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiRoom : MonoBehaviour
    {
        [SerializeField]
        float Width;
        [SerializeField]
        float Height;
        [SerializeField]
        float Depth;

        [SerializeField, Readonly]
        float originalHeight = 2.6f;

        [SerializeField, Readonly]
        float originalWidth = 3.2f;

        [SerializeField, Readonly]
        float originalDepth = 3.2f;

        void Start()
        {
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdated;
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (!(control is DanbiUIRoomShapePanelControl)) { return; }
            var roomShapePanel = control as DanbiUIRoomShapePanelControl;
            Width = roomShapePanel.width;
            Height = roomShapePanel.height;
            Depth = roomShapePanel.depth;
        }

        void OnValidate()
        {
            var newScale = new Vector3(originalWidth * Width,
                                       originalHeight * Height,
                                       originalDepth * Depth);
            transform.localScale = newScale;
        }
    };
};
