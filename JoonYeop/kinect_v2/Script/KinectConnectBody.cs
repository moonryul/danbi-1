using UnityEngine;
using Windows.Kinect;
using UnityEngine.UI;

public class KinectConnectBody : MonoBehaviour
{
    KinectSensorManager Kinect;
    BodyFrameReader _bfReader;
    KinectSensor _sensor;

    KinectAction KinectAction;
    


  
    public Text Text_connect;

    

    // Start is called before the first frame update
    void Start()
    {
        Kinect = KinectSensorManager.instance;
        KinectAction = GetComponent<KinectAction>();
        _bfReader = Kinect.m_bfReader;
        _sensor = Kinect.m_Sensor;
    }

    void FixedUpdate()
    {
        if (_bfReader != null)
        {
            //Text_connect.text = "Connect Success";

            //한 프레임 얻는다
            BodyFrame frame = _bfReader.AcquireLatestFrame();
            if (frame != null)
            {
                //몸 데이터 갱신
                frame.GetAndRefreshBodyData(KinectAction.BodyArray);
                KinectAction.KinectActionUpdate();

                //프레임 해제
                frame.Dispose();
                frame = null;

            }
        }
        else
        {
            //Text_connect.text = "Connect Fail";
        }
    }

  
}
