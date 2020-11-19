using UnityEngine;

namespace Danbi
{
    public class DanbiProjectCameraAutoRotator : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.Rotate(new Vector3(0.0f, 0.01f, 0.0f), Space.Self);
        }
    };
};
