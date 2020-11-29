using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
namespace Danbi
{
    // https://itmining.tistory.com/55?category=640760
    public class DanbiBuildAssetsBundles : MonoBehaviour
    {
        [MenuItem("Bundles/Build AssetBundles")]
        static void BuildAllAssetBundles()
        {
            string assetBundleDir = "Assets/AssetBundles";
            if (!System.IO.Directory.Exists(assetBundleDir))
            {
                System.IO.Directory.CreateDirectory(assetBundleDir);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }
    };
};
#endif
