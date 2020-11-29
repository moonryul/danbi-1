using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Danbi
{
    public class DanbiUIReflectorShapePanel : DanbiUIPanelControl
    {
        [SerializeField, Readonly]
        int m_selectedReflectorIndex;

        TMP_Text m_meshLocation;
        TMP_Text m_vertexCount;
        TMP_Text m_indexCount;
        TMP_Text m_uv0Count;

        public delegate void OnMeshLoaded(Mesh mesh);
        public static OnMeshLoaded onMeshLoaded;

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            var reflectorTypeDropdown = panel.GetChild(0).GetComponent<Dropdown>();
            reflectorTypeDropdown.AddOptions(new List<string> { "Dome", "Cone" });
            reflectorTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_selectedReflectorIndex = option;
                    DanbiUIReflectorDimensionPanel.onTypeChanged?.Invoke(option);
                    panel.gameObject.SetActive(false);
                    // DanbiUISync.onPanelUpdate?.Invoke(this);
                });
            reflectorTypeDropdown.onValueChanged?.Invoke(0);

            var panoramaMeshSelectButton = panel.GetChild(1).GetComponent<Button>();
            panoramaMeshSelectButton.onClick.AddListener(() => StartCoroutine(this.SelectMesh()));

            m_meshLocation = panel.GetChild(2).GetComponent<TMP_Text>();
            m_vertexCount = panel.GetChild(3).GetComponent<TMP_Text>();
            m_indexCount = panel.GetChild(4).GetComponent<TMP_Text>();
            m_uv0Count = panel.GetChild(5).GetComponent<TMP_Text>();
        }

        IEnumerator SelectMesh()
        {
            string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            var filter = new List<string> { ".obj" };
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filter,
                                                     "Select Mesh",
                                                     "Select");
            DanbiFileSys.GetResourcePathIntact(out var rsrcPath, out _);
            var mesh = DanbiOBJImporter.ImportFile(rsrcPath);
            onMeshLoaded?.Invoke(mesh);

            m_meshLocation.text = $"Mesh Location : {rsrcPath}";
            m_vertexCount.text = $"Vertex Count : {mesh.vertexCount}";
            m_indexCount.text = $"Index Count : {mesh.GetIndexCount(0)}";
            m_uv0Count.text = $"UV0 Count : {mesh.uv.Length}";
        }
    };
};
