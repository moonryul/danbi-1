using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUICameraCalibrationPanelControl : DanbiUIPanelControl
    {
        [SerializeField]
        List<string> UndistortionMethodContents = new List<string>();

        public DanbiCameraExternalData_struct externalData { get; set; }
        public string path { get; set;}
        readonly string startingPath = "C:/Dev/danbi_2020_march/Assets/Resources/";

        protected override void BindPanelFields()
        {
            base.BindPanelFields();

            UndistortionMethodContents.Add("direct");
            UndistortionMethodContents.Add("iterative");
            UndistortionMethodContents.Add("newton");

            var panel = Panel.transform;
            // 1. bind the "Calibrated Camera" toggle.
            var useCalibratedCameraToggle = panel.GetChild(0).GetComponent<Toggle>();
            useCalibratedCameraToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    if (isOn)
                    {
                        // Activate the elements as follow.
                        // 4 of elements.
                    }
                    else
                    {
                        // désactivez les éléments si-dessous.
                    }
                }
            );
            // 2. bind the "Lens Distortion" toggle.
            var useLensDistortionToggle = panel.GetChild(1).GetComponent<Toggle>();
            useLensDistortionToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    if (isOn)
                    {

                    }
                    else
                    {

                    }
                }
            );

            // forward the reference of the newton properties and turn it off.
            var newtonProperties = panel.GetChild(4).gameObject;
            newtonProperties.SetActive(false);

            // forward the reference of the iterative properties and turn it off.
            var iterativeProperties = panel.GetChild(5).gameObject;
            iterativeProperties.SetActive(false);

            // 2-1. bind the "Lens Distortion Methode" dropdown.
            var undistortionMethodDropdown = panel.GetChild(3).GetComponent<Dropdown>();
            undistortionMethodDropdown.AddOptions(UndistortionMethodContents);
            undistortionMethodDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    switch (option)
                    {
                        case 0: // direct
                                // no parametres are required to be passed into the shader.
                                // so turn all the properties.
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(false);
                            break;

                        case 1: // iterative
                            newtonProperties.SetActive(false);
                            iterativeProperties.SetActive(true);
                            break;

                        case 2: // newton
                            newtonProperties.SetActive(true);
                            iterativeProperties.SetActive(false);
                            break;
                    }
                }
            );

            var selectCameraExternalParameter = panel.GetChild(7).GetComponent<Button>();
            selectCameraExternalParameter.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_LoadCameraExternalParametersSelector(new string[] {
                        ".asset", ".ASSET"
                    }));
                }
            );

            // 3. bind the "External parameters" toggle.
            var useExternalParametersToggle = panel.GetChild(6).GetComponent<Toggle>();
            useExternalParametersToggle.onValueChanged.AddListener(
                (bool isOn) =>
                {
                    if (isOn)
                    {

                    }
                    else
                    {

                    }
                }
            );

            // 3-1. bind the "External paramtres selector" button.
        } // bind

        IEnumerator Coroutine_LoadCameraExternalParametersSelector(IEnumerable<string> filters)
        {
            FileBrowser.SetFilters(false, filters);

            yield return FileBrowser.WaitForLoadDialog(false, false,
            startingPath, "Load Camera External Parameters", "Select");

            if (!FileBrowser.Success)
            {
                Debug.LogError($"<color=red>Failed to load the file from FileBrowser</color>");
                yield break;
            }

            // forward the path to load the external parameters.
            string res = FileBrowser.Result[0];
            string[] splitted = res.Split('\\');
            string externalParamtersScriptableObjectName = default;

            for (int i = 0 ; i < splitted.Length; ++i)
            {
                if (splitted[i] == "Resources")
                {
                    for (int j = i + 1; j < splitted.Length; ++j)
                    {
                        if (j != splitted.Length - 1)
                        {
                            path += splitted[j] + '/';
                        }
                        else 
                        {
                            var name = splitted[j].Split('.');
                            path += name[0];
                            externalParamtersScriptableObjectName = name[0];
                        }
                    }
                    break;
                }
            }

            // Load the External parameters.
            var externalData_intact = Resources.Load<DanbiCameraExternalData>(path);
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
        }
    };
};
