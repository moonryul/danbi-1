using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Danbi {
  public class DanbiKernel : MonoBehaviour {
    public delegate void OnKernelInitialised(ComputeShader shader);
    public static OnKernelInitialised Call_KernelInitialisation;

    void Start() {
      
    }

    void Caller_KernelInitialisation(ComputeShader shader) {
      DanbiKernelHelper.AddKernalIndexWithKey(
        ("Tricon Mirror", shader.FindKernel("CreateImageTriConeMirror"))
        );
    }
  };
};
