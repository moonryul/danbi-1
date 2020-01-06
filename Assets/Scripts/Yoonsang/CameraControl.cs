using UnityEngine;
using UnityEngine.Assertions;

public class CameraControl : MonoBehaviour {

  #region Exposed variables.
  [Header("  -Camera attributes-"), Space(10)]
  public float MinRotationX;
  public float MaxRotationX;
  [Space(5)]
  public float Xsensitivity = 10.0f;
  public float Ysensitivity = 10.0f;
  [Space(5)]
  public float MovementSpeed = 10.0f;

  [Header("Toggle itself -> 'V'/ Move upward -> 'Q'/ Move downward -> 'E'."), Space(10)]
  public bool DoesMoveVerticallyOnly;
  [Header("When camera moves only vertically, it's aligned to the target."), Space(5)]
  public Transform Target;
  #endregion

  #region Private variables.
  float RotAroundX;
  float RotAroundY;
  float OriginalMovementSpeed;
  float ForwardAmount;
  float StrafeAmount;
  #endregion

  #region Event functions.
  void Start() {
    RotAroundX = transform.eulerAngles.x;
    RotAroundY = transform.eulerAngles.y;
    OriginalMovementSpeed = MovementSpeed;
    transform.LookAt(Target);

    Assert.IsNotNull(Target, "Look At Target of Camera Control is null!");
  }

  void Update() {
    // V toggles the movement mode.
    if (Input.GetKeyDown(KeyCode.V)) {
      DoesMoveVerticallyOnly = !DoesMoveVerticallyOnly;
      if (DoesMoveVerticallyOnly) {
        transform.LookAt(Target);
      }
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
    // move faster.
    if (Input.GetKey(KeyCode.LeftShift)) {
      MovementSpeed = OriginalMovementSpeed * 2.0f;
    }
    if (!Input.GetKey(KeyCode.LeftShift) &&
      OriginalMovementSpeed != MovementSpeed) {
      MovementSpeed = OriginalMovementSpeed;
    }
    // move the camera.
    ForwardAmount = Input.GetAxisRaw("Vertical") * MovementSpeed * Time.deltaTime;
    StrafeAmount = Input.GetAxisRaw("Horizontal") * MovementSpeed * Time.deltaTime;
    transform.Translate(StrafeAmount, 0, ForwardAmount);
    // fly-upward the camera.
    if (Input.GetKey(KeyCode.E)) {
      transform.Translate(0, transform.up.y * MovementSpeed * Time.deltaTime, 0);
    }
    // fly-downward the camera.
    if (Input.GetKey(KeyCode.Q)) {
      transform.Translate(0, -transform.up.y * MovementSpeed * Time.deltaTime, 0);
    }
  }

  void MoveVertically() {
    // fly-upward the camera.
    if (Input.GetKey(KeyCode.E)) {
      transform.Translate(0, transform.up.y * MovementSpeed * Time.deltaTime, 0);
    }
    // fly-downward the camera.
    if (Input.GetKey(KeyCode.Q)) {
      transform.Translate(0, -transform.up.y * MovementSpeed * Time.deltaTime, 0);
    }
  }
};