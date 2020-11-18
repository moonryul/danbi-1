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
    public class DanbiUIProjectorExternalParametersPanelControl : DanbiUIPanelControl
    {
        [HideInInspector]
        public DanbiCameraExternalData externalData = new DanbiCameraExternalData();

        [Readonly]
        public string loadPath;

        [Readonly]
        public string savePath;

        string playerPrefsKeyRoot = "ProjectorExternalParameters-";

        TMP_Text m_selectProjectorExternalParameterText;

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
            externalData = bf.Deserialize(file) as DanbiCameraExternalData;

            if (externalData is null)
            {
                return;
            }

            (uiElements[1] as TMP_InputField).text = externalData.projectorPosition.x.ToString();
            (uiElements[2] as TMP_InputField).text = externalData.projectorPosition.y.ToString();
            (uiElements[3] as TMP_InputField).text = externalData.projectorPosition.z.ToString();

            (uiElements[4] as TMP_InputField).text = externalData.xAxis.x.ToString();
            (uiElements[5] as TMP_InputField).text = externalData.xAxis.y.ToString();
            (uiElements[6] as TMP_InputField).text = externalData.xAxis.z.ToString();

            (uiElements[7] as TMP_InputField).text = externalData.yAxis.x.ToString();
            (uiElements[8] as TMP_InputField).text = externalData.yAxis.y.ToString();
            (uiElements[9] as TMP_InputField).text = externalData.yAxis.z.ToString();

            (uiElements[10] as TMP_InputField).text = externalData.zAxis.x.ToString();
            (uiElements[11] as TMP_InputField).text = externalData.zAxis.y.ToString();
            (uiElements[12] as TMP_InputField).text = externalData.zAxis.z.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        void Awake()
        {
            // Toggle useExternalParametersToggle = null;
            DanbiUIProjectorCalibratedPanelControl.onSetUseCalibratedCamera +=
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
                    float result = 0.0f;
                    if (float.TryParse(val, out result))
                    {
                        externalData.projectorPosition.x = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.projectorPosition.y = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.projectorPosition.z = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.xAxis.x = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.xAxis.y = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.xAxis.z = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.yAxis.x = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.yAxis.y = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.yAxis.z = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.zAxis.x = result;
                        // Invoke all the eventHandlers that are added to onPanelUpdate event delegate.

                        // that eventHandler is "onPanelUpdate". the central eventHandler!

                        // All the eventHandlers for all the events are invoked by the following statement.
                        // then each eventHandler checks if "this" object is related to it.
                        // Here "this"=="DanbiUIProjectorExternalParametersPanelControl"

                        // DAnbiUISync.onPanelUpdate eventHandler broadcasts "this" to all the eventHandlers.
                        // and then the eventHandler that is related to "this" processes it and the other eventHandlers
                        // just return
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.zAxis.y = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
                        externalData.zAxis.z = result;
                        DanbiUISync.onPanelUpdate?.Invoke(this);
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
            DanbiFileSys.GetResourcePathIntact(out savePath, out _);

            var bf = new BinaryFormatter();
            using (var file = File.Open(savePath, FileMode.OpenOrCreate))
            {
                bf.Serialize(file, externalData);
            }

            DanbiUISync.onPanelUpdate?.Invoke(this);
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
            DanbiFileSys.GetResourcePathIntact(out loadPath, out _);

            // Load the Internal parameters.            
            if (!File.Exists(loadPath))
            {
                yield break;
            }

            var bf = new BinaryFormatter();
            DanbiCameraExternalData loaded = null;
            using (var file = File.Open(loadPath, FileMode.Open))
            {
                loaded = bf.Deserialize(file) as DanbiCameraExternalData;
            }

            yield return new WaitUntil(() => !(loaded is null));
            externalData = loaded;

            m_selectProjectorExternalParameterText.text = loadPath;

            (elements["positionXInputField"] as TMP_InputField).text = externalData.projectorPosition.x.ToString();
            (elements["positionYInputField"] as TMP_InputField).text = externalData.projectorPosition.y.ToString();
            (elements["positionZInputField"] as TMP_InputField).text = externalData.projectorPosition.z.ToString();

            (elements["xOfxAxisInputField"] as TMP_InputField).text = externalData.xAxis.x.ToString();
            (elements["yOfxAxisInputField"] as TMP_InputField).text = externalData.xAxis.y.ToString();
            (elements["zOfxAxisInputField"] as TMP_InputField).text = externalData.xAxis.z.ToString();

            (elements["xOfyAxisInputField"] as TMP_InputField).text = externalData.yAxis.x.ToString();
            (elements["yOfyAxisInputField"] as TMP_InputField).text = externalData.yAxis.y.ToString();
            (elements["zOfyAxisInputField"] as TMP_InputField).text = externalData.yAxis.z.ToString();

            (elements["xOfzAxisInputField"] as TMP_InputField).text = externalData.zAxis.x.ToString();
            (elements["yOfzAxisInputField"] as TMP_InputField).text = externalData.zAxis.y.ToString();
            (elements["zOfzAxisInputField"] as TMP_InputField).text = externalData.zAxis.z.ToString();

            DanbiUISync.onPanelUpdate?.Invoke(this);
        }
    };
};