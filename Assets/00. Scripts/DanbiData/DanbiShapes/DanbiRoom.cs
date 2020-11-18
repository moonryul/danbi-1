using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiRoom : MonoBehaviour
    {
        [SerializeField, Readonly, Header("Unit is meter")]
        Vector3 originalSize = new Vector3(320f, 226f, 320f);

        [SerializeField, Readonly, Header("Input"), Space(15)]
        float Width;

        [SerializeField, Readonly]
        float Height;

        [SerializeField, Readonly]
        float Depth;

        void Awake()
        {
            DanbiUISync.onPanelUpdate += OnPanelUpdated;
        }

        void OnValidate()
        {
            UpdateRoomScale();
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIRoomDimensionPanelControl)
            {
                var roomShapePanel = control as DanbiUIRoomDimensionPanelControl;

                Width = roomShapePanel.width;
                Height = roomShapePanel.height;
                Depth = roomShapePanel.depth;

                UpdateRoomScale();
            }
        }

        void UpdateRoomScale()
        {
            // TODO:
            if (Width < 0.0f || Height < 0.0f || Depth < 0.0f)
            {
                return;
            }

            var newScale = new Vector3(Width / originalSize.x,
                                       Height / originalSize.y,
                                       Depth / originalSize.z) * 0.99f; // multiplied 0.99f <- 
            transform.localScale = newScale;
        }
    };
};
