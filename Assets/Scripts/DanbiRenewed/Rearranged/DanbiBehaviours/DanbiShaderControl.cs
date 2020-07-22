using System;
using System.Collections.Generic;

using UnityEngine;


namespace Danbi {
  using PrewarperSets = Dictionary<string, DanbiPrewarperSet>;  

  public class DanbiShaderControl : MonoBehaviour {
    [SerializeField, Header("Ray-Tracer Compute Shader")]
    ComputeShader RTShader;

    public ComputeShader rtShader {
      get {
        return RTShader;
      }
      set {
        RTShader = value;
      }
    }

    [SerializeField, Header("2 by default for the best performance")]
    int MaxNumOfBounce = 2;

    public int maxNumOfBounce {
      get => MaxNumOfBounce;
      set {
        if (value < 0) {
          Debug.Log("MaxNumOfBounce cannot be under 0! it's set to 2 by default");
          MaxNumOfBounce = 2;
        } else {
          MaxNumOfBounce = value;
        }
      }
    }

    [Readonly, SerializeField, Space(15)]
    Material AddMaterial_ScreenSampling;

    public Material addMaterial_ScreenSampling { get => AddMaterial_ScreenSampling; set => AddMaterial_ScreenSampling = value; }

    PrewarperSets PrewarperSets = new PrewarperSets();

    public void RegisterNewPrewarperSet(string name, DanbiPrewarperSet newSet) {
      PrewarperSets.Add(name, newSet);
    }

    public void UnregisterPrewarperSet(string name) {
      PrewarperSets.Remove(name);
    }

    public void UnregisterAllPrewarperSets() {
      PrewarperSets.Clear();
    }

    public void Rebuild(string name) {
      if (String.IsNullOrWhiteSpace (name)) {
        Debug.LogError("name is invalid!", this);
      }
    }
    public void RebuildAll() {
      
    }

    internal struct POD_MeshDatas {
      public List<Vector3> vertices;
      public List<uint> indices;
      public List<Vector2> texcoords;
    };

  };
};
