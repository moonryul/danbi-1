using UnityEngine;

public class InformationNote : MonoBehaviour {
  public bool IsReady = true;
  public string TextInfo = "";  

  void Awake() {
    enabled = false;
  }

  public void SwitchToggle() {
    IsReady = !IsReady;
  }
}
