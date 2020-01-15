using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(LocationPresetTracker))]
public class LocationPresetTrackerEditor : Editor {
  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    var tracker = target as LocationPresetTracker;
    if (GUILayout.Button("Build Lists")) {
      tracker.IsBuilt = true;
      tracker.BuildLists();
    }

    if (!tracker.IsBuilt) {
      return;
    }

    if (GUILayout.Button("Preset 01")) {
      tracker.UsePreset(0);
    }

    if (GUILayout.Button("Preset 02")) {
      tracker.UsePreset(1);
    }

    if (GUILayout.Button("Preset 03")) {
      tracker.UsePreset(2);
    }

    if (GUILayout.Button("Preset 04")) {
      tracker.UsePreset(3);
    }
  }
};
#endif
