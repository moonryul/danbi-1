using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;

namespace Danbi
{
    public class DanbiUICameraExternalParametersPanelControl : DanbiUIPanelControl
    {
        DanbiCameraExternalData externalDataAsset;
        [HideInInspector]
        public DanbiCameraExternalData_struct externalData;
        public string savePath { get; set; }
        public string loadPath { get; set; }
        string startingPath;

        public delegate void OnToggleCalibratedCameraChanged(bool flag);
        public static OnToggleCalibratedCameraChanged Call_OnToggleCalibratedCameraChanged;
        public static bool UseCalibratedCamera = false;

        InputField radialCoefficientXInputField, radialCoefficientYInputField, radialCoefficientZInputField;
        InputField tangentialCoefficientXInputField, tangentialCoefficientYInputField, tangentialCoefficientZInputField;
        InputField principalPointXInputField, principalPointYInputField;
        InputField focalLengthXInputField, focalLengthYInputField;
        InputField skewCoefficientInputField;

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            base.OnMenuButtonSelected(lastClicked);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            externalDataAsset = ScriptableObject.CreateInstance<DanbiCameraExternalData>();
            externalData = externalDataAsset.asStruct;

            startingPath = Application.dataPath + "/Resources/";
            Debug.Log($"Starting Path: {startingPath}");

            Call_OnToggleCalibratedCameraChanged += (bool flag) =>
            {
                UseCalibratedCamera = flag;
            };

            var panel = Panel.transform;

            // 1. bind the create camera external parameter button.
            var saveCameraExternalParameterButton = panel.GetChild(21).GetComponent<Button>();
            saveCameraExternalParameterButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_SaveNewCameraExternalParameter(
                        new string[]
                        {
                            ".asset", ".ASSET"
                        }));
                }
            );

            // 2. bind the select camera external parameter button.
            var selectCameraExternalParameterButton = panel.GetChild(2).GetComponent<Button>();
            selectCameraExternalParameterButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_LoadCameraExternalParametersSelector(
                        new string[]
                        {
                            ".asset", ".ASSET"
                        }));
                }
            );

            // 3-1. bind the "External parameters" button.
            // bind the radial Coefficient X
            radialCoefficientXInputField = panel.GetChild(5).GetComponent<InputField>();
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.x = asFloat;
                    }
                }
            );

            // bind the radial Coefficient Y
            radialCoefficientYInputField = panel.GetChild(6).GetComponent<InputField>();
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.y = asFloat;
                    }
                }
            );

            // bind the radial Coefficient Z
            radialCoefficientZInputField = panel.GetChild(7).GetComponent<InputField>();
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.z = asFloat;
                    }
                }
            );

            // bind the tangential Coefficient X
            tangentialCoefficientXInputField = panel.GetChild(9).GetComponent<InputField>();
            tangentialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.x = asFloat;
                    }
                }
            );

            // bind the tangential Coefficient Y
            tangentialCoefficientYInputField = panel.GetChild(10).GetComponent<InputField>();
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.y = asFloat;
                    }
                }
            );

            // bind the tangential Coefficient Z
            tangentialCoefficientZInputField = panel.GetChild(11).GetComponent<InputField>();
            tangentialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.z = asFloat;
                    }
                }
            );

            // bind the principal point X
            principalPointXInputField = panel.GetChild(13).GetComponent<InputField>();
            principalPointXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPoint.x = asFloat;
                    }
                }
            );

            // bind the principal point Y
            principalPointYInputField = panel.GetChild(14).GetComponent<InputField>();
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPoint.y = asFloat;
                    }
                }
            );

            // bind the focal length X
            focalLengthXInputField = panel.GetChild(16).GetComponent<InputField>();
            focalLengthXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLength.x = asFloat;
                    }
                }
            );

            // bind the principal point Y
            focalLengthYInputField = panel.GetChild(17).GetComponent<InputField>();
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLength.y = asFloat;
                    }
                }
            );

            // bind the skew coefficient
            skewCoefficientInputField = panel.GetChild(19).GetComponent<InputField>();
            skewCoefficientInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.skewCoefficient = asFloat;
                    }
                }
            );

            // 3. bind the "External parameters" toggle.
            var useExternalParametersToggle = panel.GetChild(0).GetComponent<Toggle>();
            useExternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    if (isOn)
                    {
                        selectCameraExternalParameterButton.interactable = true;
                        saveCameraExternalParameterButton.interactable = true;
                        radialCoefficientXInputField.interactable = true;
                        radialCoefficientYInputField.interactable = true;
                        radialCoefficientZInputField.interactable = true;
                        tangentialCoefficientXInputField.interactable = true;
                        tangentialCoefficientYInputField.interactable = true;
                        tangentialCoefficientZInputField.interactable = true;
                        principalPointXInputField.interactable = true;
                        principalPointYInputField.interactable = true;
                        focalLengthXInputField.interactable = true;
                        focalLengthYInputField.interactable = true;
                        skewCoefficientInputField.interactable = true;
                    }
                    else
                    {
                        selectCameraExternalParameterButton.interactable = false;
                        saveCameraExternalParameterButton.interactable = false;
                        radialCoefficientXInputField.interactable = false;
                        radialCoefficientYInputField.interactable = false;
                        radialCoefficientZInputField.interactable = false;
                        tangentialCoefficientXInputField.interactable = false;
                        tangentialCoefficientYInputField.interactable = false;
                        tangentialCoefficientZInputField.interactable = false;
                        principalPointXInputField.interactable = false;
                        principalPointYInputField.interactable = false;
                        focalLengthXInputField.interactable = false;
                        focalLengthYInputField.interactable = false;
                        skewCoefficientInputField.interactable = false;
                    }
                }
            );
            useExternalParametersToggle.isOn = false;
        }

        IEnumerator Coroutine_SaveNewCameraExternalParameter(IEnumerable<string> filters)
        {
            FileBrowser.SetFilters(false, filters);

            yield return FileBrowser.WaitForSaveDialog(false, false,
            startingPath, "Save Camera External Parameters", "Save");

            if (!FileBrowser.Success)
            {
                yield break;
            }

            // forward the path to load the external parameters.
            string res = FileBrowser.Result[0];            
            //Debug.Log($"save path : {savePath}", this);
            // var formatter = new BinaryFormatter();
            // FileStream file = File.Create(res);
            
            externalDataAsset.RadialCoefficient = externalData.radialCoefficient;
            externalDataAsset.TangentialCoefficient = externalData.tangentialCoefficient;
            externalDataAsset.PrincipalPoint = externalData.principalPoint;
            externalDataAsset.FocalLength = externalData.focalLength;
            externalDataAsset.SkewCoefficient = externalData.skewCoefficient;

            string assetPathName = AssetDatabase.GenerateUniqueAssetPath(res + "/New" + typeof(DanbiCameraExternalData).ToString() + ".asset");
            AssetDatabase.CreateAsset(externalDataAsset, assetPathName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = externalDataAsset;
            // formatter.Serialize(file, data);
            // file.Close();
        }

        IEnumerator Coroutine_LoadCameraExternalParametersSelector(IEnumerable<string> filters)
        {
            FileBrowser.SetFilters(false, filters);

            yield return FileBrowser.WaitForLoadDialog(false, false,
            startingPath, "Load Camera External Parameters", "Select");

            if (!FileBrowser.Success)
            {
                //Debug.LogError($"<color=red>Failed to load the file from FileBrowser</color>");
                yield break;
            }

            // forward the path to load the external parameters.
            string res = FileBrowser.Result[0];
            string[] splitted = res.Split('\\');
            string externalParamtersScriptableObjectName = default;

            for (int i = 0; i < splitted.Length; ++i)
            {
                if (splitted[i] == "Resources")
                {
                    for (int j = i + 1; j < splitted.Length; ++j)
                    {
                        if (j != splitted.Length - 1)
                        {
                            loadPath += splitted[j] + '/';
                        }
                        else
                        {
                            var name = splitted[j].Split('.');
                            loadPath += name[0];
                            externalParamtersScriptableObjectName = name[0];
                        }
                    }
                    break;
                }
            }

            // Load the External parameters.
            var externalData_intact = Resources.Load<DanbiCameraExternalData>(loadPath);
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
            radialCoefficientXInputField.text = $"{externalData.radialCoefficient.x}";
            radialCoefficientYInputField.text = $"{externalData.radialCoefficient.y}";
            radialCoefficientZInputField.text = $"{externalData.radialCoefficient.z}";
            tangentialCoefficientXInputField.text = $"{externalData.tangentialCoefficient.x}";
            tangentialCoefficientYInputField.text = $"{externalData.tangentialCoefficient.y}";
            tangentialCoefficientZInputField.text = $"{externalData.tangentialCoefficient.z}";
            principalPointXInputField.text = $"{externalData.principalPoint.x}";
            principalPointYInputField.text = $"{externalData.principalPoint.y}";
            focalLengthXInputField.text = $"{externalData.focalLength.x}";
            focalLengthYInputField.text = $"{externalData.focalLength.y}";
            skewCoefficientInputField.text = $"{externalData.skewCoefficient}";
        }

    };
};
