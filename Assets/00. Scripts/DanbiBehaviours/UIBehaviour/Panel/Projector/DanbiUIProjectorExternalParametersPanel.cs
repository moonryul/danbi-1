using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TMPro;
using Unity.Mathematics;

namespace Danbi
{
    public class DanbiUIProjectorExternalParametersPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        DanbiCameraExternalData m_externalData = new DanbiCameraExternalData();

        [SerializeField, Readonly]
        string m_loadPath;

        [SerializeField, Readonly]
        string m_savePath;

        string m_playerPrefsKeyRoot = "ProjectorExternalParameters-";

        TMP_Text m_selectProjectorExternalParameterText;

        public delegate void OnProjectorPositionXUpdate(float x);
        public static OnProjectorPositionXUpdate onProjectorPositionXUpdate;
        public delegate void OnProjectorPositionYUpdate(float y);
        public static OnProjectorPositionYUpdate onProjectorPositionYUpdate;
        public delegate void OnProjectorPositionZUpdate(float z);
        public static OnProjectorPositionZUpdate onProjectorPositionZUpdate;

        public delegate void OnProjectorXAxisXUpdate(float x);
        public static OnProjectorXAxisXUpdate onProjectorXAxisXUpdate;
        public delegate void OnProjectorXAxisYUpdate(float y);
        public static OnProjectorXAxisYUpdate onProjectorXAxisYUpdate;
        public delegate void OnProjectorXAxisZUpdate(float z);
        public static OnProjectorXAxisZUpdate onProjectorXAxisZUpdate;

        public delegate void OnProjectorYAxisXUpdate(float x);
        public static OnProjectorYAxisXUpdate onProjectorYAxisXUpdate;
        public delegate void OnProjectorYAxisYUpdate(float y);
        public static OnProjectorYAxisYUpdate onProjectorYAxisYUpdate;
        public delegate void OnProjectorYAxisZUpdate(float z);
        public static OnProjectorYAxisZUpdate onProjectorYAxisZUpdate;

        public delegate void OnProjectorZAxisXUpdate(float x);
        public static OnProjectorZAxisXUpdate onProjectorZAxisXUpdate;
        public delegate void OnProjectorZAxisYUpdate(float y);
        public static OnProjectorZAxisYUpdate onProjectorZAxisYUpdate;
        public delegate void OnProjectorZAxisZUpdate(float z);
        public static OnProjectorZAxisZUpdate onProjectorZAxisZUpdate;


        protected override void SaveValues()
        {
            PlayerPrefs.SetString(m_playerPrefsKeyRoot + "loadPath", m_loadPath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevLoadPath = PlayerPrefs.GetString(m_playerPrefsKeyRoot + "loadPath", default);
            if (string.IsNullOrEmpty(prevLoadPath))
            {
                return;
            }

            // Load the previous Internal parameters.            
            if (!File.Exists(prevLoadPath))
            {
                return;
            }

            var bf = new BinaryFormatter();
            var file = File.Open(prevLoadPath, FileMode.Open);
            m_externalData = bf.Deserialize(file) as DanbiCameraExternalData;

            if (m_externalData is null)
            {
                return;
            }

            (uiElements[1] as TMP_InputField).text = m_externalData.projectorPosition.x.ToString();
            onProjectorPositionXUpdate?.Invoke(m_externalData.projectorPosition.x);

            (uiElements[2] as TMP_InputField).text = m_externalData.projectorPosition.y.ToString();
            onProjectorPositionYUpdate?.Invoke(m_externalData.projectorPosition.y);

            (uiElements[3] as TMP_InputField).text = m_externalData.projectorPosition.z.ToString();
            onProjectorPositionZUpdate?.Invoke(m_externalData.projectorPosition.z);

            (uiElements[4] as TMP_InputField).text = m_externalData.xAxis.x.ToString();
            onProjectorXAxisXUpdate?.Invoke(m_externalData.xAxis.x);

            (uiElements[5] as TMP_InputField).text = m_externalData.xAxis.y.ToString();
            onProjectorXAxisYUpdate?.Invoke(m_externalData.xAxis.y);

            (uiElements[6] as TMP_InputField).text = m_externalData.xAxis.z.ToString();
            onProjectorXAxisZUpdate?.Invoke(m_externalData.xAxis.z);

            (uiElements[7] as TMP_InputField).text = m_externalData.yAxis.x.ToString();
            onProjectorYAxisXUpdate?.Invoke(m_externalData.yAxis.x);

            (uiElements[8] as TMP_InputField).text = m_externalData.yAxis.y.ToString();
            onProjectorYAxisYUpdate?.Invoke(m_externalData.yAxis.y);

            (uiElements[9] as TMP_InputField).text = m_externalData.yAxis.z.ToString();
            onProjectorYAxisZUpdate?.Invoke(m_externalData.yAxis.z);

            (uiElements[10] as TMP_InputField).text = m_externalData.zAxis.x.ToString();
            onProjectorZAxisXUpdate?.Invoke(m_externalData.zAxis.x);

            (uiElements[11] as TMP_InputField).text = m_externalData.zAxis.y.ToString();
            onProjectorZAxisYUpdate?.Invoke(m_externalData.zAxis.y);

            (uiElements[12] as TMP_InputField).text = m_externalData.zAxis.z.ToString();
            onProjectorZAxisZUpdate?.Invoke(m_externalData.zAxis.z);
        }

        void Awake()
        {
            DanbiUIProjectorCalibratedPanel.onUseCalibratedCamera +=
                (bool use) =>
                {
                    GetComponent<Button>().interactable = use;
                };
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // place the panel a little bit upside.
            var parentSize = transform.parent.GetComponent<RectTransform>().rect;
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 200);

            var panel = Panel.transform;
            var elements = new Dictionary<string, Selectable>();

            // 1. bind select projector external parameter button.
            var selectProjectorExternalParameterButton = panel.GetChild(0).GetComponent<Button>();
            selectProjectorExternalParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_LoadProjectorExternalParameters(panel, elements)));
            elements.Add("selectProjectorExternalParameterButton", selectProjectorExternalParameterButton);

            // 2. bind the selected projector external parameter button.
            m_selectProjectorExternalParameterText = panel.GetChild(1).GetComponent<TMP_Text>();
            m_selectProjectorExternalParameterText.text = "---";

            // 3. bind the projector positions
            var projectorPosition = panel.GetChild(2);

            #region projector positions            

            // 3-1. bind the projector position x
            var positionXInputField = projectorPosition.GetChild(0).GetComponent<TMP_InputField>();
            positionXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.projectorPosition.x = result;
                        onProjectorPositionXUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("positionXInputField", positionXInputField);

            // 3-2. bind the projector position y
            var positionYInputField = projectorPosition.GetChild(1).GetComponent<TMP_InputField>();
            positionYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.projectorPosition.y = result;
                        onProjectorPositionYUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("positionYInputField", positionYInputField);

            // 3-3. bind the projector position z
            var positionZInputField = projectorPosition.GetChild(2).GetComponent<TMP_InputField>();
            positionZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.projectorPosition.z = result;
                        onProjectorPositionZUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("positionZInputField", positionZInputField);

            #endregion projector positions

            // 4. bind the x axis
            var xAxis = panel.GetChild(3);

            #region x axis

            // 4-1. bind the x of xAxis
            var xOfxAxisInputField = xAxis.GetChild(0).GetComponent<TMP_InputField>();
            xOfxAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out float result))
                    {
                        m_externalData.xAxis.x = result;
                        onProjectorXAxisXUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("xOfxAxisInputField", xOfxAxisInputField);

            // 4-2. bind the y of xAxis
            var yOfxAxisInputField = xAxis.GetChild(1).GetComponent<TMP_InputField>();
            yOfxAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.xAxis.y = result;
                        onProjectorXAxisYUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("yOfxAxisInputField", yOfxAxisInputField);

            // 4-3. bind the z of xAxis
            var zOfxAxisInputField = xAxis.GetChild(2).GetComponent<TMP_InputField>();
            zOfxAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.xAxis.z = result;
                        onProjectorXAxisZUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("zOfxAxisInputField", zOfxAxisInputField);

            #endregion x axis

            // 5. bind the y axis
            var yAxis = panel.GetChild(4);

            #region y axis            

            // 5-1. bind the x of y axis
            var xOfyAxisInputField = yAxis.GetChild(0).GetComponent<TMP_InputField>();
            xOfyAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.yAxis.x = result;
                        onProjectorYAxisXUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("xOfyAxisInputField", xOfyAxisInputField);

            // 5-2. bind the y of y axis
            var yOfyAxisInputField = yAxis.GetChild(1).GetComponent<TMP_InputField>();
            yOfyAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.yAxis.y = result;
                        onProjectorYAxisYUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("yOfyAxisInputField", yOfyAxisInputField);

            // 5-3. bind the z of y axis
            var zOfyAxisInputField = yAxis.GetChild(2).GetComponent<TMP_InputField>();
            zOfyAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.yAxis.z = result;
                        onProjectorYAxisZUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("zOfyAxisInputField", zOfyAxisInputField);

            #endregion y axis

            // 6. bind the z axis
            var zAxis = panel.GetChild(5);

            #region z axis            

            // 6-1. bind the x of z axis
            var xOfzAxisInputField = zAxis.GetChild(0).GetComponent<TMP_InputField>();
            xOfzAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.zAxis.x = result;
                        onProjectorZAxisXUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("xOfzAxisInputField", xOfzAxisInputField);

            // 6-2. bind the y of z axis
            var yOfzAxisInputField = zAxis.GetChild(1).GetComponent<TMP_InputField>();
            yOfzAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.zAxis.y = result;
                        onProjectorZAxisYUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("yOfzAxisInputField", yOfzAxisInputField);

            // 6-3. bind the z of z axis
            var zOfzAxisInputField = zAxis.GetChild(2).GetComponent<TMP_InputField>();
            zOfzAxisInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var result))
                    {
                        m_externalData.zAxis.z = result;
                        onProjectorZAxisZUpdate?.Invoke(result);
                    }
                }
            );
            elements.Add("zOfzAxisInputField", zOfzAxisInputField);

            #endregion z axis

            // 7. bind save projector external parameters button
            var saveProjectorExternalParametersButton = panel.GetChild(6).GetComponent<Button>();
            saveProjectorExternalParametersButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveProjectorExtneralParameters()));
            elements.Add("saveProjectorExternalParametersButton", saveProjectorExternalParametersButton);

            LoadPreviousValues(elements.Values.ToArray());
        }

        IEnumerator Coroutine_SaveProjectorExtneralParameters()
        {
            // https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
            var filters = new string[] { ".dat" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
            yield return DanbiFileSys.OpenSaveDialog(startingPath,
                                                     filters,
                                                     "Save Camera Internal Parameters",
                                                     "Save");

            // forward the path to save the Internal parameters as a file.
            DanbiFileSys.GetResourcePathIntact(out m_savePath, out _);

            var bf = new BinaryFormatter();
            using (var file = File.Open(m_savePath, FileMode.OpenOrCreate))
            {
                bf.Serialize(file, m_externalData);
            }            
        }

        IEnumerator Coroutine_LoadProjectorExternalParameters(Transform panel, Dictionary<string, Selectable> elements)
        {
            var filters = new string[] { ".dat" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
            yield return DanbiFileSys.OpenLoadDialog(startingPath, filters, "Load Camera Externel Paramaters (Scriptable Object)", "Select");
            DanbiFileSys.GetResourcePathIntact(out m_loadPath, out _);

            // Load the Internal parameters.            
            if (!File.Exists(m_loadPath))
            {
                yield break;
            }

            var bf = new BinaryFormatter();
            DanbiCameraExternalData loaded = null;
            using (var file = File.Open(m_loadPath, FileMode.Open))
            {
                loaded = bf.Deserialize(file) as DanbiCameraExternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));
            m_externalData = loaded;

            m_selectProjectorExternalParameterText.text = m_loadPath;

            (elements["positionXInputField"] as TMP_InputField).text = m_externalData.projectorPosition.x.ToString();
            onProjectorPositionXUpdate?.Invoke(m_externalData.projectorPosition.x);

            (elements["positionYInputField"] as TMP_InputField).text = m_externalData.projectorPosition.y.ToString();
            onProjectorPositionYUpdate?.Invoke(m_externalData.projectorPosition.y);

            (elements["positionZInputField"] as TMP_InputField).text = m_externalData.projectorPosition.z.ToString();
            onProjectorPositionZUpdate?.Invoke(m_externalData.projectorPosition.z);

            (elements["xOfxAxisInputField"] as TMP_InputField).text = m_externalData.xAxis.x.ToString();
            onProjectorXAxisXUpdate?.Invoke(m_externalData.xAxis.x);

            (elements["yOfxAxisInputField"] as TMP_InputField).text = m_externalData.xAxis.y.ToString();
            onProjectorXAxisYUpdate?.Invoke(m_externalData.xAxis.y);

            (elements["zOfxAxisInputField"] as TMP_InputField).text = m_externalData.xAxis.z.ToString();
            onProjectorXAxisZUpdate?.Invoke(m_externalData.xAxis.z);

            (elements["xOfyAxisInputField"] as TMP_InputField).text = m_externalData.yAxis.x.ToString();
            onProjectorYAxisXUpdate?.Invoke(m_externalData.yAxis.x);

            (elements["yOfyAxisInputField"] as TMP_InputField).text = m_externalData.yAxis.y.ToString();
            onProjectorYAxisYUpdate?.Invoke(m_externalData.yAxis.y);

            (elements["zOfyAxisInputField"] as TMP_InputField).text = m_externalData.yAxis.z.ToString();
            onProjectorYAxisZUpdate?.Invoke(m_externalData.yAxis.z);

            (elements["xOfzAxisInputField"] as TMP_InputField).text = m_externalData.zAxis.x.ToString();
            onProjectorZAxisXUpdate?.Invoke(m_externalData.zAxis.x);

            (elements["yOfzAxisInputField"] as TMP_InputField).text = m_externalData.zAxis.y.ToString();
            onProjectorZAxisYUpdate?.Invoke(m_externalData.zAxis.y);

            (elements["zOfzAxisInputField"] as TMP_InputField).text = m_externalData.zAxis.z.ToString();
            onProjectorZAxisZUpdate?.Invoke(m_externalData.zAxis.z);
        }
    };
};