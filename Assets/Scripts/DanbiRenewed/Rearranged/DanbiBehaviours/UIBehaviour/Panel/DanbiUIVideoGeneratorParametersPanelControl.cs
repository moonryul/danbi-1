using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{
    public class DanbiUIVideoGeneratorParametersPanelControl : DanbiUIPanelControl
    {
        int MaximumBoundCount, SamplingThreshold;

        string path { get; set; }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            var maxBoundCountInputField = panel.GetChild(0).GetComponent<InputField>();
            maxBoundCountInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        MaximumBoundCount = asInt;
                    }
                }
            );

            var samplingThresholdInputField = panel.GetChild(1).GetComponent<InputField>();
            samplingThresholdInputField.onValueChanged.AddListener(
                (string val) =>
                {
                    if (int.TryParse(val, out var asInt))
                    {
                        SamplingThreshold = asInt;
                    }
                }
            );

            var selectTargetTextureButton = panel.GetChild(2).GetComponent<Button>();
            selectTargetTextureButton.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Coroutine_SelectTargetTexture(
                        new string[]
                        {
                            ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG"
                        }
                    ));
                }
            );
        }

        IEnumerator Coroutine_SelectTargetTexture(IEnumerable<string> filters)
        {
            FileBrowser.SetFilters(false, filters);
            string startingPath = Application.dataPath + "/Resources/";
            yield return FileBrowser.WaitForLoadDialog(false, false,
                startingPath, "Load Target Texture", "Select");

            if (!FileBrowser.Success)
            {
                Debug.LogError($"<color=red>Failed to select the file from FileBrowser!</color>");
                yield break;
            }

            // forward the result path.
            string res = FileBrowser.Result[0];
            string[] splitted = res.Split('\\');
            string textureName = default;

            // refine the path to load the image.
            for (int i = 0; i < splitted.Length; ++i)
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
                            textureName = name[0];
                        }
                    }
                    break;
                }
            }
        }
    };
};
