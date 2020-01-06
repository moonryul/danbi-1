using UnityEngine;
using UnityEngine.Assertions;

public class CameraControl : MonoBehaviour {
  float rot_around_x, rot_around_y;
  bool is_cam_moved = false;
  float original_move_speed;
  float forward, strafe;

  [Header("Camera attributes"), Space(10)]
  public float MinRotationX;
  public float MaxRotationX;
  [Space(5)]
  public float Xsensitivity = 10.0f;
  public float Ysensitivity = 10.0f;
  [Space(5)]
  public float MovementSpeed = 10.0f;

  [Header("Toggle with key 'V'/ Up 'Q', Down 'E'"), Space(10)]
  public bool DoesMoveVerticallyOnly;
  [Space(5)]
  public Transform Target;

  void Start() {
    rot_around_x = transform.eulerAngles.x;
    rot_around_y = transform.eulerAngles.y;
    original_move_speed = MovementSpeed;
    transform.LookAt(Target);

    Assert.IsNotNull(Target, "Look At Target of Camera Control is null!");
  }

  void Update() {
    // V toggles the movement mode.
    if (Input.GetKeyDown(KeyCode.V)) {
      DoesMoveVerticallyOnly = !DoesMoveVerticallyOnly;
      transform.LookAt(Target);
    }
    
    if (!DoesMoveVerticallyOnly) {
      // Fly freely.
      MoveFreely();
    } else {
      // Fly only vertically.
      MoveVertically();
    }
  }

  void MoveFreely() {
    // rotate the camera.
    rot_around_x += Input.GetAxisRaw("Mouse Y") * Xsensitivity;
    rot_around_y += Input.GetAxisRaw("Mouse X") * Ysensitivity;
    rot_around_x = Mathf.Clamp(rot_around_x, MinRotationX, MaxRotationX);
    transform.rotation = Quaternion.Euler(-rot_around_x, rot_around_y, 0);
    // move faster.
    if (Input.GetKey(KeyCode.LeftShift)) {
      MovementSpeed = original_move_speed * 2.0f;
    }
    if (!Input.GetKey(KeyCode.LeftShift) &&
      original_move_speed != MovementSpeed) {
      MovementSpeed = original_move_speed;
    }
    // move the camera.
    forward = Input.GetAxisRaw("Vertical") * MovementSpeed * Time.deltaTime;
    strafe = Input.GetAxisRaw("Horizontal") * MovementSpeed * Time.deltaTime;
    transform.Translate(strafe, 0, forward);
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