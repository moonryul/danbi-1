using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiRoom : MonoBehaviour
    {
        [SerializeField, Readonly, Header("Unit is meter")]
        Vector3 originalSize = new Vector3(3.2f, 2.26f, 3.2f);

        [SerializeField, Readonly, Header("Input"), Space(15)]
        float Width;

        [SerializeField, Readonly]
        float Height;
        
        [SerializeField, Readonly]
        float Depth;

        void Start()
        {
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdated;
        }

        void OnDisable()
        {
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdated;
        }

        void OnValidate()
        {
            UpdateScale();
        }

        void OnPanelUpdated(DanbiUIPanelControl control)
        {
            if (control is DanbiUIRoomShapePanelControl)
            {
                var roomShapePanel = control as DanbiUIRoomShapePanelControl;

                Width = roomShapePanel.width;
                Height = roomShapePanel.height;
                Depth = roomShapePanel.depth;

                UpdateScale();
            }
        }

        void UpdateScale()
        {
            var newScale = new Vector3(Width / originalSize.x, Height / originalSize.y, Depth / originalSize.z) * 0.99f;
            transform.localScale = newScale;
        }
    };
};
