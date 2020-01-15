using System.Collections.Generic;
using UnityEngine;

public class LocationPresetTracker : MonoBehaviour {
  [SerializeField] Transform Target;
  public List<Transform> Prefabs = new List<Transform>();
  public List<List<Transform>> Presets = new List<List<Transform>>();
  public bool IsBuilt = false;

  public void BuildLists() {
    var tfs = Target.GetComponentsInChildren<Transform>();

    for (int i = 0; i < Prefabs.Count; ++i) {
      var fwd = Instantiate(Prefabs[i]).GetComponentsInChildren<Transform>();
      foreach (var e in fwd) {
        e.gameObject.hideFlags = HideFlags.HideAndDontSave;
      }      
      Presets.Add(new List<Transform>(fwd));
    }
  }

  public void UsePreset(int number) {
    Target = Presets[number][0];
  }
};
