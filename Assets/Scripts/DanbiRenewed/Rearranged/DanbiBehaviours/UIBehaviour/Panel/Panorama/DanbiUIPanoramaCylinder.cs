using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaCylinder
    {
        public float radius { get; set; }
        public float ch { get; set; }
        public float cl { get; set; }

        DanbiUIPanoramaScreenShapePanelControl Owner;

        public DanbiUIPanoramaCylinder(DanbiUIPanoramaScreenShapePanelControl owner)
        {
            Owner = owner;
        }

        void OnDisable()
        {
            PlayerPrefs.SetFloat("PanoramaCylinder-radius", radius);
            PlayerPrefs.SetFloat("PanoramaCylinder-ch", ch);
            PlayerPrefs.SetFloat("PanoramaCylinder-cl", cl);
        }

        public void BindInput(Transform panel)
        {
            // bind the radius
            var radiusInputField = panel.GetChild(0).GetComponent<InputField>();
            var prevRadius = PlayerPrefs.GetFloat("PanoramaCylinder-radius", default);
            radiusInputField.text = prevRadius.ToString();
            radius = prevRadius;
            radiusInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        radius = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the ch
            var chInputField = panel.GetChild(1).GetComponent<InputField>();
            var prevCh = PlayerPrefs.GetFloat("PanoramaCylinder-ch", ch);
            chInputField.text = prevCh.ToString();
            ch = prevCh;
            chInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        ch = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );

            // bind the cl
            var clInputField = panel.GetChild(2).GetComponent<InputField>();
            var prevCl = PlayerPrefs.GetFloat("PanoramaCylinder-cl", cl);
            clInputField.text = prevCl.ToString();
            cl = prevCl;
            clInputField?.onValueChanged.AddListener(
                (string val) =>
                {
                    if (float.TryParse(val, out var asFloat))
                    {
                        cl = asFloat;
                        DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
                    }
                }
            );
        }
    };
};