using UnityEngine;

[SerializeField]
public enum eMovementSpeedMode {
  NORMAL, FAST, SLOW
};

[SerializeField]
public enum eMovementMode {
  FREECAM = 0, VERTICAL = 1, YROTATING = 2
};

public class CameraControl : MonoBehaviour {
  #region Exposed variables.
  [Header("The Camera moves along this."), Space(2)]
  public eMovementMode MovementMode;

  [Header("Min/Max rotation of X")]
  [Header("  -Camera attributes-"), Space(10)]
  public float MinRotationX;
  public float MaxRotationX;

  [Header("Mouse sensitivity"), Space(5)]
  public float Xsensitivity = 10.0f;
  public float Ysensitivity = 10.0f;

  [Header("Movement speed / 'Left Shift' -> Fast / 'Caps Locks' -> Slow"), Space(5)]
  public float MovementSpeed = 10.0f;
  public float FastMovementSpeed = 20.0f;
  public float SlowMovementSpeed = 3.0f;  
  #endregion

  #region Private variables.
  eMovementSpeedMode MovementSpeedMode = eMovementSpeedMode.NORMAL;

  float GetMovementSpeed {
    get {
      float res = Time.deltaTime;
      switch (MovementSpeedMode) {
        case eMovementSpeedMode.NORMAL:
        res *= MovementSpeed;
        break;

        case eMovementSpeedMode.FAST:
        res *= FastMovementSpeed;
        break;

        case eMovementSpeedMode.SLOW:
        res *= SlowMovementSpeed;
        break;
      }
      return res;
    }
  }

  float RotAroundX;
  float RotAroundY;
  #endregion

  #region Event functions.
  void Start() {
    RotAroundX = transform.eulerAngles.x;
    RotAroundY = transform.eulerAngles.y;
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.Alpha1)) {
      MovementSpeedMode = eMovementSpeedMode.SLOW;
    }

    if (Input.GetKeyDown(KeyCode.Alpha2)) {
      MovementSpeedMode = eMovementSpeedMode.NORMAL;
    }

    if (Input.GetKeyDown(KeyCode.Alpha3)) {
      MovementSpeedMode = eMovementSpeedMode.FAST;
    }

    switch (MovementMode) {
      case eMovementMode.FREECAM:
      MoveFreely();
      break;

      case eMovementMode.VERTICAL:
      MoveVertically();
      break;

      case eMovementMode.YROTATING:
      MoveYRotating();
      break;
    }
  }
  #endregion

  void MoveFreely() {
    // rotate the camera.
    RotAroundX += Input.GetAxis("Mouse Y") * Xsensitivity;
    RotAroundY += Input.GetAxis("Mouse X") * Ysensitivity;
    RotAroundX = Mathf.Clamp(RotAroundX, MinRotationX, MaxRotationX);
    transform.rotation = Quaternion.Euler(-RotAroundX, RotAroundY, 0);

    // move the camera.
    float ForwardAmount = Input.GetAxisRaw("Vertical") * GetMovementSpeed;
    float StrafeAmount = Input.GetAxisRaw("Horizontal") * GetMovementSpeed;
    transform.Translate(StrafeAmount, 0, ForwardAmount);
    // fly-upward the camera.
    if (Input.GetKey(KeyCode.E)) {
      transform.Translate(0, transform.up.y * GetMovementSpeed, 0);
    }
    // fly-downward the camera.
    if (Input.GetKey(KeyCode.Q)) {
      transform.Translate(0, -transform.up.y * GetMovementSpeed, 0);
    }
  }

  void MoveVertically() {
    float ForwardAmount = Input.GetAxisRaw("Vertical") * GetMovementSpeed;
    transform.Translate(0, 0, ForwardAmount);
  }

  void MoveYRotating() {
    float StrafeAmount = Input.GetAxisRaw("Horizontal") * GetMovementSpeed;
    transform.Rotate(new Vector3(0, 0, 1), Mathf.Rad2Deg * StrafeAmount);
  }
};
