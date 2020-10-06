using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Danbi
{
    public class DanbiUIProjectorExternalParametersPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiCameraExternalData externalData = new DanbiCameraExternalData();

        [Readonly]
        public bool useExternalParameters = false;

        [Readonly]
        public string loadPath;

        [Readonly]
        public string savePath;

        string playerPrefsKeyRoot = "ProjectorExternalParameters-";

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt(playerPrefsKeyRoot + "useExternalParameters", useExternalParameters == true ? 1 : 0);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-X", externalData.radialCoefficientX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", externalData.radialCoefficientY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", externalData.radialCoefficientZ);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", externalData.tangentialCoefficientX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", externalData.tangentialCoefficientY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Z", externalData.tangentialCoefficientZ);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-X", externalData.principalPointX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-Y", externalData.principalPointY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-X", externalData.focalLengthX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-Y", externalData.focalLengthY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "skewCoefficient", externalData.skewCoefficient);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            bool prevUseExternalParamters = PlayerPrefs.GetInt(playerPrefsKeyRoot + "useExternalParameters", default) == 1;
            useExternalParameters = prevUseExternalParamters;
            (uiElements[0] as Toggle).isOn = prevUseExternalParamters;

            (uiElements[1] as Button).interactable = prevUseExternalParamters;

            float prevRadialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-X", default);
            externalData.radialCoefficientX = prevRadialCoefficientX;
            (uiElements[2] as InputField).text = prevRadialCoefficientX.ToString();

            float prevRadialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", default);
            externalData.radialCoefficientY = prevRadialCoefficientY;
            (uiElements[3] as InputField).text = prevRadialCoefficientY.ToString();

            float prevRadialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", default);
            externalData.radialCoefficientZ = prevRadialCoefficientZ;
            (uiElements[4] as InputField).text = prevRadialCoefficientZ.ToString();

            float prevTangentialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", default);
            externalData.tangentialCoefficientX = prevTangentialCoefficientX;
            (uiElements[5] as InputField).text = prevTangentialCoefficientX.ToString();

            float prevTangentialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", default);
            externalData.tangentialCoefficientY = prevTangentialCoefficientY;
            (uiElements[6] as InputField).text = prevTangentialCoefficientY.ToString();

            float prevTangentialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentailCoefficient-Z", default);
            externalData.tangentialCoefficientZ = prevTangentialCoefficientZ;
            (uiElements[7] as InputField).text = prevTangentialCoefficientZ.ToString();

            float prevPrincipalPointX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-X", default);
            externalData.principalPointX = prevPrincipalPointX;
            (uiElements[8] as InputField).text = prevPrincipalPointX.ToString();

            float prevPrincipalPointY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-Y", default);
            externalData.principalPointY = prevPrincipalPointY;
            (uiElements[9] as InputField).text = prevPrincipalPointY.ToString();

            float prevFocalLengthX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-X", default);
            externalData.focalLengthX = prevFocalLengthX;
            (uiElements[10] as InputField).text = prevFocalLengthX.ToString();

            float prevFocalLengthY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-Y", default);
            externalData.focalLengthY = prevFocalLengthY;
            (uiElements[11] as InputField).text = prevFocalLengthY.ToString();

            float prevSkewCoefficient = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "skewCoefficient", default);
            externalData.skewCoefficient = prevSkewCoefficient;
            (uiElements[12] as InputField).text = prevSkewCoefficient.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var elements = new Dictionary<string, Selectable>();

            // 1. bind the "External parameters" toggle.
            var useExternalParametersToggle = panel.GetChild(0).GetComponent<Toggle>();
            useExternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useExternalParameters = isOn;
                    elements.ToList().ForEach(pair => pair.Value.interactable = isOn);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );            
            elements.Add("useExternalParametersToggle", useExternalParametersToggle);

            // 2. bind the select camera external parameter button.
            var selectCameraExternalParameterButton = panel.GetChild(1).GetComponent<Button>();
            selectCameraExternalParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_LoadCameraExternalParametersSelector(panel, elements)));
            elements.Add("selectCameraExternalParametersButton", selectCameraExternalParameterButton);

            // 3. bind the "External parameters" buttons.
            #region Radial Coefficient
            // bind the radial Coefficient X
            var radialCoefficient = panel.GetChild(3);

            var radialCoefficientXInputField = radialCoefficient.GetChild(0).GetComponent<InputField>();
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficientX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("radialCoefficientX", radialCoefficientXInputField);

            // bind the radial Coefficient Y
            var radialCoefficientYInputField = radialCoefficient.GetChild(1).GetComponent<InputField>();
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficientY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("radialCoefficientY", radialCoefficientYInputField);

            // bind the radial Coefficient Z
            var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<InputField>();
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficientZ = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("radialCoefficientZ", radialCoefficientZInputField);
            #endregion Radial Coefficient

            #region Tangential Coefficient
            // bind the tangential Coefficient X
            var tangentialCoefficient = panel.GetChild(4);

            var tangentialCoefficientXInputField = tangentialCoefficient.GetChild(0).GetComponent<InputField>();
            tangentialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficientX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("tangentialCoefficientX", tangentialCoefficientXInputField);

            // bind the tangential Coefficient Y
            var tangentialCoefficientYInputField = tangentialCoefficient.GetChild(1).GetComponent<InputField>();
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficientY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("tangentialCoefficientY", tangentialCoefficientYInputField);

            // bind the tangential Coefficient Z
            var tangentialCoefficientZInputField = tangentialCoefficient.GetChild(2).GetComponent<InputField>();
            tangentialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficientZ = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("tangentialCoefficientZ", tangentialCoefficientZInputField);
            #endregion Tangential Coefficient

            #region Principal Point
            // bind the principal point X
            var principalPoint = panel.GetChild(5);

            var principalPointXInputField = principalPoint.GetChild(0).GetComponent<InputField>();
            principalPointXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPointX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("principalPointX", principalPointXInputField);

            // bind the principal point Y
            var principalPointYInputField = principalPoint.GetChild(1).GetComponent<InputField>();
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPointY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("principalPointY", principalPointYInputField);
            #endregion Principal Point

            #region Focal Length / Skew Coefficient
            // bind the focal length X
            var focalLength = panel.GetChild(6);

            var focalLengthXInputField = focalLength.GetChild(0).GetComponent<InputField>();
            focalLengthXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLengthX = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("focalLengthX", focalLengthXInputField);

            // bind the principal point Y
            var focalLengthYInputField = focalLength.GetChild(1).GetComponent<InputField>();
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLengthY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("focalLengthY", focalLengthYInputField);

            // bind the skew coefficient
            var skewCoefficientInputField = panel.GetChild(7).GetComponent<InputField>();
            skewCoefficientInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.skewCoefficient = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("skewCoefficient", skewCoefficientInputField);
            #endregion Focal Length / Skew Coefficient

            // bind the create camera external parameter button.
            var saveCameraExternalParameterButton = panel.GetChild(8).GetComponent<Button>();
            saveCameraExternalParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveNewCameraExternalParameter()));
            elements.Add("saveCameraExternalParam", saveCameraExternalParameterButton);

            LoadPreviousValues(elements.Values.ToArray());
            useExternalParametersToggle.interactable = true;
        }

        IEnumerator Coroutine_SaveNewCameraExternalParameter()
        {
            var filters = new string[] { ".dat", ".DAT" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileSys.OpenSaveDialog(startingPath,
                                                     filters,
                                                     "Save Camera External Parameters",
                                                     "Save");
            // forward the path to save the external parameters as a file.
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);

            var bf = new BinaryFormatter();
            var file = File.Open(savePath, FileMode.OpenOrCreate);
            bf.Serialize(file, externalData);
            file.Close();
        }

        IEnumerator Coroutine_LoadCameraExternalParametersSelector(Transform panel, Dictionary<string, Selectable> elements)
        {
            var filters = new string[] { ".dat", ".DAT" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif
            yield return DanbiFileSys.OpenLoadDialog(startingPath, filters, "Load Camera Externel Paramaters (Scriptable Object)", "Select");
            DanbiFileSys.GetResourcePathIntact(out loadPath, out _);

            // Load the External parameters.            
            var loaded = default(DanbiCameraExternalData);
            if (File.Exists(loadPath))
            {
                var bf = new BinaryFormatter();
                var file = File.Open(loadPath, FileMode.Open);
                loaded = bf.Deserialize(file) as DanbiCameraExternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));
            externalData = loaded;

            panel.GetChild(2).GetComponent<Text>().text = loadPath;

            (elements["radialCoefficientX"] as InputField).text = externalData.radialCoefficientX.ToString();
            (elements["radialCoefficientY"] as InputField).text = externalData.radialCoefficientY.ToString();
            (elements["radialCoefficientZ"] as InputField).text = externalData.radialCoefficientZ.ToString();

            (elements["tangentialCoefficientX"] as InputField).text = externalData.tangentialCoefficientX.ToString();
            (elements["tangentialCoefficientY"] as InputField).text = externalData.tangentialCoefficientY.ToString();
            (elements["tangentialCoefficientZ"] as InputField).text = externalData.tangentialCoefficientZ.ToString();

            (elements["principalPointX"] as InputField).text = externalData.principalPointX.ToString();
            (elements["principalPointY"] as InputField).text = externalData.principalPointY.ToString();

            (elements["focalLengthX"] as InputField).text = externalData.focalLengthX.ToString();
            (elements["focalLengthY"] as InputField).text = externalData.focalLengthY.ToString();

            (elements["skewCoefficient"] as InputField).text = externalData.skewCoefficient.ToString();
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

    };
};
