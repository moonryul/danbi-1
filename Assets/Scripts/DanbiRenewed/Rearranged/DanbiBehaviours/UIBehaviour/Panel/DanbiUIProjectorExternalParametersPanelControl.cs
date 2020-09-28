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
        public bool useExternalParameters = false;
        public string loadPath;
        public string savePath;
        Dictionary<string, Selectable> PanelElementsDic = new Dictionary<string, Selectable>();
        string playerPrefsKeyRoot = "ProjectorExternalParameters-";
        void OnDisable()
        {
            PlayerPrefs.SetInt(playerPrefsKeyRoot + "useExternalParameters", useExternalParameters == true ? 1 : 0);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-X", externalData.radialCoefficient.x);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", externalData.radialCoefficient.y);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", externalData.radialCoefficient.z);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", externalData.tangentialCoefficient.x);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", externalData.tangentialCoefficient.y);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Z", externalData.tangentialCoefficient.z);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-X", externalData.principalPoint.x);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "principalPoint-Y", externalData.principalPoint.y);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-X", externalData.focalLength.x);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "focalLength-Y", externalData.focalLength.y);
            PlayerPrefs.SetFloat(playerPrefsKeyRoot + "skewCoefficient", externalData.skewCoefficient);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // Create a bew external data instance to save.
            externalDataAsset = ScriptableObject.CreateInstance<DanbiCameraExternalData>();
            externalData = externalDataAsset.asStruct;

            var panel = Panel.transform;

            // bind the "External parameters" toggle.
            var useExternalParametersToggle = panel.GetChild(0).GetComponent<Toggle>();
            bool prevUseExternalParamters = PlayerPrefs.GetInt(playerPrefsKeyRoot + "useExternalParameters", default) == 1;
            useExternalParameters = prevUseExternalParamters;
            useExternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    useExternalParameters = isOn;
                    foreach (var i in PanelElementsDic)
                    {
                        i.Value.interactable = isOn;
                    }
                    DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                }
            );
            // useExternalParametersToggle

            // bind the select camera external parameter button.
            var selectCameraExternalParameterButton = panel.GetChild(1).GetComponent<Button>();
            selectCameraExternalParameterButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_LoadCameraExternalParametersSelector(panel));
                }
            );
            PanelElementsDic.Add("selectCameraExternalParam", selectCameraExternalParameterButton);

            // bind the "External parameters" buttons.
            // bind the radial Coefficient X
            var radialCoefficient = panel.GetChild(3);

            var radialCoefficientXInputField = radialCoefficient.GetChild(0).GetComponent<InputField>();
            float prevRadialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-X", default);
            radialCoefficientXInputField.text = prevRadialCoefficientX.ToString();
            externalData.radialCoefficient.x = prevRadialCoefficientX;
            radialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.x = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientX", radialCoefficientXInputField);

            // bind the radial Coefficient Y
            var radialCoefficientYInputField = radialCoefficient.GetChild(1).GetComponent<InputField>();
            float prevRadialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Y", default);
            radialCoefficientYInputField.text = prevRadialCoefficientY.ToString();
            externalData.radialCoefficient.y = prevRadialCoefficientY;
            radialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.y = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientY", radialCoefficientYInputField);

            // bind the radial Coefficient Z
            var radialCoefficientZInputField = radialCoefficient.GetChild(2).GetComponent<InputField>();
            float prevRadialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "radialCoefficient-Z", default);
            radialCoefficientZInputField.text = prevRadialCoefficientZ.ToString();
            externalData.radialCoefficient.z = prevRadialCoefficientZ;
            radialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.radialCoefficient.z = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("radialCoefficientZ", radialCoefficientZInputField);

            // bind the tangential Coefficient X
            var tangentialCoefficient = panel.GetChild(4);

            var tangentialCoefficientXInputField = tangentialCoefficient.GetChild(0).GetComponent<InputField>();
            float prevTangentialCoefficientX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-X", default);
            tangentialCoefficientXInputField.text = prevTangentialCoefficientX.ToString();
            externalData.tangentialCoefficient.x = prevTangentialCoefficientX;
            tangentialCoefficientXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.x = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientX", tangentialCoefficientXInputField);

            // bind the tangential Coefficient Y
            var tangentialCoefficientYInputField = tangentialCoefficient.GetChild(1).GetComponent<InputField>();
            float prevTangentialCoefficientY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentialCoefficient-Y", default);
            tangentialCoefficientYInputField.text = prevTangentialCoefficientY.ToString();
            externalData.tangentialCoefficient.y = prevTangentialCoefficientY;
            tangentialCoefficientYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.y = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientY", tangentialCoefficientYInputField);

            // bind the tangential Coefficient Z
            var tangentialCoefficientZInputField = tangentialCoefficient.GetChild(2).GetComponent<InputField>();
            float prevTangentialCoefficientZ = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "tangentailCoefficient-Z", default);
            tangentialCoefficientZInputField.text = prevTangentialCoefficientZ.ToString();
            externalData.tangentialCoefficient.z = prevTangentialCoefficientZ;
            tangentialCoefficientZInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.tangentialCoefficient.z = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("tangentialCoefficientZ", tangentialCoefficientZInputField);

            // bind the principal point X
            var principalPoint = panel.GetChild(5);

            var principalPointXInputField = principalPoint.GetChild(0).GetComponent<InputField>();
            float prevPrincipalPointX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-X", default);
            principalPointXInputField.text = prevPrincipalPointX.ToString();
            externalData.principalPoint.x = prevPrincipalPointX;
            principalPointXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPoint.x = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("principalPointX", principalPointXInputField);

            // bind the principal point Y
            var principalPointYInputField = principalPoint.GetChild(1).GetComponent<InputField>();
            float prevPrincipalPointY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "principalPoint-Y", default);
            principalPointYInputField.text = prevPrincipalPointY.ToString();
            externalData.principalPoint.y = prevPrincipalPointY;
            principalPointYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.principalPoint.y = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("principalPointY", principalPointYInputField);

            // bind the focal length X
            var focalLength = panel.GetChild(6);

            var focalLengthXInputField = focalLength.GetChild(0).GetComponent<InputField>();
            float prevFocalLengthX = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-X", default);
            focalLengthXInputField.text = prevFocalLengthX.ToString();
            externalData.focalLength.x = prevFocalLengthX;
            focalLengthXInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLength.x = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("focalLengthX", focalLengthXInputField);

            // bind the principal point Y
            var focalLengthYInputField = focalLength.GetChild(1).GetComponent<InputField>();
            float prevFocalLengthY = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "focalLength-Y", default);
            focalLengthYInputField.text = prevFocalLengthY.ToString();
            externalData.focalLength.y = prevFocalLengthY;
            focalLengthYInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        externalData.focalLength.y = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
                    }
                }
            );
            PanelElementsDic.Add("focalLengthY", focalLengthYInputField);

            // bind the skew coefficient
            var skewCoefficientInputField = panel.GetChild(7).GetComponent<InputField>();
            float prevSkewCoefficient = PlayerPrefs.GetFloat(playerPrefsKeyRoot + "skewCoefficient", default);
            skewCoefficientInputField.text = prevSkewCoefficient.ToString();
            externalData.skewCoefficient = prevSkewCoefficient;
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
            PanelElementsDic.Add("skewCoefficient", skewCoefficientInputField);

            // bind the create camera external parameter button.
            var saveCameraExternalParameterButton = panel.GetChild(8).GetComponent<Button>();
            saveCameraExternalParameterButton.onClick.AddListener(
                () => { StartCoroutine(Coroutine_SaveNewCameraExternalParameter()); }
            );
            PanelElementsDic.Add("saveCameraExternalParam", saveCameraExternalParameterButton);

            useExternalParametersToggle.isOn = prevUseExternalParamters;
        }

        IEnumerator Coroutine_SaveNewCameraExternalParameter()
        {
            var filters = new string[] { ".asset", ".ASSET" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileBrowser.OpenSaveDialog(startingPath,
                                                         filters,
                                                         "Save Camera External Parameters",
                                                         "Save");
            DanbiFileBrowser.getActualResourcePath(out savePath,
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

            string assetPathName = AssetDatabase.GenerateUniqueAssetPath(loadPath + "/New" + typeof(DanbiCameraExternalData).ToString() + ".asset");
            AssetDatabase.CreateAsset(externalDataAsset, assetPathName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = externalDataAsset;
            // formatter.Serialize(file, data);
            // file.Close();
        }

        IEnumerator Coroutine_LoadCameraExternalParametersSelector(Transform panel)
        {
            var filters = new string[] { ".asset", ".ASSET" };
            string startingPath = Application.dataPath + "/Resources/";
            yield return DanbiFileBrowser.OpenLoadDialog(startingPath,
                                                         filters,
                                                         "Load Camera Externel Paramaters (Scriptable Object)",
                                                         "Select");
            DanbiFileBrowser.getActualResourcePath(out loadPath, out var name);
            // Load the External parameters.
            var loaded = Resources.Load<DanbiCameraExternalData>(loadPath);
            yield return new WaitUntil(() => !loaded.Null());
            externalData = loaded.asStruct;
            panel.GetChild(2).GetComponent<Text>().text = loadPath;

            (PanelElementsDic["radialCoefficientX"] as InputField).text = externalData.radialCoefficient.x.ToString();
            (PanelElementsDic["radialCoefficientY"] as InputField).text = externalData.radialCoefficient.y.ToString();
            (PanelElementsDic["radialCoefficientZ"] as InputField).text = externalData.radialCoefficient.z.ToString();

            (PanelElementsDic["tangentialCoefficientX"] as InputField).text = externalData.tangentialCoefficient.x.ToString();
            (PanelElementsDic["tangentialCoefficientY"] as InputField).text = externalData.tangentialCoefficient.y.ToString();
            (PanelElementsDic["tangentialCoefficientZ"] as InputField).text = externalData.tangentialCoefficient.z.ToString();

            (PanelElementsDic["principalPointX"] as InputField).text = externalData.principalPoint.x.ToString();
            (PanelElementsDic["principalPointY"] as InputField).text = externalData.principalPoint.y.ToString();

            (PanelElementsDic["focalLengthX"] as InputField).text = externalData.focalLength.x.ToString();
            (PanelElementsDic["focalLengthY"] as InputField).text = externalData.focalLength.y.ToString();

            (PanelElementsDic["skewCoefficient"] as InputField).text = externalData.skewCoefficient.ToString();
            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

    };
};
