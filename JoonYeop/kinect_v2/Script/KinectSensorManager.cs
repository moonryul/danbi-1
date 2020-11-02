using UnityEngine;
using Windows.Kinect;
using UnityEngine.UI;

public class KinectSensorManager : MonoBehaviour
{
    public static KinectSensorManager instance;
    public KinectSensor m_Sensor;
    public BodyFrameReader m_bfReader;

    private void Awake()
    {
        instance = this;

        //키넥트센서, 리더 가져오기
        m_Sensor = KinectSensor.GetDefault();
        if (m_Sensor != null)
        {
            m_bfReader = m_Sensor.BodyFrameSource.OpenReader();
            if (!m_Sensor.IsOpen) m_Sensor.Open();
        }
    }

  
    
   
    void OnApplicationQuit()
    {
        //모든 인스턴스 해제
        if (m_bfReader != null)
        {
            m_bfReader.Dispose();
            m_bfReader = null;
        }
        if (m_Sensor != null)
        {
            if (m_Sensor.IsOpen) m_Sensor.Close();
            m_Sensor = null;
        }
    }
    
}


