using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiSceneView : MonoBehaviour
    {
        [SerializeField]
        float m_movementSpeed;
        [SerializeField]
        float m_wheelMovementSpeed;
        Camera m_sceneViewCam;
        GameObject m_selected;
        Vector3 m_prevMousePos;

        // This is the target we'll orbit around
        [SerializeField]
        Transform m_target;

        // Our desired distance from the target object.
        [SerializeField]
        float m_distance = 5;

        [SerializeField]
        float m_dampening = 2;

        // These will store our currently desired angles
        Quaternion m_pitch;
        Quaternion m_yaw;

        // this is where we want to go.
        Quaternion m_targetRotation;
        Vector3 m_targetPosition;

        [SerializeField]
        float m_yawLimit = 45.0f;

        [SerializeField]
        float m_pitchLimit = 45.0f;

        public float yaw
        {
            get { return m_yaw.eulerAngles.y; }
            set { m_yaw = Quaternion.Euler(0, value, 0); }
        }

        public float pitch
        {
            get { return m_pitch.eulerAngles.x; }
            set { m_pitch = Quaternion.Euler(value, 0, 0); }
        }

        public void Move(float yawDelta, float pitchDelta)
        {
            m_yaw *= Quaternion.Euler(0, yawDelta, 0);
            m_pitch *= Quaternion.Euler(pitchDelta, 0, 0);
            ApplyConstraints();
        }

        void Awake()
        {
            m_sceneViewCam = GetComponent<Camera>();
            // initialise our pitch and yaw settings to our current orientation.
            m_pitch = Quaternion.Euler(this.transform.rotation.eulerAngles.x, 0, 0);
            m_yaw = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
        }

        /// <summary>
        /// Only for Physics update
        /// </summary>
        void FixedUpdate()
        {
            // Select the object
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
        }

        void Update()
        {
            // Move the selected object
            if (m_selected != null)
            {
                if (Input.GetMouseButton(1))
                {
                    return;
                }

                float yMovement = Input.GetAxis("Vertical"); //  ==> -1 ~ 1

                m_selected.transform.Translate(new Vector3(0.0f,
                                                           yMovement * Time.deltaTime * m_movementSpeed,
                                                           0.0f));
            }

            // calculate target positions
            m_targetRotation = m_yaw * m_pitch;
            m_targetPosition = m_target.transform.position + m_targetRotation * (-Vector3.forward * m_distance);

            // apply movement damping
            // (Yeah I know this is not a mathematically correct use of Lerp. We'll never reach destination. Sue me!)
            // (It doesn't matter because we are damping. We Do Not Need to arrive at our exact destination, we just want to move smoothly and get really, really close to it.)
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, m_targetRotation, Mathf.Clamp01(Time.smoothDeltaTime * m_dampening));

            // offset the camera at distance from the target position.
            Vector3 offset = this.transform.rotation * (-Vector3.forward * m_distance);
            this.transform.position = m_target.transform.position + offset;

            // alternatively, if we desire a slightly different behaviour, we could also add damping to the target position. But this can lead to awkward behaviour if the user rotates quickly or the damping is low.
            //this.transform.position = Vector3.Lerp(this.transform.position, m_targetPosition, Mathf.Clamp01(Time.smoothDeltaTime * _damping));
        }

        /// <summary>
        /// LateUpdate() is only responsible for updating the camera after updating all other objects.
        /// </summary>
        void LateUpdate()
        {
            // Camera move forward/backward with mouse wheel scroll
            float wheelMovement = Input.GetAxis("Mouse ScrollWheel");
            if (wheelMovement != 0.0f)
            {
                m_sceneViewCam.transform.Translate(0.0f, 0.0f, m_sceneViewCam.transform.forward.z * wheelMovement * m_wheelMovementSpeed);
            }

            // When Camera is Selected
            if (Input.GetMouseButton(1))
            {
                // Camera Input process goes here.
                float xMovement = Input.GetAxis("Horizontal");
                float yMovement = Input.GetAxis("Vertical");
                m_sceneViewCam.transform.Translate(xMovement * Time.deltaTime * m_movementSpeed,
                                                   yMovement * Time.deltaTime * m_movementSpeed,
                                                   0.0f);
            } // if (Input.GetMouseButtonDown(1))     

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Vector3 mouseDelta = Input.mousePosition - m_prevMousePos;
                Vector3 moveDelta = mouseDelta * 360.0f / Screen.height;
                Move(moveDelta.x, moveDelta.y);
            }
            m_prevMousePos = Input.mousePosition;
        } // Update()



        void ApplyConstraints()
        {
            Quaternion targetYaw = Quaternion.Euler(0, m_target.rotation.eulerAngles.y, 0);
            Quaternion targetPitch = Quaternion.Euler(m_target.rotation.eulerAngles.x, 0, 0);

            float yawDifference = Quaternion.Angle(m_yaw, targetYaw);
            float pitchDifference = Quaternion.Angle(m_pitch, targetPitch);

            float yawOverflow = yawDifference - m_yawLimit;
            float pitchOverflow = pitchDifference - m_pitchLimit;

            // We'll simply use lerp to move a bit towards the focus target's orientation. Just enough to get back within the constraints.
            // This way we don't need to worry about wether we need to move left or right, up or down.
            if (yawOverflow > 0)
            {
                m_yaw = Quaternion.Slerp(m_yaw, targetYaw, yawOverflow / yawDifference);
            }

            if (pitchOverflow > 0)
            {
                m_pitch = Quaternion.Slerp(m_pitch, targetPitch, pitchOverflow / pitchDifference);
            }
        }
    };
};
