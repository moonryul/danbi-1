using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi {
  public class DanbiComputeShaderHelper : MonoBehaviour {
    [SerializeField, Header("Ray-Tracer Compute Shader"), Space(10)]
    ComputeShader RTShader;

    public ComputeShader rtShader {
      get => RTShader; set => RTShader = value;
    }

    [SerializeField, Header("2 by default for the best performance")]
    int MaxNumOfBounce = 2;

    public int maxNumOfBounce {
      get => MaxNumOfBounce;
      set {
        if (value < 0) {
          Debug.Log("MaxNumOfBounce cannot be under 0!");
          MaxNumOfBounce = 2;
        }
        else {
          MaxNumOfBounce = value;
        }
      }
    }


  };
};
