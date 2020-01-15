using UnityEngine.Assertions;
public class RTmeshProjectorQuad : RTmeshObject {
  public override void OnEnable() {
    Assert.IsFalse(gameObject.isStatic, "Mesh Objects cannot be static!");
    RTcomputeShaderHelper.RegisterToRTmeshProjectorQuad(this);
    RTcomputeShaderHelper.DoesNeedToRebuildRTmeshProjectorQuad = true;
  }

  public override void OnDisable() {
    RTcomputeShaderHelper.UnregisterFromRTmeshProjectorQuad(this);
    RTcomputeShaderHelper.DoesNeedToRebuildRTmeshProjectorQuad = true;
  }
};
