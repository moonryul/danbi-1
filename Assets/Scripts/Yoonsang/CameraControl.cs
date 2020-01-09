using UnityEngine;
using UnityEngine.Assertions;

[SerializeField]
public enum eMovementSpeedMode {
  NORMAL, FAST, SLOW
};

public class CameraControl : MonoBehaviour {
  #region Exposed variables.
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

  [Header("Toggle itself -> 'V'/ Move upward -> 'S'/ Move downward -> 'W'."), Space(10)]
  public bool DoesMoveVerticallyOnly;
  [Header("When camera moves only vertically, it's aligned to the target."), Space(5)]
  public Transform Target;
  #endregion

  #region Private variables.
  eMovementSpeedMode MovementSpeedMode;
  float RotAroundX;
  float RotAroundY;
  #endregion

  #region Event functions.
  void Start() {
    RotAroundX = transform.eulerAngles.x;
    RotAroundY = transform.eulerAngles.y;
    Assert.IsNotNull(Target, "Look At Target of Camera Control is null!");
    transform.LookAt(Target);
  }

  void Update() {
    // V toggles the movement mode.
    if (Input.GetKeyDown(KeyCode.V)) {
      DoesMoveVerticallyOnly = !DoesMoveVerticallyOnly;
    }

    // Left shift for a fast mode.
    if (Input.GetKey(KeyCode.LeftShift)) {
      MovementSpeedMode = eMovementSpeedMode.FAST;
    } else
    // Caps lock for a slow move.
    if (Input.GetKey(KeyCode.CapsLock)) {
      MovementSpeedMode = eMovementSpeedMode.SLOW;
    } // if not, a normal mode.
    else {
      MovementSpeedMode = eMovementSpeedMode.NORMAL;
    }

    if (!DoesMoveVerticallyOnly) {
      // Fly freely.
      MoveFreely();
    } else {
      // Fly only vertically.
      MoveVertically();
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
    float ForwardAmount = Input.GetAxisRaw("Vertical") * GetMovementSpeed();
    float StrafeAmount = Input.GetAxisRaw("Horizontal") * GetMovementSpeed();
    transform.Translate(StrafeAmount, 0, ForwardAmount);
    // fly-upward the camera.
    if (Input.GetKey(KeyCode.E)) {
      transform.Translate(0, transform.up.y * GetMovementSpeed(), 0);
    }
    // fly-downward the camera.
    if (Input.GetKey(KeyCode.Q)) {
      transform.Translate(0, -transform.up.y * GetMovementSpeed(), 0);
    }
  }

  void MoveVertically() {
    float ForwardAmount = Input.GetAxisRaw("Vertical") * GetMovementSpeed();
    transform.Translate(0, 0, ForwardAmount);
  }

  float GetMovementSpeed() {
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
};