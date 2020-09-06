using System;
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

        readonly string startingPath = "C:/Dev/danbi_2020_march/Assets/Resources/";

        protected override void BindPanelFields()
        {
            base.BindPanelFields();

            var panel = Panel.transform;

            var textureSelectorButton = panel.GetChild(1).GetComponent<Button>();
            textureSelectorButton?.onClick.AddListener(
                () =>
                {
                    StartCoroutine(Timer_LoadTextureSelector());
                }
            );

            var tilingInputField = panel.GetChild(4).GetComponent<InputField>();
            tilingInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    tiling = float.Parse(val);
                }
            );

            var opacityInputField = panel.GetChild(6).GetComponent<InputField>();
            opacityInputField?.onValueChanged.AddListener(
                (val) =>
                {
                    opacity = float.Parse(val);
                }
            );
        }

        IEnumerator Timer_LoadTextureSelector()
        {
            // https://github.com/yasirkula/UnitySimpleFileBrowser
            yield return FileBrowser.WaitForLoadDialog(false, false, startingPath, "Load Texture for room", "Select");

            string path = default;
            if (FileBrowser.Success)
            {
                path = FileBrowser.Result[0];
            }

            // char buf = '0';
            // char[] temp = path.ToCharArray();
            // int i = 0;

            // path.TrimStart("");
            // Debug.Log($"{path}");

            // while (buf != 'A')
            // {
            //     buf = temp[i];
            // }

            // string textureLocation = path.Substring(i);
            // roomTex = Resources.Load<Texture2D>(textureLocation);
        }
    };
};