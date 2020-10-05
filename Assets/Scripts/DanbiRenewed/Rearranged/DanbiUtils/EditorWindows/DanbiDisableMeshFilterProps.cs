

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
/// <summary>
/// To disable the unnecessary mesh filter's props.
/// </summary>
public class DanbiDisableMeshFilterProps : MonoBehaviour
{
    /// <summary>
    /// Disable all the unnecessary MeshRenderer Properties!
    /// </summary>
    [MenuItem("Danbi/Disable all of unnecessary MeshRenderer properties")]
    public static void DisableAllUnnecessaryMeshRendererProps()
    {
        var arrAllObjs = FindObjectsOfType<MeshRenderer>();
        if (arrAllObjs.Length == 0)
        {
            Debug.LogWarning("Disabling unnecessary MeshRender properties failed! Can't find any MeshRenderer components!");
            return;
        }

        int len = 0;
        foreach (var e in arrAllObjs)
        {
            e.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            e.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            e.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            ++len;
        }
        Debug.Log($"{len} of Targets became optimized in MeshRenderer a bit.");
    }
};
#endif