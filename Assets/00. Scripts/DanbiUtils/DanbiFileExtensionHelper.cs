namespace Danbi
{
    public static class DanbiFileExtensionHelper
    {
        public static string getVideoExtString(EDanbiVideoType vidType)
        {
            switch (vidType)
            {
                case EDanbiVideoType.mp4:
                    return ".mp4";

                case EDanbiVideoType.avi:
                    return ".avi";

                case EDanbiVideoType.m4v:
                    return ".m4v";

                case EDanbiVideoType.mov:
                    return ".mov";

                case EDanbiVideoType.webm:
                    return ".webm";

                case EDanbiVideoType.wmv:
                    return ".wmv";

                default:
                    return ".error";
            }
        }

        public static EDanbiVideoType getVideoExt(string vidExt)
        {
            if (vidExt == ".mp4")
            {
                return EDanbiVideoType.mp4;
            }
            else if (vidExt == ".avi")
            {
                return EDanbiVideoType.avi;
            }
            else if (vidExt == ".m4v")
            {
                return EDanbiVideoType.m4v;
            }
            else if (vidExt == ".mov")
            {
                return EDanbiVideoType.mov;
            }
            else if (vidExt == ".webm")
            {
                return EDanbiVideoType.webm;
            }
            else
            {
                DanbiUtils.LogErr("not valid video extension! using default ext -> .mp4");
                return EDanbiVideoType.mp4;
            }
        }
    };
};