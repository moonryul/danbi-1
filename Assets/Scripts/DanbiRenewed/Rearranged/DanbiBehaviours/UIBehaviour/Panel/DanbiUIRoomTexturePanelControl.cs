using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections;

namespace Danbi
{
    public class DanbiUIRoomTexturePanelControl : DanbiUIPanelControl
    {
        public Texture2D roomTex { get; set; }
        public float tiling { get; set; }
        public float opacity { get; set; }
        public string path { get; set; }

        readonly string startingPath = "C:/Dev/danbi_2020_march/Assets/Resources/";

        readonly List<string> filter = new List<string>();

        Text TexturePathText;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // Add Filter for image browser.
            filter.Add(".jpg");
            filter.Add(".JPG");
            filter.Add(".jpeg");
            filter.Add(".JPEG");
            filter.Add(".png");
            filter.Add(".PNG");

            // get panel transform.
            var panel = Panel.transform;

            // bind the texture Selector.
            var textureSelectorButton = panel.GetChild(1).GetComponent<Button>();
            textureSelectorButton?.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Timer_LoadTextureSelector(panel));
                }
            );

            // bind the tiling.
            var tilingInputField = panel.GetChild(6).GetComponent<InputField>();
            tilingInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    tiling = float.Parse(val);
                }
            );

            // bind the opacity.
            var opacityInputField = panel.GetChild(8).GetComponent<InputField>();
            opacityInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    opacity = float.Parse(val);
                }
            );
        }

        IEnumerator Timer_LoadTextureSelector(Transform panel)
        {
            // https://github.com/yasirkula/UnitySimpleFileBrowser
            FileBrowser.SetFilters(false, filter);
            yield return FileBrowser.WaitForLoadDialog(false, false,
               startingPath, "Load Texture for room", "Select");

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

            // Load the texture.
            roomTex = Resources.Load<Texture2D>(path);
            yield return new WaitUntil(() => !roomTex.Null());

            // update the texture, name, resolution.
            panel.GetChild(2).GetComponent<RawImage>().texture = roomTex;
            panel.GetChild(3).GetComponent<Text>().text
                = $"Resolution : {roomTex.width} x {roomTex.height}";
            panel.GetChild(4).GetComponent<Text>().text
                = $"Name : {textureName}";
        }
    };
};