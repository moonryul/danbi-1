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
    public class DanbiUIProjectorInternalParametersPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiCameraInternalData internalData = new DanbiCameraInternalData();

        [Readonly]
        public bool useInternalParameters = false;

        [Readonly]
        public string loadPath;

        [Readonly]
        public string savePath;

        string playerPrefsKeyRoot = "ProjectorInternalParameters-";

        protected override void SaveValues()
        {
            PlayerPrefs.SetInt(playerPrefsKeyRoot + "useInternalParameters", useInternalParameters == true ? 1 : 0);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-X", internalData.radialCoefficientX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", internalData.radialCoefficientY);
            // PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", internalData.radialCoefficientZ);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", internalData.tangentialCoefficientX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", internalData.tangentialCoefficientY);
            // PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Z", internalData.tangentialCoefficientZ);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-X", internalData.principalPointX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-Y", internalData.principalPointY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-X", internalData.focalLengthX);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-Y", internalData.focalLengthY);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "skewCoefficient", internalData.skewCoefficient);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            bool prevUseInternalParamters = PlayerPrefs.GetInt(playerPrefsKeyRoot + "useInternalParameters", default) == 1;
            useInternalParameters = prevUseInternalParamters;
            (uiElements[0] as Toggle).isOn = prevUseInternalParamters;

            (uiElements[1] as Button).interactable = prevUseInternalParamters;

            float prevRadialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-X", default);
            internalData.radialCoefficientX = prevRadialCoefficientX;
            (uiElements[2] as InputField).text = prevRadialCoefficientX.ToString();

            float prevRadialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", default);
            internalData.radialCoefficientY = prevRadialCoefficientY;
            (uiElements[3] as InputField).text = prevRadialCoefficientY.ToString();

            // float prevRadialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", default);
            // internalData.radialCoefficientZ = prevRadialCoefficientZ;
            // (uiElements[4] as InputField).text = prevRadialCoefficientZ.ToString();

            float prevTangentialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", default);
            internalData.tangentialCoefficientX = prevTangentialCoefficientX;
            (uiElements[4] as InputField).text = prevTangentialCoefficientX.ToString();

            float prevTangentialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", default);
            internalData.tangentialCoefficientY = prevTangentialCoefficientY;
            (uiElements[5] as InputField).text = prevTangentialCoefficientY.ToString();

            // float prevTangentialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentailCoefficient-Z", default);
            // internalData.tangentialCoefficientZ = prevTangentialCoefficientZ;
            // (uiElements[7] as InputField).text = prevTangentialCoefficientZ.ToString();

            float prevPrincipalPointX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-X", default);
            internalData.principalPointX = prevPrincipalPointX;
            (uiElements[6] as InputField).text = prevPrincipalPointX.ToString();

            float prevPrincipalPointY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-Y", default);
            internalData.principalPointY = prevPrincipalPointY;
            (uiElements[7] as InputField).text = prevPrincipalPointY.ToString();

            float prevFocalLengthX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-X", default);
            internalData.focalLengthX = prevFocalLengthX;
            (uiElements[8] as InputField).text = prevFocalLengthX.ToString();

            float prevFocalLengthY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-Y", default);
            internalData.focalLengthY = prevFocalLengthY;
            (uiElements[9] as InputField).text = prevFocalLengthY.ToString();

            float prevSkewCoefficient = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "skewCoefficient", default);
            internalData.skewCoefficient = prevSkewCoefficient;
            (uiElements[10] as InputField).text = prevSkewCoefficient.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var parentSize = transform.parent.GetComponent<RectTransform>().rect;
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 150);

            var panel = Panel.transform;
            var elements = new Dictionary<string, Selectable>();

            // 1. bind the "Internal parameters" toggle.
            var useInternalParametersToggle = panel.GetChild(0).GetComponent<Toggle>();
            useInternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useInternalParameters = isOn;
                    elements.ToList().ForEach(pair => pair.Value.interactable = isOn);
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );
            elements.Add("useInternalParametersToggle", useInternalParametersToggle);

            // 2. bind the select camera Internal parameter button.
            var selectCameraInternalParameterButton = panel.GetChild(1).GetComponent<Button>();
            selectCameraInternalParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_LoadCameraInternalParametersSelector(panel, elements)));
            elements.Add("selectCameraInternalParametersButton", selectCameraInternalParameterButton);

            // 3. bind the "Internal parameters" buttons.
            #region Radial Coefficient
            // bind the radial Coefficient X
            var radialCoefficient = panel.GetChild(3);

            var radialCoefficientXInputField = radialCoefficient.GetChild(0).GetComponent<InputField>();
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.radialCoefficientX = asFloat;
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
                        internalData.radialCoefficientY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("radialCoefficientY", radialCoefficientYInputField);

            // bind the radial Coefficient Z
            // var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<InputField>();
            // radialCoefficientZInputField.onValueChanged.AddListener(
            //     (string val) =>
            //     {
            //         if (float.TryParse(val, out var asFloat))
            //         {
            //             internalData.radialCoefficientZ = asFloat;
            //             DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
            //         }
            //     }
            // );
            // elements.Add("radialCoefficientZ", radialCoefficientZInputField);
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
                        internalData.tangentialCoefficientX = asFloat;
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
                        internalData.tangentialCoefficientY = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("tangentialCoefficientY", tangentialCoefficientYInputField);

            // bind the tangential Coefficient Z
            // var tangentialCoefficientZInputField = tangentialCoefficient.GetChild(2).GetComponent<InputField>();
            // tangentialCoefficientZInputField.onValueChanged.AddListener(
            //     (string val) =>
            //     {
            //         if (float.TryParse(val, out var asFloat))
            //         {
            //             internalData.tangentialCoefficientZ = asFloat;
            //             DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
            //         }
            //     }
            // );
            // elements.Add("tangentialCoefficientZ", tangentialCoefficientZInputField);
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
                        internalData.principalPointX = asFloat;
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
                        internalData.principalPointY = asFloat;
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
                        internalData.focalLengthX = asFloat;
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
                        internalData.focalLengthY = asFloat;
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
                        internalData.skewCoefficient = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("skewCoefficient", skewCoefficientInputField);
            #endregion Focal Length / Skew Coefficient

            // bind the create camera Internal parameter button.
            var saveCameraInternalParameterButton = panel.GetChild(8).GetComponent<Button>();
            saveCameraInternalParameterButton.onClick.AddListener(() => StartCoroutine(Coroutine_SaveNewCameraInternalParameter()));
            elements.Add("saveCameraInternalParam", saveCameraInternalParameterButton);

            LoadPreviousValues(elements.Values.ToArray());
            useInternalParametersToggle.interactable = true;
        }

        IEnumerator Coroutine_SaveNewCameraInternalParameter()
        {
            var filters = new string[] { ".dat", ".DAT" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileSys.OpenSaveDialog(startingPath,
                                                     filters,
                                                     "Save Camera Internal Parameters",
                                                     "Save");
            // forward the path to save the Internal parameters as a file.
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);

            var bf = new BinaryFormatter();
            var file = File.Open(savePath, FileMode.OpenOrCreate);
            bf.Serialize(file, internalData);
            file.Close();
        }

        IEnumerator Coroutine_LoadCameraInternalParametersSelector(Transform panel, Dictionary<string, Selectable> elements)
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

            // Load the Internal parameters.            
            var loaded = default(DanbiCameraInternalData);
            if (File.Exists(loadPath))
            {
                var bf = new BinaryFormatter();
                var file = File.Open(loadPath, FileMode.Open);
                loaded = bf.Deserialize(file) as DanbiCameraInternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));
            internalData = loaded;

            panel.GetChild(2).GetComponent<Text>().text = loadPath;

            (elements["radialCoefficientX"] as InputField).text = internalData.radialCoefficientX.ToString();
            (elements["radialCoefficientY"] as InputField).text = internalData.radialCoefficientY.ToString();
            // (elements["radialCoefficientZ"] as InputField).text = internalData.radialCoefficientZ.ToString();

            (elements["tangentialCoefficientX"] as InputField).text = internalData.tangentialCoefficientX.ToString();
            (elements["tangentialCoefficientY"] as InputField).text = internalData.tangentialCoefficientY.ToString();
            // (elements["tangentialCoefficientZ"] as InputField).text = internalData.tangentialCoefficientZ.ToString();

            (elements["principalPointX"] as InputField).text = internalData.principalPointX.ToString();
            (elements["principalPointY"] as InputField).text = internalData.principalPointY.ToString();

            (elements["focalLengthX"] as InputField).text = internalData.focalLengthX.ToString();
            (elements["focalLengthY"] as InputField).text = internalData.focalLengthY.ToString();

            (elements["skewCoefficient"] as InputField).text = internalData.skewCoefficient.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

    };
};
