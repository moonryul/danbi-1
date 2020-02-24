using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;

public class KinectAction : MonoBehaviour {
  public int m_iCurrentAction;

  KinectSensorManager kinect;
  KinectConnectBody connectBody;

  public GameObject Target;

  public Text PeopleCount;

  public Material[] Materials;

  [SerializeField]
  GameObject LeftHand, RightHand, Head;
  GameObject[] LeftHands, RightHands, Heads;

  public Body[] BodyArray = null;

  int MaxBodyCount;

  Vector3[] RightSavePosition;
  Vector3[] RighttCurrentPosition;

  Vector3[] LeftSavePosition;
  Vector3[] LeftCurrentPosition;

  // Start is called before the first frame update
  void Start() {
    kinectInit();
    modelInit();
  }
  /////////////////////////////
  //
  // 키넥트 초기화
  //
  void kinectInit() {
    kinect = KinectSensorManager.instance;
    connectBody = GetComponent<KinectConnectBody>();
    MaxBodyCount = kinect.m_Sensor.BodyFrameSource.BodyCount;
    if (BodyArray == null) {
      BodyArray = new Body[MaxBodyCount];
    }
  }
  /////////////////////////////
  //
  // 각종 오브젝트 초기화
  //
  void modelInit() {
    LeftHands = new GameObject[MaxBodyCount];
    RightHands = new GameObject[MaxBodyCount];
    Heads = new GameObject[MaxBodyCount];
    RightSavePosition = new Vector3[MaxBodyCount];
    RighttCurrentPosition = new Vector3[MaxBodyCount];
    LeftSavePosition = new Vector3[MaxBodyCount];
    LeftCurrentPosition = new Vector3[MaxBodyCount];

    for (int i = 0; i < MaxBodyCount; ++i) {
      LeftHands[i] = LeftHand;
      RightHands[i] = RightHand;
      Heads[i] = Head;

      LeftHands[i].GetComponent<MeshRenderer>().sharedMaterial = Materials[i];
      RightHands[i].GetComponent<MeshRenderer>().sharedMaterial = Materials[i];
      Heads[i].GetComponent<MeshRenderer>().sharedMaterial = Materials[i];

      LeftHands[i] = Instantiate(LeftHands[i]);
      RightHands[i] = Instantiate(RightHands[i]);
      Heads[i] = Instantiate(Heads[i]);
    }
  }

  void MirrorJoint() {

  }
  public int currentIndex; // 현재 실행되는 텍스쳐 인덱스

  //////////////////////////////////////////////////////////////////////////////
  //
  // 트래킹을 할 범위인지 판정 (base -> 기준점 , distance -> 거리, radius -> 범위)
  //
  bool TrackBodyCondition(Vector3 _base, float _distance, float _radius) {
    float distance = Vector3.Distance(new Vector3(0, _base.y, _distance), _base);
    Debug.Log(distance);
    if (distance < _radius) {
      return true;
    } else {
      return false;
    }
  }

  void ResetAll(int i) {
    LeftHands[i].transform.position = Vector3.up * 100;
    RightHands[i].transform.position = Vector3.up * 100;
    Heads[i].transform.position = Vector3.up * 100;
  }
  ///////////////////////////////
  //
  // 키넥트 행동 관련 메인 함수
  //
  public void KinectActionUpdate() {
    for (int i = 0; i < MaxBodyCount; ++i) {
      if (!BodyArray[i].IsTracked) {
        ResetAll(i);
        continue;
      } else {
        UpdateBodyPosition(i); // 모든 몸 위치 갱신

        // TODO : 몸 전체가 안들어오면 작동안함. 회전이 부드럽지않음..
        if (TrackBodyCondition(PointToVector3(BodyArray[i].Joints[JointType.SpineBase].Position), TrackingDinstance, TrackingRadius)) // 트래킹 범위 내에 사람이있는지 판단 (1.5미터 거리에 0.15센치반경의 원)
        {
          UpdateTrackingBodyPosition(i); // 트래킹이 된 몸 위치 갱신
          EventExcute(i); // 이벤트 실행
          SaveTrackingBodyPosition(i); // 트래킹이 된 몸 위치 저장
        }
      }
    }
    //PeopleCount.text = count.ToString() + "명";
  }

  void UpdateBodyPosition(int i) {
    LeftHands[i].transform.position = PointToVector3(BodyArray[i].Joints[JointType.HandTipLeft].Position);
    RightHands[i].transform.position = PointToVector3(BodyArray[i].Joints[JointType.HandTipRight].Position);
    Heads[i].transform.position = PointToVector3(BodyArray[i].Joints[JointType.Head].Position);
  }

  void UpdateTrackingBodyPosition(int i) {
    RighttCurrentPosition[i] = RightHands[i].transform.position;
    LeftCurrentPosition[i] = LeftHands[i].transform.position;
  }

  void EventExcute(int i) {
    RotateScreen(RightHands[i], true, i);
    RotateScreen(LeftHands[i], false, i);
  }

  void SaveTrackingBodyPosition(int i) {
    RightSavePosition[i] = RighttCurrentPosition[i];
    LeftSavePosition[i] = LeftCurrentPosition[i];
  }

  [Range(0, 1)]
  public float Sensitive;  // 민감도 -> 높을수록 몸의 움직임에 민감하게 반응

  [Range(0, 1)]
  public float RotationSensitive; // 회전 민감도 -> 높을수록 더 많이 화면이 회전함

  [Range(0, 5)]
  public float TrackingDinstance;

  [Range(0, 0.3f)]
  public float TrackingRadius;

  float RotPower; // 현재 회전 속도
  public Texture[] textures;
  int currentTextureIndex;
  float Timer;

  //////////////////////////////////////////
  //
  // 손을 위로 올릴 시 화면 전환 이벤트
  //
  bool ChangeScreen(GameObject LHand, GameObject RHand, int i) {
    Timer -= Time.fixedDeltaTime;

    if (Timer > 0) {
      RightSavePosition[i] = RighttCurrentPosition[i];
      LeftSavePosition[i] = LeftCurrentPosition[i];
      return true;
    }

    float Rdistance = (RighttCurrentPosition[i] - RightSavePosition[i]).y;
    float Ldistance = (LeftCurrentPosition[i] - LeftSavePosition[i]).y;
    float RdistanceX = Mathf.Abs((RightSavePosition[i] - RighttCurrentPosition[i]).x);
    float LdistanceX = Mathf.Abs((LeftSavePosition[i] - LeftCurrentPosition[i]).x);
    if (Rdistance > 0.3f * (1 - Sensitive) || Ldistance > 0.3f * (1 - Sensitive) && RdistanceX < 0.05f * (1 - Sensitive) && LdistanceX < 0.05f * (1 - Sensitive)) {
      Timer = 0.3f;
      currentTextureIndex++;
      currentTextureIndex = currentTextureIndex % textures.Length;
      Target.GetComponent<MeshRenderer>().material.mainTexture = textures[currentTextureIndex];

      RightSavePosition[i] = RighttCurrentPosition[i];
      LeftSavePosition[i] = LeftCurrentPosition[i];
      return true;
    }
    return false;

  }
  /////////////////////////////////////////////////
  //
  // 손을 좌우로 이동시 화면 회전 // dir -> 회전 방향
  //
  void RotateScreen(GameObject Hand, bool dir, int i) {

    if (dir) {
      RighttCurrentPosition[i] = Hand.transform.position;
      float distance = (RightSavePosition[i] - RighttCurrentPosition[i]).x;

      if (distance > 0.15f * (1 - Sensitive)) {
        RotPower += distance * 4000 * RotationSensitive;
      }

      RightSavePosition[i] = RighttCurrentPosition[i];
    } else {
      LeftCurrentPosition[i] = Hand.transform.position;
      float distance = (LeftSavePosition[i] - LeftCurrentPosition[i]).x;
      if (distance < -0.15f * (1 - Sensitive)) {
        RotPower += distance * 4000 * RotationSensitive;
      }
    }
  }

  private void FixedUpdate() {
    RotPower = Mathf.Lerp(RotPower, 0, Time.fixedDeltaTime * 3);

    Target.transform.Rotate(new Vector3(0.0f, Time.fixedDeltaTime * RotPower, 0.0f));
  }

  Vector3 PointToVector3(CameraSpacePoint p) => new Vector3(p.X, p.Y, p.Z);

}
