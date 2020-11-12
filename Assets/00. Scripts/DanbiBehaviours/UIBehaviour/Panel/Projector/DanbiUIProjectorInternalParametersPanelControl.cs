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
    public class DanbiUIProjectorInternalParametersPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiCameraInternalData internalData = new DanbiCameraInternalData();

        [Readonly]
        public string loadPath;

        [Readonly]
        public string savePath;

        string playerPrefsKeyRoot = "ProjectorInternalParameters-";

        TMP_Text m_selectedPanoramaInternalParameterText;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString(playerPrefsKeyRoot + "loadPath", loadPath);
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
            internalData = bf.Deserialize(file) as DanbiCameraInternalData;

            if (internalData is null)
            {
                return;
            }

            (uiElements[1] as TMP_InputField).text = internalData.radialCoefficientX.ToString();
            (uiElements[2] as TMP_InputField).text = internalData.radialCoefficientY.ToString();
            (uiElements[3] as TMP_InputField).text = internalData.radialCoefficientZ.ToString();

            (uiElements[4] as TMP_InputField).text = internalData.tangentialCoefficientX.ToString();
            (uiElements[5] as TMP_InputField).text = internalData.tangentialCoefficientY.ToString();

            (uiElements[6] as TMP_InputField).text = internalData.principalPointX.ToString();
            (uiElements[7] as TMP_InputField).text = internalData.principalPointY.ToString();

            (uiElements[8] as TMP_InputField).text = internalData.focalLengthX.ToString();
            (uiElements[9] as TMP_InputField).text = internalData.focalLengthY.ToString();

            (uiElements[10] as TMP_InputField).text = internalData.skewCoefficient.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // place the panel a little bit upside.
            var parentSize = transform.parent.GetComponent<RectTransform>().rect;
            Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(parentSize.width, 140);

            // Disable the button at first until using Calibrated Camera!
            GetComponent<Button>().interactable = false;

            DanbiUIProjectorCalibratedPanelControl.onSetUseCalibratedCamera +=
                (bool use) =>
                {
                    GetComponent<Button>().interactable = use;
                };

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
                        if (float.TryParse(val, out var asFloat))
                        {
                            internalData.radialCoefficientX = asFloat;
                            DanbiUISync.onPanelUpdate?.Invoke(this);
                        }
                    }
                );
            elements.Add("radialCoefficientX", radialCoefficientXInputField);

            // 3-2. bind the radial Coefficient Y
            var radialCoefficientYInputField = radialCoefficient.GetChild(1).GetComponent<TMP_InputField>();
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.radialCoefficientY = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("radialCoefficientY", radialCoefficientYInputField);

            // 3-3. bind the radial Coefficient Z
            var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<TMP_InputField>();
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.radialCoefficientZ = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.tangentialCoefficientX = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("tangentialCoefficientX", tangentialCoefficientXInputField);

            // 4-2. bind the tangential Coefficient Y
            var tangentialCoefficientYInputField = tangentialCoefficient.GetChild(1).GetComponent<TMP_InputField>();
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.tangentialCoefficientY = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.principalPointX = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("principalPointX", principalPointXInputField);

            // 5-2. bind the principal point Y
            var principalPointYInputField = principalPoint.GetChild(1).GetComponent<TMP_InputField>();
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.principalPointY = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.focalLengthX = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("focalLengthX", focalLengthXInputField);

            // 6-2. bind the focal length Y
            var focalLengthYInputField = focalLength.GetChild(1).GetComponent<TMP_InputField>();
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.focalLengthY = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("focalLengthY", focalLengthYInputField);

            // 7. bind the skew coefficient
            var skewCoefficientInputField = panel.GetChild(6).GetComponent<TMP_InputField>();
            skewCoefficientInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        internalData.skewCoefficient = asFloat;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
                    }
                }
            );
            elements.Add("skewCoefficient", skewCoefficientInputField);
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
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);

            var bf = new BinaryFormatter();
            using (var file = File.Open(savePath, FileMode.OpenOrCreate))
            {
                bf.Serialize(file, internalData);
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
            DanbiFileSys.GetResourcePathIntact(out loadPath, out _);

            // Load the Internal parameters.            
            if (!File.Exists(loadPath))
            {
                yield break;
            }

            var bf = new BinaryFormatter();
            DanbiCameraInternalData loaded = null;
            using (var file = File.Open(loadPath, FileMode.Open))
            {
                loaded = bf.Deserialize(file) as DanbiCameraInternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));
            internalData = loaded;

            m_selectedPanoramaInternalParameterText.text = loadPath;

            (elements["radialCoefficientX"] as TMP_InputField).text = internalData.radialCoefficientX.ToString();
            (elements["radialCoefficientY"] as TMP_InputField).text = internalData.radialCoefficientY.ToString();
            (elements["radialCoefficientZ"] as TMP_InputField).text = internalData.radialCoefficientZ.ToString();

            (elements["tangentialCoefficientX"] as TMP_InputField).text = internalData.tangentialCoefficientX.ToString();
            (elements["tangentialCoefficientY"] as TMP_InputField).text = internalData.tangentialCoefficientY.ToString();

            (elements["principalPointX"] as TMP_InputField).text = internalData.principalPointX.ToString();
            (elements["principalPointY"] as TMP_InputField).text = internalData.principalPointY.ToString();

            (elements["focalLengthX"] as TMP_InputField).text = internalData.focalLengthX.ToString();
            (elements["focalLengthY"] as TMP_InputField).text = internalData.focalLengthY.ToString();

            (elements["skewCoefficient"] as TMP_InputField).text = internalData.skewCoefficient.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

    };
};
