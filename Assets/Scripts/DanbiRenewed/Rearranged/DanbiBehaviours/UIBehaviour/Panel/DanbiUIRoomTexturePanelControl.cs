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
        // public Texture2D roomTex { get; set; }
        public float tiling { get; set; }
        public float opacity { get; set; }
        string path;

        Text TexturePathText;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            // get panel transform.
            var panel = Panel.transform;

            // bind the texture Selector.
            var textureSelectorButton = panel.GetChild(1).GetComponent<Button>();
            textureSelectorButton?.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Timer_LoadTextureSelector(panel, new string[]
                    {
                        ".jpg", ".JPG", ".jpeg", ".JPEG", ".png", ".PNG"
                    }));
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

        IEnumerator Timer_LoadTextureSelector(Transform panel, IEnumerable<string> filters)
        {
            yield return DanbiFileBrowser.OpenLoadDialog(Application.dataPath + "/Resources/",
                                                         filters,
                                                         "Load Room Texture",
                                                         "Select");
            DanbiFileBrowser.getActualResourcePath(out path,
                                                   out var textureName);

            // Load the texture.
            var roomTex = Resources.Load<Texture2D>(path);
            yield return new WaitUntil(() => !roomTex.Null());

            // update the texture, name, resolution.
            panel.GetChild(2).GetComponent<RawImage>().texture = roomTex;
            panel.GetChild(3).GetComponent<Text>().text = $"Resolution : {roomTex.width} x {roomTex.height}";
            panel.GetChild(4).GetComponent<Text>().text = $"Name : {textureName}";
        }
    };
};