using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiRoom : MonoBehaviour
    {
        [SerializeField, Readonly, Header("Unit is meter")]
        Vector3 m_originalSize = new Vector3(320f, 226f, 320f);

        [SerializeField, Readonly, Header("Input"), Space(15)]
        float m_width;

        [SerializeField, Readonly]
        float m_height;

        [SerializeField, Readonly]
        float m_depth;

        void Awake()
        {
            DanbiUIRoomDimensionPanel.onWidthUpdate +=
                (float width) =>
                {
                    m_width = width;
                    UpdateRoomScale();
                };

            DanbiUIRoomDimensionPanel.onHeightUpdate +=
                (float height) =>
                {
                    m_height = height;
                    UpdateRoomScale();
                };

            DanbiUIRoomDimensionPanel.onDepthUpdate +=
                (float depth) =>
                {
                    m_depth = depth;
                    UpdateRoomScale();
                };
        }

        void UpdateRoomScale()
        {
            // TODO:
            if (m_width < 0.0f || m_height < 0.0f || m_depth < 0.0f)
            {
                return;
            }

            var newScale = new Vector3(m_width / m_originalSize.x,
                                       m_height / m_originalSize.y,
                                       m_depth / m_originalSize.z) * 0.99f; // multiplied 0.99f <- 
            transform.localScale = newScale;
        }
    };
};
