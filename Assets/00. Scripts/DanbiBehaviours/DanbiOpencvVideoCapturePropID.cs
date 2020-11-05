using OpenCVForUnity.VideoioModule;
namespace Danbi
{
    public static class DanbiOpencvVideoCapturePropID
    {
        /// <summary>
        /// 0-based index of the frame to be decoded/caputred next.
        /// </summary>
        public readonly static int frame_pos = Videoio.CAP_PROP_POS_FRAMES;
        /// <summary>
        /// Relative position of the video file
        /// 0 - start of the file, 1 - end of the file
        /// </summary>
        public readonly static int pos_avi_ratio = Videoio.CAP_PROP_POS_AVI_RATIO;
        /// <summary>
        /// Width of the frames in the video stream.
        /// </summary>
        public readonly static int frame_width = Videoio.CAP_PROP_FRAME_WIDTH;
        /// <summary>
        /// Height of the frames in the video stream.
        /// </summary>
        public readonly static int frame_height = Videoio.CAP_PROP_FRAME_HEIGHT;
        /// <summary>
        /// Frame rate
        /// </summary>
        public readonly static int fps = Videoio.CAP_PROP_FPS;
        /// <summary>
        /// 4-character code of codec 
        /// https://www.fourcc.org/codecs.php
        /// </summary>
        public readonly static int fourcc = Videoio.CAP_PROP_FOURCC;
        /// <summary>
        /// Number of frames in the video file
        /// </summary>
        public readonly static int frame_count = Videoio.CAP_PROP_FRAME_COUNT;
        /// <summary>
        /// Format of the Mat Objects returned by retrieve().
        /// </summary>
        public readonly static int format = Videoio.CAP_PROP_FORMAT;        
    };
};