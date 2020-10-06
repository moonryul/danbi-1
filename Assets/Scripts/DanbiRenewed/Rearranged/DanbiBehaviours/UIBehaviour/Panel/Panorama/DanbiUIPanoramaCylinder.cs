using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    [System.Serializable]
    public class DanbiUIPanoramaCylinder
    {
        [Readonly]
        public float radius;
        [Readonly]
        public float ch;
        [Readonly]
        public float cl;

        readonly DanbiUIPanoramaScreenShapePanelControl Owner;

        public DanbiUIPanoramaCylinder(DanbiUIPanoramaScreenShapePanelControl owner)
        {
            Owner = owner;
        }

        void LoadPreviousValues(params ILayoutElement[] uiElements)
        {
            var prevRadius = PlayerPrefs.GetFloat("PanoramaCylinder-radius", default);
            radius = prevRadius;
            (uiElements[0] as InputField).text = prevRadius.ToString();

            var prevCh = PlayerPrefs.GetFloat("PanoramaCylinder-ch", ch);
            ch = prevCh;
            (uiElements[1] as InputField).text = prevCh.ToString();

            var prevCl = PlayerPrefs.GetFloat("PanoramaCylinder-cl", cl);
            cl = prevCl;
            (uiElements[2] as InputField).text = prevCl.ToString();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(Owner);
        }

        public void BindInput(Transform panel)
        {
            // bind the radius
            var radiusInputField = panel.GetChild(0).GetComponent<InputField>();
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

            LoadPreviousValues(radiusInputField, chInputField, clInputField);
        }
    };
};