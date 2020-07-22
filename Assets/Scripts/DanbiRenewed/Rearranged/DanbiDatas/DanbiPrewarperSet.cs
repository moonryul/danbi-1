using UnityEngine;

public class DanbiPrewarperSet : MonoBehaviour {
  [SerializeField]
  EDanbiPrewarperType CurrentPrewarperType = EDanbiPrewarperType.None;
  public EDanbiPrewarperType currentPrewarperType { get => CurrentPrewarperType; set => CurrentPrewarperType = value; }

};
