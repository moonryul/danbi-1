using UnityEngine;

namespace Danbi {
  public class DanbiPrewarperSetting : MonoBehaviour {
    [SerializeField, Readonly]
    string PrewarperType;
    public string prewarperType { get => PrewarperType; set => PrewarperType = value; }

    [SerializeField]
    DanbiCameraInternalParameters CamParams;
    public DanbiCameraInternalParameters camParams { get => CamParams; set => CamParams = value; }
  };
};
