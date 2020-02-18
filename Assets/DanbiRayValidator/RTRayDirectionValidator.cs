using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if RAY_VALIDATOR_ON
public class RTRayDirectionValidator : MonoBehaviour {
  public float Length = 5.0f;

  public ComputeShader RayTracerRef { get; set; }

  public List<RayInfo> RayInfoList = new List<RayInfo>();

  ComputeBuffer RayInfoBuf;

  public List<RayValidatorLine> RayValidatorLineList = new List<RayValidatorLine>();

  void Start() {
    Assert.IsFalse(RayTracerRef == null);
    RayInfoBuf = new ComputeBuffer(Screen.width * Screen.height, 24);
    for (int i = 0; i < RayInfoBuf.count; ++i) {
      InitLine(i);
    }
  }

  void InitLine(int idx) {
    RayValidatorLineList.Add(new RayValidatorLine());
    var go = new GameObject($"RayValidator Line <{idx}>").AddComponent<LineRenderer>();
    var newLine = go.GetComponent<LineRenderer>();
    newLine.material = new Material(Shader.Find("Yoonsang/ColoredLine"));
    newLine.positionCount = 2;
    newLine.startWidth = 1.0f;
    newLine.endWidth = 1.0f;
    newLine.startColor = Color.black;
    newLine.endColor = Color.black;
    newLine.useWorldSpace = true;
    RayValidatorLineList[idx].Line = newLine;
    RayValidatorLineList[idx].SetLineColor(eRayValidatorLineColor.GREEN);
  }

  void OnDisable() {
    RayInfoBuf?.Release();
  }

  void Update() {
    
  }
};
#endif