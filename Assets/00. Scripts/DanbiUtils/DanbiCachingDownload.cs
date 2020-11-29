// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;

// namespace Danbi
// {
//     public class DanbiCachingDownload : MonoBehaviour
//     {
//         // 번들 다운 받을 서버의 주소
//         public string m_buildURL;
//         // 번들 버전
//         public int version;

//         void Start()
//         {
//             StartCoroutine(DownloadAndCache());
//         }

//         IEnumerator DownloadAndCache()
//         {
//             // cache 폴더에 AssetBundle 을 담을 것이므로 캐싱 준비까지 대기
//             // while (!Caching.ready)
//             // {
//             //     yield return null;
//             // }

//             // using (var www = WWW.LoadFromCacheOrDownload(m_buildURL, version))
//             // {
//             //     yield return www;
//             //     if (www.error != null)
//             //     {
//             //         throw new System.Exception("WWW download failed :" + www.error);
//             //     }

//             //     var bundle = www.assetBundle;

//             //     AssetBundleRequest request = bundle.LoadAssetAsync("GA509_x5.7_y5.7_z2.9_1", typeof(GameObject));
//             //     yield return request;
//             //     bundle.Unload(false);
//             // }

//             // string uri = $"file:///{Application.dataPath}/AssetsBundles/bundle";
//             // var request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
//             // yield return request.SendWebRequest();

//             // AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
//             // AssetBundleRequest bundleRequest = bundle.LoadAssetAsync<GameObject>("Test_Cube");

//             // yield return bundleRequest;

//             // var pf = bundleRequest.asset;
//             // Instantiate(pf);


//         }
//     };
// };
