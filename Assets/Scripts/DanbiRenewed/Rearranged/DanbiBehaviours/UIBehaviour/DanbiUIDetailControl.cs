using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Danbi {
  public class DanbiUIDetailControl : MonoBehaviour {
    [SerializeField, Readonly]
    GameObject[] PfDetails;

    DanbiUIBaseSubmenu[] InstDetails;

    [SerializeField, Readonly]
    Button Button_FinishDetailSetting;

    public delegate void OnDetailMove(int activeIdx);
    public static OnDetailMove Call_OnDetailMove;

    void Awake() {
      // 1. Bind
      Call_OnDetailMove += Caller_OnDetailMove;

      // 2. Load Detail Prefabs.
      PfDetails = new GameObject[5];
      PfDetails[0] = Resources.Load<GameObject>("UIDetails/RoomDetail");
      PfDetails[1] = Resources.Load<GameObject>("UIDetails/PanoramaDetail");
      PfDetails[2] = Resources.Load<GameObject>("UIDetails/ReflectorDetail");
      PfDetails[3] = Resources.Load<GameObject>("UIDetails/CameraDetail");
      PfDetails[4] = Resources.Load<GameObject>("UIDetails/FinalDetail");
      InstDetails = new DanbiUIBaseSubmenu[PfDetails.Length];

      // 3. Get the Reference of the finish button.
      Button_FinishDetailSetting = GetComponentInChildren<Button>();
      Button_FinishDetailSetting.onClick.AddListener(Call_OnFinishDetailSettingClicked);
      Button_FinishDetailSetting.gameObject.SetActive(false);
    }

    void Start() {
      int detailCnt = 0;
      foreach (var i in PfDetails) {
        var inst = Instantiate(i, default(Transform)).transform;
        inst.SetParent(transform);
        InstDetails[detailCnt] = inst.GetComponent<DanbiUIBaseSubmenu>();
        DanbiManager.Instance.RegisterUnloadForSimulator(InstDetails[detailCnt++]);
      }
      Caller_OnDetailMove(0);
    }

    void OnDisable() {
      Call_OnDetailMove -= Caller_OnDetailMove;
    }

    void Caller_OnDetailMove(int activeIdx) {
      // 1. Finish button only appears on the last stage (FinalDetail).
      Button_FinishDetailSetting.gameObject
        .SetActive(activeIdx == 4 ? true : false);

      // 2. Toggle each details according to the activeIdx.
      for (int i = 0; i < InstDetails.Length; ++i) {
        InstDetails[i].gameObject
          .SetActive(i == activeIdx ? true : false);
      }
    }

    void Call_OnFinishDetailSettingClicked() {
      //SceneManager.LoadScene();
    }
  };
}
