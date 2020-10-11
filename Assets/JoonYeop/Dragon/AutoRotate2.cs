using UnityEngine;

public class AutoRotate2 : MonoBehaviour {
  // Start is called before the first frame update
  void Start() {

  }

  // Update is called once per frame
  void Update() {
    transform.Rotate(new Vector3(0, Time.deltaTime, 0) * 12);
  }
}
