using UnityEngine;

public class DanbiPrewarperSet : MonoBehaviour {  
  [SerializeField, Readonly]
  string PrewarperType;
  public string prewarperType {get => PrewarperType; set => PrewarperType = value;}

};
