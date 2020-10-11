

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
/// <summary>
/// To disable the unnecessary mesh filter's props.
/// </summary>
public class DanbiDisableMeshFilterProps : MonoBehaviour
{
    /// <summary>
    /// Disable all the unnecessary MeshRenderer Properties.
    /// </summary>
    [MenuItem("Danbi/Disable all of unnecessary MeshRenderer properties")]
    public static void DisableAllUnnecessaryMeshRendererProps()
    {
        var arrAllObjs = FindObjectsOfType<MeshRenderer>();
        if (arrAllObjs.Length == 0)
        {
            Debug.LogWarning("<color=red>Disabling unnecessary MeshRender properties failed! Can't find any MeshRenderer components!</color>");
            return;
        }

        foreach (var it in arrAllObjs)
        {
            it.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            it.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            it.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            Debug.Log($"<color=cyan>Some properties of {it.name} meshRenderer is disabled for the optimization.</color>");
        }
    }
};
#endif