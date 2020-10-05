using UnityEngine;
using UnityEditor;

namespace Danbi
{
    public static class DanbiUtils
    {
        // public static bool TryParse<T>(string str, out T res) where T : struct
        // {
        //     var c_str = str.ToCharArray();
        //     char buf = ' ';
        //     int idx = 0;
        //     while (buf != ',' || buf != '.')
        //     {
        //         buf = c_str[idx];
        //         if (idx > str.Length)
        //         {
        //             res = float.Parse(str);
        //             return true;
        //         }
        //     }
        //     var temp = str.Split(buf);
        //     res = temp[0] + temp[1];
        //     return true;
        // }
    };



    public static class DanbiMeshHelper
    {


        ///// <summary>
        ///// Make the custom mesh.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="meshType"></param>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public static T MakeCustomMesh<T>(EDanbiCustomMeshType meshType, string name)
        //  where T : DanbiCustomShape {
        //  var newShape = new DanbiCustomShape(name);
        //  switch (meshType) {
        //    case EDanbiCustomMeshType.Cylinder:
        //    //newShape = new Danbi
        //    break;

        //    case EDanbiCustomMeshType.Cube:
        //    break;

        //    case EDanbiCustomMeshType.Cone:
        //    newShape = new DanbiCone(name);
        //    break;

        //    case EDanbiCustomMeshType.Pyramid:
        //    break;

        //    case EDanbiCustomMeshType.Hemisphere:
        //    break;
        //  }

        //  return (T)newShape;
        //}

        //public static T MakeProceduralMesh<T>(EDanbiProceduralMeshType meshType, string name)
        //  where T : DanbiProceduralShape {
        //  var newShape = new DanbiProceduralShape(name);
        //  switch (meshType) {
        //    case EDanbiProceduralMeshType.Sphere:
        //    break;
        //  }

        //  return (T)newShape;
        //};
    };
};