using UnityEngine;

namespace Danbi
{
    public class DanbiFreeCam : MonoBehaviour
    {
        public enum eMovementSpeedMode
        {
            normal, slow
        };

        public enum eMovementMode
        {
            freecam, orbit
        };

        Camera m_sceneViewCam;
        GameObject m_selected;

        #region Exposed variables.
        [Header("The Camera moves along this."), Space(2)]
        public eMovementMode m_movementMode;

        public float m_wheelMovementSpeed;

        [Header("Min/Max rotation of X")]
        [Header("  -Camera attributes-"), Space(10)]
        public float m_minRotationX;
        public float m_maxRotationX;

        [Header("Mouse sensitivity"), Space(5)]
        public float m_xSensitivity = 10.0f;
        public float m_ySensitivity = 10.0f;

        [Header("Movement speed / 'Left Shift' -> Fast / 'Caps Locks' -> Slow"), Space(5)]
        public float m_normalMovementSpeed = 10.0f;
        public float m_slowMovementSpeed = 3.0f;

        [Space(20)]
        public Transform target;
        public float distance = 2.0f;
        public float xSpeed = 20.0f;
        public float ySpeed = 20.0f;
        public float yMinLimit = -90f;
        public float yMaxLimit = 90f;
        public float distanceMin = 10f;
        public float distanceMax = 10f;
        public float smoothTime = 2f;
        float rotationYAxis = 0.0f;
        float rotationXAxis = 0.0f;
        float velocityX = 0.0f;
        float velocityY = 0.0f;
        #endregion

        #region Private variables.
        eMovementSpeedMode m_movementSpeedMode = eMovementSpeedMode.normal;

        float getMovementSpeed
        {
            get
            {
                float res = Time.deltaTime;
                switch (m_movementSpeedMode)
                {
                    case eMovementSpeedMode.normal:
                        res *= m_normalMovementSpeed;
                        break;

                    case eMovementSpeedMode.slow:
                        res *= m_slowMovementSpeed;
                        break;
                }
                return res;
            }
        }

        float RotAroundX;
        float RotAroundY;

        float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
        #endregion

        #region Event functions.
        void Start()
        {
            m_sceneViewCam = GetComponent<Camera>();
            RotAroundX = transform.eulerAngles.x;
            RotAroundY = transform.eulerAngles.y;

            Vector3 angles = transform.eulerAngles;
            rotationYAxis = angles.y;
            rotationXAxis = angles.x;
        }

        void Update()
        {
            // Speed Normal <-> Slow ( left shift hold )
            if (Input.GetKey(KeyCode.LeftShift))
            {
                m_movementSpeedMode = eMovementSpeedMode.slow;
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                m_movementSpeedMode = eMovementSpeedMode.normal;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_movementMode = eMovementMode.freecam;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                m_movementMode = eMovementMode.orbit;
            }

            // Select the object (mouse left button click)
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = m_sceneViewCam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000.0f)) // click
                {
                    // if something hit by the ray
                    m_selected = hit.collider.gameObject;

                    // TODO:
                    // enable the outline shader on the selected object
                    // update the UI of the selected object (Transform)
                }
                else // unclick
                {
                    // nothing collided
                    m_selected = null;

                    // TODO:
                    // disable all the outline shader on the selected object
                    // update the UI with 0 of the selected object (Transform)
                }
            } // if (Input.GetMouseButtonDown(0))

            // Move the selected object
            if (m_selected != null)
            {
                if (Input.GetMouseButton(1))
                {
                    return;
                }

                float yMovement = Input.GetAxis("Vertical"); //  ==> -1 ~ 1

                m_selected.transform.Translate(new Vector3(0.0f,
                                                           yMovement * Time.deltaTime * m_normalMovementSpeed,
                                                           0.0f));
            }
        }

        void LateUpdate()
        {
            // Camera move forward/backward (mouse wheel scroll up/down)
            float wheelMovement = Input.GetAxis("Mouse ScrollWheel");
            if (wheelMovement != 0.0f)
            {
                m_sceneViewCam.transform.Translate(0.0f, 0.0f, m_sceneViewCam.transform.forward.z * wheelMovement * m_wheelMovementSpeed);
            }

            // Movement mode (mouse right button hold)
            if (Input.GetMouseButton(1))
            {
                switch (m_movementMode)
                {
                    case eMovementMode.freecam:
                        MoveFreely();
                        break;

                    case eMovementMode.orbit:
                        MoveAlongOrbit();
                        break;
                }
            }
        }
        #endregion

        void MoveFreely()
        {
            // rotate the camera.
            RotAroundX += Input.GetAxis("Mouse Y") * m_xSensitivity;
            RotAroundY += Input.GetAxis("Mouse X") * m_ySensitivity;
            RotAroundX = Mathf.Clamp(RotAroundX, m_minRotationX, m_maxRotationX);
            transform.rotation = Quaternion.Euler(-RotAroundX, RotAroundY, 0);

            // move the camera.
            float ForwardAmount = Input.GetAxisRaw("Vertical") * getMovementSpeed;
            float StrafeAmount = Input.GetAxisRaw("Horizontal") * getMovementSpeed;
            transform.Translate(StrafeAmount, 0, ForwardAmount);
            // fly-upward the camera.
            if (Input.GetKey(KeyCode.E))
            {
                transform.Translate(0, transform.up.y * getMovementSpeed, 0);
            }
            // fly-downward the camera.
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Translate(0, -transform.up.y * getMovementSpeed, 0);
            }
        }

        void MoveAlongOrbit()
        {
            if (target)
            {
                if (Input.GetMouseButton(1))
                {
                    velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * 0.005f;
                    velocityY += ySpeed * Input.GetAxis("Mouse Y") * 0.02f;
                }
                rotationYAxis += velocityX;
                rotationXAxis -= velocityY;
                rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
                Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
                Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
                Quaternion rotation = toRotation;

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
                if (Physics.Linecast(target.position, transform.position, out var hit))
                {
                    distance -= hit.distance;
                }
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + target.position;

                transform.rotation = rotation;
                transform.position = position;
                velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
                velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
            }
        }
    };
};
