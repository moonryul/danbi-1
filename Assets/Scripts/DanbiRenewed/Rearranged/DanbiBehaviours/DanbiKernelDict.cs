using System.Collections.Generic;

namespace Danbi
{
    public static class DanbiKernelDict
    {
        public static Dictionary<EDanbiKernelKey, int> KernalDic { get; set; }
        public static EDanbiKernelKey CurrentKernalKey { get; set; }

        public static int CurrentKernelIndex { get; set; }
        //Property: https://gamedev.stackexchange.com/questions/158096/what-does-get-set-in-unity-mean
        // The above, called Auto Property,  is equivalent to:
        // int  _CurrentKernelIndex; // known as a 'backinng field'
        // public static int CurrentKernelIndex
        // {  get { return _CurrentKernelIndex; }
        //    set { _CurrentKernelIndex = value;}
        // }

        static DanbiKernelDict()
        {
            KernalDic = new Dictionary<EDanbiKernelKey, int>();
            //CurrentKernalKey = "";
        }

        public static bool AddKernalIndexWithKey(EDanbiKernelKey key, int kernalIndex)
        {
            if (KernalDic.ContainsKey(key))
            {
                return false;      // if the key is already in the dict, return false
            }
            else
            {
                KernalDic.Add(key, kernalIndex);
                return true;
            }
        }

        public static bool AddKernalIndexWithKey((EDanbiKernelKey, int) keyKernalIndexPair)
        {
            if (KernalDic.ContainsKey(keyKernalIndexPair.Item1))
            {
                return false;       // if the key is already in the dict, return false
            }
            else
            {

                KernalDic.Add(keyKernalIndexPair.Item1, keyKernalIndexPair.Item2);
                return true;

            }

        }

        public static int GetKernalIndex(EDanbiKernelKey key)
        {
            return KernalDic[key];
        }
    };

};