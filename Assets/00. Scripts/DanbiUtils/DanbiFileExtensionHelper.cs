namespace Danbi
{
    public static class DanbiFileExtensionHelper
    {
        public static string getVideoExtString(EDanbiVideoExt vidType)
        {
            switch (vidType)
            {
                case EDanbiVideoExt.mp4:
                    return ".mp4";

                case EDanbiVideoExt.avi:
                    return ".avi";

                case EDanbiVideoExt.m4v:
                    return ".m4v";

                case EDanbiVideoExt.mov:
                    return ".mov";

                case EDanbiVideoExt.webm:
                    return ".webm";

                case EDanbiVideoExt.wmv:
                    return ".wmv";

                default:
                    return ".error";
            }
        }

        public static EDanbiVideoExt getVideoExt(string vidExt)
        {
            if (vidExt == ".mp4")
            {
                return EDanbiVideoExt.mp4;
            }
            else if (vidExt == ".avi")
            {
                return EDanbiVideoExt.avi;
            }
            else if (vidExt == ".m4v")
            {
                return EDanbiVideoExt.m4v;
            }
            else if (vidExt == ".mov")
            {
                return EDanbiVideoExt.mov;
            }
            else if (vidExt == ".webm")
            {
                return EDanbiVideoExt.webm;
            }
            else
            {
                DanbiUtils.LogErr("not valid video extension! using default ext -> .mp4");
                return EDanbiVideoExt.mp4;
            }
        }
    };
};