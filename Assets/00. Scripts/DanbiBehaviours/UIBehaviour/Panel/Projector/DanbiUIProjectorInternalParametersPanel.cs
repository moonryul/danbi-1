using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TMPro;

namespace Danbi
{
    public class DanbiUIProjectorInternalParametersPanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        DanbiCameraInternalData m_internalData = new DanbiCameraInternalData();

        [SerializeField, Readonly]
        string m_loadPath;

        [SerializeField, Readonly]
        string m_savePath;

        string playerPrefsKeyRoot = "ProjectorInternalParameters-";

        TMP_Text m_selectedPanoramaInternalParameterText;

        public delegate void OnRadialCoefficientXUpdate(float x);
        public static OnRadialCoefficientXUpdate onRadialCoefficientXUpdate;
        public delegate void OnRadialCoefficientYUpdate(float y);
        public static OnRadialCoefficientYUpdate onRadialCoefficientYUpdate;
        public delegate void OnRadialCoefficientZUpdate(float z);
        public static OnRadialCoefficientZUpdate onRadialCoefficientZUpdate;

        public delegate void OnTangentialCoefficientXUpdate(float x);
        public static OnTangentialCoefficientXUpdate onTangentialCoefficientXUpdate;
        public delegate void OnTangentialCoefficientYUpdate(float y);
        public static OnTangentialCoefficientYUpdate onTangentialCoefficientYUpdate;

        public delegate void OnPrincipalPointXUpdate(float x);
        public static OnPrincipalPointXUpdate onPrincipalPointXUpdate;
        public delegate void OnPrincipalPointYUpdate(float y);
        public static OnPrincipalPointYUpdate onPrincipalPointYUpdate;

        public delegate void OnFocalLengthXUpdate(float x);
        public static OnFocalLengthXUpdate onFocalLengthXUpdate;
        public delegate void OnFocalLengthYUpdate(float y);
        public static OnFocalLengthYUpdate onFocalLengthYUpdate;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString(playerPrefsKeyRoot + "loadPath", m_loadPath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevLoadPath = PlayerPrefs.GetString(playerPrefsKeyRoot + "loadPath", default);
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
            m_internalData = bf.Deserialize(file) as DanbiCameraInternalData;

            if (m_internalData is null)
            {
                return;
            }

            (uiElements[1] as TMP_InputField).text = m_internalData.radialCoefficient.x.ToString();
            onRadialCoefficientXUpdate?.Invoke(m_internalData.radialCoefficient.x);

            (uiElements[2] as TMP_InputField).text = m_internalData.radialCoefficient.y.ToString();
            onRadialCoefficientYUpdate?.Invoke(m_internalData.radialCoefficient.y);

            (uiElements[3] as TMP_InputField).text = m_internalData.radialCoefficient.z.ToString();
            onRadialCoefficientZUpdate?.Invoke(m_internalData.radialCoefficient.z);

            (uiElements[4] as TMP_InputField).text = m_internalData.tangentialCoefficient.x.ToString();
            onTangentialCoefficientXUpdate?.Invoke(m_internalData.tangentialCoefficient.x);

            (uiElements[5] as TMP_InputField).text = m_internalData.tangentialCoefficient.y.ToString();
            onTangentialCoefficientYUpdate?.Invoke(m_internalData.tangentialCoefficient.y);

            (uiElements[6] as TMP_InputField).text = m_internalData.principalPoint.x.ToString();
            onPrincipalPointXUpdate?.Invoke(m_internalData.principalPoint.x);

            (uiElements[7] as TMP_InputField).text = m_internalData.principalPoint.y.ToString();
            onPrincipalPointYUpdate?.Invoke(m_internalData.principalPoint.y);

            (uiElements[8] as TMP_InputField).text = m_internalData.focalLength.x.ToString();
            onFocalLengthXUpdate?.Invoke(m_internalData.focalLength.x);

            (uiElements[9] as TMP_InputField).text = m_internalData.focalLength.y.ToString();
            onFocalLengthYUpdate?.Invoke(m_internalData.focalLength.y);
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
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 140);

            var panel = Panel.transform;
            var elements = new Dictionary<string, Selectable>();

            // 1. bind the select projector internal parameter button.
            var selectProjectorInternalParameterButton = panel.GetChild(0).GetComponent<Button>();
            selectProjectorInternalParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_LoadProjectorInternalParameters(panel, elements)));
            elements.Add("selectProjectorInternalParameterButton", selectProjectorInternalParameterButton);

            // 2. bind the selected projector internal parameter button.
            m_selectedPanoramaInternalParameterText = panel.GetChild(1).GetComponent<TMP_Text>();
            m_selectedPanoramaInternalParameterText.text = "---";

            // 3. bind the radial Coefficients
            var radialCoefficient = panel.GetChild(2);

            #region Radial Coefficient

            // 3-1. bind the radial Coefficient X
            var radialCoefficientXInputField = radialCoefficient.GetChild(0).GetComponent<TMP_InputField>();
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                    {
                        if (float.TryParse(val, out var res))
                        {
                            m_internalData.radialCoefficient.x = res;
                            onRadialCoefficientXUpdate?.Invoke(res);
                        }
                    }
                );
            elements.Add("radialCoefficientX", radialCoefficientXInputField);

            // 3-2. bind the radial Coefficient Y
            var radialCoefficientYInputField = radialCoefficient.GetChild(1).GetComponent<TMP_InputField>();
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.radialCoefficient.y = res;
                        onRadialCoefficientYUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("radialCoefficientY", radialCoefficientYInputField);

            // 3-3. bind the radial Coefficient Z
            var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<TMP_InputField>();
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.radialCoefficient.z = res;
                        onRadialCoefficientZUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("radialCoefficientZ", radialCoefficientZInputField);

            #endregion Radial Coefficient

            // 4. bind the tangential Coefficients
            var tangentialCoefficient = panel.GetChild(3);

            #region Tangential Coefficient            

            // 4-1. bind the tangential Coefficient X
            var tangentialCoefficientXInputField = tangentialCoefficient.GetChild(0).GetComponent<TMP_InputField>();
            tangentialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.tangentialCoefficient.x = res;
                        onTangentialCoefficientXUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("tangentialCoefficientX", tangentialCoefficientXInputField);

            // 4-2. bind the tangential Coefficient Y
            var tangentialCoefficientYInputField = tangentialCoefficient.GetChild(1).GetComponent<TMP_InputField>();
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.tangentialCoefficient.y = res;
                        onTangentialCoefficientYUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("tangentialCoefficientY", tangentialCoefficientYInputField);

            #endregion Tangential Coefficient

            // 5. bind the principal point
            var principalPoint = panel.GetChild(4);

            #region Principal Point            

            // 5-1. bind the principal point X
            var principalPointXInputField = principalPoint.GetChild(0).GetComponent<TMP_InputField>();
            principalPointXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.principalPoint.x = res;
                        onPrincipalPointXUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("principalPointX", principalPointXInputField);

            // 5-2. bind the principal point Y
            var principalPointYInputField = principalPoint.GetChild(1).GetComponent<TMP_InputField>();
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.principalPoint.y = res;
                        onPrincipalPointYUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("principalPointY", principalPointYInputField);

            #endregion Principal Point

            // 6. bind the focal length
            var focalLength = panel.GetChild(5);

            #region Focal Length / Skew Coefficient            

            // 6-1. bind the focal length X
            var focalLengthXInputField = focalLength.GetChild(0).GetComponent<TMP_InputField>();
            focalLengthXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.focalLength.x = res;
                        onFocalLengthXUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("focalLengthX", focalLengthXInputField);

            // 6-2. bind the focal length Y
            var focalLengthYInputField = focalLength.GetChild(1).GetComponent<TMP_InputField>();
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var res))
                    {
                        m_internalData.focalLength.y = res;
                        onFocalLengthYUpdate?.Invoke(res);
                    }
                }
            );
            elements.Add("focalLengthY", focalLengthYInputField);

            // 7. bind the skew coefficient
            // var skewCoefficientInputField = panel.GetChild(6).GetComponent<TMP_InputField>();
            // skewCoefficientInputField.onValueChanged.AddListener(
            //     (string val) =>
            //     {
            //         if (float.TryParse(val, out var res))
            //         {
            //             m_internalData.sk = res;
            //             DanbiUISync.onPanelUpdate?.Invoke(this);
            //         }
            //     }
            // );
            // elements.Add("skewCoefficient", skewCoefficientInputField);
            #endregion Focal Length / Skew Coefficient

            // 8. bind the create camera Internal parameter button.
            var saveCameraParameterButton = panel.GetChild(7).GetComponent<Button>();
            saveCameraParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveProjectorInternalParameters()));
            elements.Add("saveCameraParam", saveCameraParameterButton);

            LoadPreviousValues(elements.Values.ToArray());
        }

        IEnumerator Coroutine_SaveProjectorInternalParameters()
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
                                                     "Save Projector Internal Parameters",
                                                     "Save");

            // forward the path to save the Internal parameters as a file.
            DanbiFileSys.GetResourcePathIntact(out m_savePath, out _);

            var bf = new BinaryFormatter();
            using (var file = File.Open(m_savePath, FileMode.OpenOrCreate))
            {
                bf.Serialize(file, m_internalData);
            }

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        IEnumerator Coroutine_LoadProjectorInternalParameters(Transform panel, Dictionary<string, Selectable> elements)
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
            DanbiCameraInternalData loaded = null;
            using (var file = File.Open(m_loadPath, FileMode.Open))
            {
                loaded = bf.Deserialize(file) as DanbiCameraInternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));
            m_internalData = loaded;

            m_selectedPanoramaInternalParameterText.text = m_loadPath;

            (elements["radialCoefficientX"] as TMP_InputField).text = m_internalData.radialCoefficient.x.ToString();
            onRadialCoefficientXUpdate?.Invoke(m_internalData.radialCoefficient.x);

            (elements["radialCoefficientY"] as TMP_InputField).text = m_internalData.radialCoefficient.y.ToString();
            onRadialCoefficientYUpdate?.Invoke(m_internalData.radialCoefficient.y);

            (elements["radialCoefficientZ"] as TMP_InputField).text = m_internalData.radialCoefficient.z.ToString();
            onRadialCoefficientZUpdate?.Invoke(m_internalData.radialCoefficient.z);

            (elements["tangentialCoefficientX"] as TMP_InputField).text = m_internalData.tangentialCoefficient.x.ToString();
            onTangentialCoefficientXUpdate?.Invoke(m_internalData.tangentialCoefficient.x);

            (elements["tangentialCoefficientY"] as TMP_InputField).text = m_internalData.tangentialCoefficient.y.ToString();
            onTangentialCoefficientYUpdate?.Invoke(m_internalData.tangentialCoefficient.y);

            (elements["principalPointX"] as TMP_InputField).text = m_internalData.principalPoint.x.ToString();
            onPrincipalPointXUpdate?.Invoke(m_internalData.principalPoint.x);

            (elements["principalPointY"] as TMP_InputField).text = m_internalData.principalPoint.y.ToString();
            onPrincipalPointYUpdate?.Invoke(m_internalData.principalPoint.y);

            (elements["focalLengthX"] as TMP_InputField).text = m_internalData.focalLength.x.ToString();
            onFocalLengthXUpdate?.Invoke(m_internalData.focalLength.x);

            (elements["focalLengthY"] as TMP_InputField).text = m_internalData.focalLength.y.ToString();
            onFocalLengthYUpdate?.Invoke(m_internalData.focalLength.y);
        }

    };
};
