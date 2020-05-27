using System.IO;

using Unity.Collections;

using UnityEditor.Media;

using UnityEngine;

public class ImageConvertToVideo {
  static public void EncodeVideo() {
    Debug.Log("Enconde Start");
    VideoInfo info = VideoInfo.videoInfo;
    VideoTrackAttributes videoAttr = new VideoTrackAttributes {
      frameRate = new MediaRational(info.FrameRate),
      width = info.width,
      height = info.height,
      includeAlpha = info.includeAlpha
    };

    AudioTrackAttributes audioAttr = new AudioTrackAttributes {
      sampleRate = new MediaRational(48000),
      channelCount = 2,
      language = "fr"
    };

    int sampleFramesPerVideoFrame = audioAttr.channelCount *
        audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;

    string encodedFilePath = Path.Combine(Application.dataPath + "/Resources/ConvertVideo", "my_movie.mp4");

    Texture2D tex = new Texture2D((int)videoAttr.width, (int)videoAttr.height, TextureFormat.RGBA32, false);

    using (MediaEncoder encoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr))
    using (NativeArray<float> audioBuffer = new NativeArray<float>(sampleFramesPerVideoFrame, Allocator.Temp)) {
      foreach (Texture2D _tex in info.TexList) {
        encoder.AddFrame(_tex);
      }
      //for (int i = 0; i < info.TotalFrameCount; ++i)
      //{
      //    // Fill 'tex' with the video content to be encoded into the file for this frame.
      //    // ...



      //    // Fill 'audioBuffer' with the audio content to be encoded into the file for this frame.
      //    // ...
      //    encoder.AddSamples(audioBuffer);
      //}
    }
  }
}