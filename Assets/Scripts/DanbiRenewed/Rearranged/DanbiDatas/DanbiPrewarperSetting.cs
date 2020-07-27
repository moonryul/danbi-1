using UnityEngine;

public class DanbiPrewarperSetting : MonoBehaviour {  
  [SerializeField, Readonly]
  string PrewarperType;
  public string prewarperType {get => PrewarperType; set => PrewarperType = value;}



};
