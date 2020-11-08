using OpenCVForUnity.VideoioModule;
namespace Danbi
{
    public enum EDanbiOpencvCodec_fourcc_
    {
        h264,
        h265,
        divx,
        mpeg4,
        hevc
    };

    public static class DanbiOpencvVideoCodec_fourcc
    {
        /// <summary>
        /// make fourcc form of videoCodec!
        /// (-999 -> error)
        /// </summary>
        /// <param name="codec"></param>
        /// <returns></returns>
        public static int get_fourcc_videoCodec(EDanbiOpencvCodec_fourcc_ codec)
        {
            // switch (codec)
            // {
            //     case EDanbiOpencvCodec_fourcc_.h264:
            //         return VideoWriter.fourcc('X', '2', '6', '4');

            //     case EDanbiOpencvCodec_fourcc_.h265:
            //     case EDanbiOpencvCodec_fourcc_.hevc:
            //         return VideoWriter.fourcc('H', 'E', 'V', 'C');

            //     case EDanbiOpencvCodec_fourcc_.mpeg4:
            //         return VideoWriter.fourcc('M', 'P', '4', '2');

            //     case EDanbiOpencvCodec_fourcc_.divx:
            //         return VideoWriter.fourcc('D', 'I', 'V', 'X');

            //     default:
            //         return -999;
            // }
            // return VideoWriter.fourcc('M', 'J', 'P', 'G'); -> error to write a video
            return VideoWriter.fourcc('D', 'I', 'V', 'X');
            // return VideoWriter.fourcc('X', '2', '6', '4');
        }
    };
};