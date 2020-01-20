using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(RTmaster))]
public class PrewarpedImageCaptureEditor : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    if (GUILayout.Button("Create Prewarped Image!")) {
      var inst = target as RTmaster;
      var rt = inst.ResultRenderTexture;
      RenderTexture.active = rt;
      var resTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
      resTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
      resTex.Apply();
      RenderTexture.active = null;

      byte[] bytes;
      bytes = resTex.EncodeToPNG();

      string path = Application.dataPath + $"\\Resources\\result{RTmaster.resultID++}.png";
      System.IO.File.WriteAllBytes(path, bytes);
      AssetDatabase.ImportAsset(path);
      Debug.Log($"Render Texture is saved to ${path}");
    }
  }
};
#endif
