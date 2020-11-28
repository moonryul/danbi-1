using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIPanoramaScreenShapePanel : DanbiUIPanelControl
    {
        [SerializeField]
        int m_selectedPanoramaScreenIdx;

        public delegate void OnPanoramaScreenShapeChange(int idx);
        public static OnPanoramaScreenShapeChange onPanoramaScreenShapeChange;

        public delegate void OnMeshLoaded(MeshRenderer renderer, MeshFilter filter);
        public static OnMeshLoaded onMeshLoaded;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var panoramaTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            panoramaTypeDropdown?.AddOptions(new List<string> { "Cube", "Cylinder" });
            panoramaTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_selectedPanoramaScreenIdx = option;
                    onPanoramaScreenShapeChange?.Invoke(m_selectedPanoramaScreenIdx);
                    DanbiUIPanoramaScreenDimensionPanel.onTypeChange?.Invoke(option);
                    panel.gameObject.SetActive(false);
                }
            );
            panoramaTypeDropdown.value = 0;

            // var panoramaMeshSelectButton = panel.GetChild(1).GetComponent<Button>();
            // panoramaMeshSelectButton.onClick.AddListener(() => this.SelectMesh());
        }

        IEnumerator SelectMesh()
        {
            // string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            // var filter = new List<string> { ".fbx", ".obj" };
            // yield return DanbiFileSys.OpenLoadDialog(startingPath,
            //                                          filter,
            //                                          "Select Mesh",
            //                                          "Select");
            yield break;
        }
    };
};