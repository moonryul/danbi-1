using System.Collections;
using System.Collections.Generic;

using UnityEngine;
namespace Danbi {
  public class DanbiUIDetailControl : MonoBehaviour {
    [SerializeField, Readonly]
    GameObject[] PfDetails;

    DanbiInitialDetail[] InstDetails;

    public delegate void OnDetailMove(int activeIdx);
    public static OnDetailMove Call_OnDetailMove;

    void Awake() {
      Call_OnDetailMove += Caller_OnDetailMove;

      PfDetails = new GameObject[5];
      PfDetails[0] = Resources.Load<GameObject>("UIDetails/RoomDetail");
      PfDetails[1] = Resources.Load<GameObject>("UIDetails/PanoramaDetail");
      PfDetails[2] = Resources.Load<GameObject>("UIDetails/ReflectorDetail");
      PfDetails[3] = Resources.Load<GameObject>("UIDetails/CameraDetail");
      PfDetails[4] = Resources.Load<GameObject>("UIDetails/FinalDetail");
      InstDetails = new DanbiInitialDetail[PfDetails.Length];
    }

    void Start() {
      int detailCnt = 0;
      foreach (var i in PfDetails) {
        var inst = Instantiate(i, default(Transform)).transform;
        inst.SetParent(transform);
        InstDetails[detailCnt] = inst.GetComponent<DanbiInitialDetail>();
        DanbiManager.Inst.RegisterUnloadForSimulator(InstDetails[detailCnt++]);
      }

      Caller_OnDetailMove(0);
    }

    void OnDisable() {
      Call_OnDetailMove -= Caller_OnDetailMove;
    }

    void Caller_OnDetailMove(int activeIdx) {
      for (int i = 0; i < InstDetails.Length; ++i) {
        InstDetails[i].gameObject.SetActive(i == activeIdx ? true : false);
      }
    }
  };
}
