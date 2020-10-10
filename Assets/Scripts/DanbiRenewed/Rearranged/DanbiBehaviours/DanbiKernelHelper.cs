using System.Collections.Generic;

namespace Danbi
{
    public static class DanbiKernelHelper
    {
        public static Dictionary<EDanbiKernelKey, int> KernalDic { get; set; } = new Dictionary<EDanbiKernelKey, int>();
        public static int CurrentKernelIndex { get; set; }

        public static void AddKernalIndexWithKey(EDanbiKernelKey key, int kernalIndex)
        {
            KernalDic.Add(key, kernalIndex);
        }

        public static void AddKernalIndexWithKey(params (EDanbiKernelKey, int)[] keyKernalIndexPair)
        {
            foreach (var e in keyKernalIndexPair)
            {
                KernalDic.Add(e.Item1, e.Item2);
            }
        }

        public static int CalcCurrentKernelIndex(EDanbiPrewarperSetting_MeshType meshType, EDanbiPrewarperSetting_PanoramaType panoramaType)
        {
            int finder = 0x0000;
            switch (meshType)
            {
                case EDanbiPrewarperSetting_MeshType.Custom_Dome:
                    finder = 0x0100;
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Cone:
                    finder = 0x1000;
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Cylinder:
                    finder = 0x10000;
                    break;

                case EDanbiPrewarperSetting_MeshType.Custom_Pyramid:
                    finder = 0x100000;
                    break;

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

            return KernalDic[(EDanbiKernelKey)finder];
        }
    };

};