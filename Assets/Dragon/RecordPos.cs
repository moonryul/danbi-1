
using UnityEngine;

public class RecordPos : MonoBehaviour {
  public Vector3[] posArray = new Vector3[50];

  public int frameCount;
  private void Start() {
    for (int i = 0; i < 50; ++i) {
      posArray[i] = Vector3.zero;
    }
  }

  private void FixedUpdate() {
    frameCount++;

    if (frameCount > 49) {
      frameCount = 0;
    }

    posArray[frameCount] = transform.position;
  }
}
