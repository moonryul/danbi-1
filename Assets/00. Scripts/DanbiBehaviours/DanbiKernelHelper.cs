using System.Collections.Generic;

namespace Danbi
{
    public static class DanbiKernelHelper
    {
        static Dictionary<EDanbiKernelKey, int> KernalDic { get; set; } = new Dictionary<EDanbiKernelKey, int>();
        public static int CurrentKernelIndex { get; set; }

        /// <summary>
        /// No overlaps of key allowed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="kernalIndex"></param>        
        public static void AddKernalIndexWithKey(EDanbiKernelKey key, int kernalIndex)
        {
            if (KernalDic.ContainsKey(key))
            {
                return;
            }

            KernalDic.Add(key, kernalIndex);
        }

        /// <summary>
        /// No overlaps of key allowed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        public static void AddKernalIndexWithKey(params (EDanbiKernelKey key, int index)[] keyKernalIndexPair)
        {
            foreach (var (k, i) in keyKernalIndexPair)
            {
                if (KernalDic.ContainsKey(k))
                {
                    continue;
                }

                KernalDic.Add(k, i);
            }
        }

        /// <summary>
        /// Calculate the kernel Index by bitwise shift operation with the meshType and the panoramaType.
        /// </summary>
        /// <param name="meshType"></param>
        /// <param name="panoramaType"></param>
        /// <returns></returns>
        public static int CalcCurrentKernelIndex(EDanbiPrewarperSetting_MeshType meshType, EDanbiPrewarperSetting_PanoramaType panoramaType)
        {
            int finder = 0x0000;
            switch (meshType)
            {
                case EDanbiPrewarperSetting_MeshType.Custom_Dome:
                    finder = 0x0100;
                    break;

                // case EDanbiPrewarperSetting_MeshType.Custom_Cone:
                //     finder = 0x1000;
                //     break;

                case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
                    finder = 0x10000;
                    break;

                // case EDanbiPrewarperSetting_MeshType.Custom_Pyramid:
                //     finder = 0x100000;
                //     break;

                default:
                    break;
            }

            switch (panoramaType)
            {
                case EDanbiPrewarperSetting_PanoramaType.Cube_panorama:
                    finder |= 0x0001;
                    break;

                case EDanbiPrewarperSetting_PanoramaType.Cylinder_panorama:
                    finder |= 0x0010;
                    break;
            }
            
            if (KernalDic.ContainsKey((EDanbiKernelKey)finder))
            {
                return KernalDic[(EDanbiKernelKey)finder];
            }
            else
            {
                return -9999;
            }
        }
    };

};