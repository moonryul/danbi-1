using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using UnityEditor;

namespace Danbi
{
    public class DanbiUIProjectorExternalParametersPanelControl : DanbiUIPanelControl
    {
        DanbiCameraExternalData externalDataAsset;

        [HideInInspector]
        public DanbiCameraExternalData_struct externalData;
        public string savePath { get; set; }
        public string loadPath { get; set; }

        public delegate void OnToggleCalibratedCameraChanged(bool flag);
        public static OnToggleCalibratedCameraChanged Call_OnToggleCalibratedCameraChanged;
        public static bool UseCalibratedCamera = false;
        Dictionary<string, Selectable> PanelElementsDic = new Dictionary<string, Selectable>();

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            DanbiUIControl.instance.PanelControlDic.Add(DanbiUIPanelKey.ProjectorExternalParameters, this);

            // Create a bew external data instance to save.
            externalDataAsset = ScriptableObject.CreateInstance<DanbiCameraExternalData>();
            externalData = externalDataAsset.asStruct;

            Call_OnToggleCalibratedCameraChanged += (bool flag) =>
            {
                UseCalibratedCamera = flag;
            };

            var panel = Panel.transform;

            // 1. bind the create camera external parameter button.
            var saveCameraExternalParameterButton = panel.GetChild(8).GetComponent<Button>();
            saveCameraExternalParameterButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveNewCameraExternalParameter()); }
            );
            PanelElementsDic.Add("saveCameraExternalParam", saveCameraExternalParameterButton);

            // 2. bind the select camera external parameter button.
            var selectCameraExternalParameterButton = panel.GetChild(1).GetComponent<Button>();
            selectCameraExternalParameterButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_LoadCameraExternalParametersSelector());
                }
            );
            PanelElementsDic.Add("selectCameraExternalParam", selectCameraExternalParameterButton);

            // 3-1. bind the "External parameters" button.
            // bind the radial Coefficient X
            var radialCoefficient = panel.GetChild(3);

            var radialCoefficientXInputField = radialCoefficient.GetChild(0).GetComponent<InputField>();
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.x = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientX", radialCoefficientXInputField);

            // bind the radial Coefficient Y
            var radialCoefficientYInputField = radialCoefficient.GetChild(1).GetComponent<InputField>();
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.y = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientY", radialCoefficientYInputField);

            // bind the radial Coefficient Z
            var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<InputField>();
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.z = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientZ", radialCoefficientZInputField);

            // bind the tangential Coefficient X
            var tangentialCoefficient = panel.GetChild(4);

            var tangentialCoefficientXInputField = tangentialCoefficient.GetChild(0).GetComponent<InputField>();
            tangentialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.x = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientX", tangentialCoefficientXInputField);

            // bind the tangential Coefficient Y
            var tangentialCoefficientYInputField = tangentialCoefficient.GetChild(1).GetComponent<InputField>();
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.y = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientY", tangentialCoefficientYInputField);

            // bind the tangential Coefficient Z
            var tangentialCoefficientZInputField = tangentialCoefficient.GetChild(2).GetComponent<InputField>();
            tangentialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.z = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientZ", tangentialCoefficientZInputField);

            // bind the principal point X
            var principalPoint = panel.GetChild(5);

            var principalPointXInputField = principalPoint.GetChild(0).GetComponent<InputField>();
            principalPointXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPoint.x = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("principalPointX", principalPointXInputField);

            // bind the principal point Y
            var principalPointYInputField = principalPoint.GetChild(1).GetComponent<InputField>();
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPoint.y = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("principalPointY", principalPointYInputField);

            // bind the focal length X
            var focalLength = panel.GetChild(6);

            var focalLengthXInputField = focalLength.GetChild(0).GetComponent<InputField>();
            focalLengthXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLength.x = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("focalLengthX", focalLengthXInputField);

            // bind the principal point Y
            var focalLengthYInputField = focalLength.GetChild(1).GetComponent<InputField>();
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLength.y = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("focalLengthY", focalLengthYInputField);

            // bind the skew coefficient
            var skewCoefficientInputField = panel.GetChild(7).GetComponent<InputField>();
            skewCoefficientInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.skewCoefficient = asFloat;
                    }
                }
            );
            PanelElementsDic.Add("skewCoefficient", skewCoefficientInputField);

            // 3. bind the "External parameters" toggle.
            var useExternalParametersToggle = panel.GetChild(0).GetComponent<Toggle>();
            useExternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    if (isOn)
                    {
                        foreach (var i in PanelElementsDic)
                        {
                            i.Value.interactable = true;
                        }
                    }
                    else
                    {
                        foreach (var i in PanelElementsDic)
                        {
                            i.Value.interactable = false;
                        }
                    }
                }
            );
            useExternalParametersToggle.isOn = false;
        }

        IEnumerator Coroutine_SaveNewCameraExternalParameter()
        {
            string[] filters = new string[] { ".asset", ".ASSET" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileBrowser.OpenSaveDialog(startingPath,
                                                         filters,
                                                         "Save Camera External Parameters",
                                                         "Save");
            DanbiFileBrowser.getActualResourcePath(out var actualPath,
                                                   out _);

            // forward the path to load the external parameters.

            //Debug.Log($"save path : {savePath}", this);
            // var formatter = new BinaryFormatter();
            // FileStream file = File.Create(res);

            externalDataAsset.RadialCoefficient = externalData.radialCoefficient;
            externalDataAsset.TangentialCoefficient = externalData.tangentialCoefficient;
            externalDataAsset.PrincipalPoint = externalData.principalPoint;
            externalDataAsset.FocalLength = externalData.focalLength;
            externalDataAsset.SkewCoefficient = externalData.skewCoefficient;

            string assetPathName = AssetDatabase.GenerateUniqueAssetPath(actualPath + "/New" + typeof(DanbiCameraExternalData).ToString() + ".asset");
            AssetDatabase.CreateAsset(externalDataAsset, assetPathName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = externalDataAsset;
            // formatter.Serialize(file, data);
            // file.Close();
        }

        IEnumerator Coroutine_LoadCameraExternalParametersSelector()
        {
            string[] filters = new string[] { ".asset", ".ASSET" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileBrowser.OpenLoadDialog(startingPath,
                                                         filters,
                                                         "Load Camera Externel Paramaters",
                                                         "Select");
            DanbiFileBrowser.getActualResourcePath(out var actualPath, out var name);

            // Load the External parameters.
            var externalData_intact = Resources.Load<DanbiCameraExternalData>(actualPath);
            yield return new WaitUntil(() => !externalData_intact.Null());
            externalData = externalData_intact.asStruct;

            // update the path, name, values.
            // 1. RadialCoefficient
            // 2. TangentialCoefficient
            // 3. PrincipalPoint
            // 4. ExternalFocalLength
            // 5. SkewCoefficient
            // 6. path
            // 7. name
            (PanelElementsDic["radialCoefficientX"] as InputField).text = $"{externalData.radialCoefficient.x}";
            (PanelElementsDic["radialCoefficientY"] as InputField).text = $"{externalData.radialCoefficient.y}";
            (PanelElementsDic["radialCoefficientZ"] as InputField).text = $"{externalData.radialCoefficient.z}";

            (PanelElementsDic["tangentialCoefficientX"] as InputField).text = $"{externalData.tangentialCoefficient.x}";
            (PanelElementsDic["tangentialCoefficientY"] as InputField).text = $"{externalData.tangentialCoefficient.y}";
            (PanelElementsDic["tangentialCoefficientZ"] as InputField).text = $"{externalData.tangentialCoefficient.z}";

            (PanelElementsDic["principalPointX"] as InputField).text = $"{externalData.principalPoint.x}";
            (PanelElementsDic["principalPointY"] as InputField).text = $"{externalData.principalPoint.y}";

            (PanelElementsDic["focalLengthX"] as InputField).text = $"{externalData.focalLength.x}";
            (PanelElementsDic["focalLengthY"] as InputField).text = $"{externalData.focalLength.y}";

            (PanelElementsDic["skewCoefficient"] as InputField).text = $"{externalData.skewCoefficient}";
        }

    };
};
