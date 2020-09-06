using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

namespace Danbi
{    
    public class DanbiUIRoomTexturePanelControl : DanbiUIPanelControl
    {
        public Texture2D roomTex { get; set; }
        public float tiling { get; set; }
        public float opacity { get; set; }

        protected override void BindPanelFields()
        {
            base.BindPanelFields();

            // https://github.com/yasirkula/UnitySimpleFileBrowser
            string[] loadRes = new string[1];
            FileBrowser.ShowLoadDialog((paths) => {
                loadRes = paths;
                Debug.Log($"{loadRes[0]}");
            },
            () => {

            }, false, false, null, "Load Texture for Room", "Select");
            
            var panel = Panel.transform;

            // TODO: Texture Picker.

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
    };
};