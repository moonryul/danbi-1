namespace Danbi
{
    public static class DanbiStringColorHelper
    {
        internal static (string, string) _format(string colorID)
        {
            return ($"<color={colorID}>", "</color>");
        }
        /// <summary>
        /// get colored string tags (string, string).
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static (string startTag, string endTag) getStringColor(EDanbiStringColor col)
        {
            switch (col)
            {
                case EDanbiStringColor.black:
                    return _format("black");

                case EDanbiStringColor.grey:
                    return _format("grey");

                case EDanbiStringColor.violet:
                    return _format("violet");

                case EDanbiStringColor.red:
                    return _format("red");

                case EDanbiStringColor.orange:
                    return _format("orange");

                case EDanbiStringColor.yellow:
                    return _format("yellow");

                case EDanbiStringColor.teal:
                    return _format("teal");

                case EDanbiStringColor.blue:
                    return _format("blue");

                case EDanbiStringColor.green:
                    return _format("green");

                case EDanbiStringColor.lime:
                    return _format("lime");

                default:
                    return _format("black");
            }

            // only in c#8.0 (maybe 2020.2)
            // return col switch
            // {
            //     EDanbiStringColor.black => __format("black"),
            //     EDanbiStringColor.grey => __format("grey"),
            //     EDanbiStringColor.violet => __format("violet"),
            //     EDanbiStringColor.red => __format("red"),
            //     EDanbiStringColor.orange => __format("orange"),
            //     EDanbiStringColor.yellow => __format("yellow"),
            //     EDanbiStringColor.teal => __format("teal"),
            //     EDanbiStringColor.blue => __format("blue"),
            //     EDanbiStringColor.green => __format("green"),
            //     EDanbiStringColor.lime => __format("lime"),
            //     _ => __format("black"),
            // };
        }
    };
};