
using UnityEngine;

public class FollowPos : MonoBehaviour {
  public RecordPos record;



  // Update is called once per frame
  void Update() {
    int oldframe = record.frameCount + 1;
    oldframe = oldframe % 49;
    transform.position = record.posArray[oldframe];
  }
}
