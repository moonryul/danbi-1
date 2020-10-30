using System.Collections.Generic;

namespace Danbi
{
    public static class DanbiSceneParamsDict
    {
        public static Dictionary<EDanbiSceneParamKey, float> SceneParamsDic { get; set; }
        public static EDanbiSceneParamKey SceneParamKey { get; set; }

        public static float  SceneParamValue { get; set; }
        //Property: https://gamedev.stackexchange.com/questions/158096/what-does-get-set-in-unity-mean
        // The above, called Auto Property,  is equivalent to:
        // int  _CurrentKernelIndex; // known as a 'backinng field'
        // public static int CurrentKernelIndex
        // {  get { return _CurrentKernelIndex; }
        //    set { _CurrentKernelIndex = value;}
        // }

        static DanbiSceneParamsDict()
        {
            SceneParamsDic = new Dictionary<EDanbiSceneParamKey, float>();
            //CurrentKernalKey = "";
        }

        public static bool AddKernalIndexWithKey(EDanbiSceneParamKey key, int kernalIndex)
        {
            if (SceneParamsDic.ContainsKey(key))
            {
                return false;      // if the key is already in the dict, return false
            }
            else
            {
                SceneParamsDic.Add(key, kernalIndex);
                return true;
            }
        }

        public static bool AddKernalIndexWithKey((EDanbiSceneParamKey, float) keyValuePair)
        {
            if (SceneParamsDic.ContainsKey(keyValuePair.Item1))
            {
                return false;       // if the key is already in the dict, return false
            }
            else
            {

                SceneParamsDic.Add(keyValuePair.Item1, keyValuePair.Item2);
                return true;

            }

        }

        public static float  GetKernalIndex(EDanbiSceneParamKey key)
        {
            return SceneParamsDic[key];
        }
    };

};