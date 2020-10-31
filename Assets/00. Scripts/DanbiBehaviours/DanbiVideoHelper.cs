namespace Danbi
{
    public static class DanbiVideoHelper
    {
        // References.
        // https://infodbbase.tistory.com/90 
        // https://trac.ffmpeg.org/wiki/Concatenate#differentcodec <- ffmpeg concat cmd arguments
        // http://mwultong.blogspot.com/2006/11/dos-file-copy-command.html <- copy/ move cmd arguments

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetFileNames"></param>
        /// <param name="outputFileName"></param>
        /// <param name="ffmpegExecutableLocation"></param>
        public static System.Collections.IEnumerator ConcatVideoClips(string ffmpegExecutableLocation,
                                                                           string outputFileLocation,
                                                                           string outputFileName,
                                                                           string[] targetFileNames,
                                                                           EDanbiVideoType ext)
        {
            // cmd argument "merging video with ffmpeg.exe"
            // ffmpeg -f concat -safe 0 -i VideoList.txt(target video clips names) -c copy output.mp4(output video file name)
            // string arg = $"ffmpeg -f concat -safe 0 -i {targetVideoClipFiles} -c copy {outputFileName}";
            string extstr = DanbiFileExtensionHelper.getVideoExtString(ext);
            string outputFinal = $"{outputFileName}{extstr}";
            DanbiUtils.Log(outputFinal, EDanbiStringColor.teal);

            // Write the target video clips.
            using (var writer = new System.IO.StreamWriter($"{outputFileLocation}/{outputFileName}.txt", true))
            {
                for (var i = 0; i < targetFileNames.Length; ++i)
                {
                    writer.WriteLine($"file '{targetFileNames[i]}'");
                }
                writer.Close();
            }

            if (new System.IO.FileInfo($"{outputFileLocation}/{outputFinal}").Exists)
            {
                System.IO.File.Delete($"{outputFileLocation}/{outputFinal}");
                yield return null;
            }

            string arg = $"ffmpeg -f concat -safe 0 -i {outputFileName}.txt -c copy {outputFinal}";
            // DanbiUtils.Log(arg, EDanbiStringColor.teal);
            DanbiFileSys.OpenProcessWithArguments(outputFileLocation, arg);
            System.Diagnostics.Process.Start(@"" + outputFileLocation);
        }
    };
};