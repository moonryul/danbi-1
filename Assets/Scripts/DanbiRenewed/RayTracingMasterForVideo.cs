using System.Collections;
using System.IO;
using Unity.Collections;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class RayTracingMasterForVideo : RayTracingMaster {
  #region Exposed Variables

  [SerializeField] VideoClip videoToPlay;

  [SerializeField, Space(20)] string ExtractedImgFolderDirName;
  [SerializeField] string ExtractedImgFileName;

  [SerializeField, Space(20)] string EncodedVideoFolderDirName;
  [SerializeField] string EncodedVideoFileName;

  [SerializeField, Space(20)] int CurrentFrameCounter;
  [SerializeField, Space(20)] int Test_MaxFrameCounter = 120;

  #endregion

  #region Internal Variables

  VideoPlayer VideoPlayer;
  VideoSource VideoSource;

  AudioSource AudioSource;

  public Texture2D ExtractedTex;
  RenderTexture[] CopiedResultRTArr;
  public float[] AudioClipDataArr;

  WaitUntil WaitUntilVideoPrepared;
  WaitUntil WaitUntilVideoFrameExtracted;
  WaitUntil WaitUntilFrameIsEncoded;
  WaitUntil WaitUntilPredistortedImageReady;

  Coroutine CoroutineHandle_ProcessVideo;    

  /// <summary>
  /// Check the next frame is ready (received) for only one frame.
  /// </summary>
  bool bNextFrameReady = false;  

  bool bCurrentFrameEncoded = false;

  bool bCurrentSoundEncoded = false;

  AudioTrackAttributes audioAttr;

  VideoTrackAttributes videoAttr;

  int sampleFramesPerVideoFrame;

  string encodedFilePath;

  #endregion

  protected override void Start() {
    Application.runInBackground = true;

    #region Bind yieldinstructions as lambdas expression.

    WaitUntilVideoPrepared = new WaitUntil(() => VideoPlayer.isPrepared);

    // WaitUntilVideoFrameExtracted = new WaitUntil(() => (CurrentFrameCounter != 0 && CurrentFrameCounter % VideoFrameNumbersForOneTimeConversion == 0) ||
    //                                                    CurrentFrameCounter == (int) VideoPlayer.frameCount - 1); // or When the current frame counter hits the last frame count of the video.

    WaitUntilVideoFrameExtracted = new WaitUntil(() =>
            bNextFrameReady
    //ExtractedTexturesArr[CurrentFrameCounter % ArrayMax] != null &&
    //CurrentFrameCounter % ArrayMax == 0 &&
    //CurrentFrameCounter != 0 ||
    //CurrentFrameCounter < (int) VideoPlayer.frameCount - 1
    );

    WaitUntilFrameIsEncoded = new WaitUntil(() => bCurrentFrameEncoded);
    WaitUntilPredistortedImageReady = new WaitUntil(() => bPredistortedImageReady);

    #endregion

    #region Prepare videos

    SimulatorMode = Danbi.EDanbiSimulatorMode.PREPARE;

    VideoPlayer = GetComponent<VideoPlayer>();
    AudioSource = GetComponent<AudioSource>();

    // Disable Play on Awake for both Video and Audio
    VideoPlayer.playOnAwake = false;
    AudioSource.playOnAwake = false;

    VideoPlayer.source = VideoSource.VideoClip;
    VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    VideoPlayer.EnableAudioTrack(0, true);
    VideoPlayer.SetTargetAudioSource(0, AudioSource);

    // Set video To Play then prepare Audio to prevent Buffering
    VideoPlayer.clip = videoToPlay;

    // Enables the frameReady events, it will be invoked when a frame is ready to be drawn.
    VideoPlayer.sendFrameReadyEvents = true;
    // bind the event to invoke explicitly when a new frame is ready.
    VideoPlayer.frameReady += OnReceivedNewFrame;

    VideoPlayer.Prepare();

    CurrentScreenResolutions.x = (int)VideoPlayer.width;
    CurrentScreenResolutions.y = (int)VideoPlayer.height;

    encodedFilePath = Path.Combine(Application.dataPath + $"/Resources/{EncodedVideoFolderDirName}/",
        $"{EncodedVideoFileName}.mp4");

    videoAttr = new VideoTrackAttributes {
      frameRate = new MediaRational((int)VideoPlayer.frameRate),
      width = VideoPlayer.width,
      height = VideoPlayer.height,
      includeAlpha = false
    };

    audioAttr = new AudioTrackAttributes {
      sampleRate = new MediaRational(48000),
      channelCount = 2,
      language = "fr"
    };

    sampleFramesPerVideoFrame =
        audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
    AudioClipDataArr = new float[sampleFramesPerVideoFrame];

    #endregion

    base.Start();

    OnInitCreateDistortedImageFromVideoFrame();
  } // Start()

  protected override void OnDisable() {
    base.OnDisable();
    if (CoroutineHandle_ProcessVideo != null) {
      StopCoroutine(CoroutineHandle_ProcessVideo);
      CoroutineHandle_ProcessVideo = null;
    }
  } // OnDisable()

  public void OnInitCreateDistortedImageFromVideoFrame() {
    CoroutineHandle_ProcessVideo = StartCoroutine(Coroutine_ProcessVideo());
  }

  IEnumerator Coroutine_ProcessVideo() {
    // Wait for (!videoPlayer.isPrepared)
    yield return WaitUntilVideoPrepared;

    VideoPlayer.sendFrameReadyEvents = true;
    VideoPlayer.Play();
    AudioSource.Play();

    var distortedToTex2D = new Texture2D((int)VideoPlayer.width, (int)VideoPlayer.height, TextureFormat.RGBA32, true);

    using (var videoEncoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr))
    using (var audioBuffer = new NativeArray<float>(sampleFramesPerVideoFrame, Allocator.Temp)) {
      // 1. While CurrentFrameCounter is lower than VideoPlayer.framecount(Total frame Count).
      while (CurrentFrameCounter < (int)VideoPlayer.frameCount) {

        //  2. Wait until the next frame is ready (the next frame is extracted from the video).
        while (!bNextFrameReady) {
          yield return null;
        }

        // 3. Process the extracted Texture to the predistorted texture.
        yield return ProcessExtractedTexToPredistortedTex(distortedToTex2D);
        

        // 4. Encoding process.
        // Add(Encode) predistorted images to the predistorted video.
        bCurrentFrameEncoded = videoEncoder.AddFrame(distortedToTex2D);
        
        if (!bCurrentFrameEncoded) {
          Debug.LogError("<color=red>Failed to VideoEncoder.AddFrame()</color>", this);
        }


        if (AudioSource.clip != null) {
          Debug.LogWarning($"<color=blue><b>AudioSource.clip is no longer invalid!</b></color>", this);
        }

        // Add(Encode) sound of video to the predistorted video.
        //AudioSource.clip.GetData(AudioClipDataArr, 0);
        //audioBuffer.CopyFrom(AudioClipDataArr);
        //bCurrentSoundEncoded = videoEncoder.AddSamples(0, audioBuffer);


        // if (!bCurrentSoundEncoded)
        // {
        //     Debug.LogError("<color=red>Failed to VideoEncoder.AddSample()</color>", this);
        // }

        Debug.Log($"<color=green>{CurrentFrameCounter} frames are encoded.</color>");

        // 5. Resume the OnReceivedNewFrame(). (Extracting one frame of the video). Encoding of 1 frame is finished!
        bNextFrameReady = false;
        VideoPlayer.sendFrameReadyEvents = true;
        VideoPlayer.Play();
        AudioSource.Play();

        // 6.
        yield return null;        

        // prevent to iterate all the frames for short iteration time for each video.
        if (CurrentFrameCounter > Test_MaxFrameCounter) break;
      } // (CurrentFrameCounter < (int) VideoPlayer.frameCount)      
    }

    // Resource disopsing.
    DestroyImmediate(distortedToTex2D, true);    
    Debug.Log("Convert all the frames To Video is Complete");

    // Prevent the call of OnRenderImage();
    bPredistortedImageReady = true; // Do not call renderer of the predistorted Image.

    yield break;
  } // Coroutine_ProcessVideo()

  IEnumerator ProcessExtractedTexToPredistortedTex(Texture2D distortedResult) {
    // 1. Distort the actual Image.
    // Make the predistorted Image Ready.
    OnInitCreateDistortedImage(ExtractedTex);

    // 2. Wait (yield) until the predistorted image is generated (wait for screen sampling (default : 30)).
    yield return WaitUntilPredistortedImageReady;

    RenderTexture prevRT = RenderTexture.active;

    RenderTexture currentRT = ConvergedRenderTexForNewImage;
    RenderTexture.active = currentRT;

    // 3. ReadPixels() from the current active RenderTexture.
    distortedResult.ReadPixels(
        new Rect(0, 0, ConvergedRenderTexForNewImage.width, ConvergedRenderTexForNewImage.height), 0, 0);
    distortedResult.Apply();

    // 4. Restore the previous RenderTexture at the last frame.
    RenderTexture.active = prevRT;
    
    yield break;
  } // Coroutine ProcessExtractedTexToPredistortedTex(Texture2D)

  void OnReceivedNewFrame(VideoPlayer source, long frameIdx) {
    RenderTexture prevRT = RenderTexture.active;
    // Get the source texture (current frame).
    RenderTexture currentRT = source.texture as RenderTexture;
    // Set the current render texture as an active render texture.
    RenderTexture.active = currentRT;

    Texture2D sourceFrameTex = new Texture2D(1280, 720);
    if (sourceFrameTex.width != currentRT.width || sourceFrameTex.height != currentRT.height) {
      sourceFrameTex.Resize(currentRT.width, currentRT.height);
    }

    // 1. Perform on GPU-side
    //Graphics.CopyTexture(RenderTexture.active, videoFrame);
    // 2. Perform on CPU-side
    sourceFrameTex.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
    sourceFrameTex.Apply();
    //
    // NOTE: Since Texture2D.ReadPixels() is performed on CPU-side
    // it can be a great performant between choosing them.
    // 
    // restore the active render texture by the previous render texture.
    RenderTexture.active = prevRT;

    //SaveExtractedImagesToJPG(videoFrame);

    // Pause the video while the frame is extracted.
    VideoPlayer.Pause();
    AudioSource.Pause();
    VideoPlayer.sendFrameReadyEvents = false;

    bNextFrameReady = true;

    //ExtractedAudioSource = source.GetTargetAudioSource((ushort) frameIdx);
    //float[] srcClipData = new float[4000];
    // var srcAudioSource = source.GetTargetAudioSource(0); 
    // var srcAudioClip = srcAudioSource.clip;
    //srcAudioClip.GetData(srcClipData, 0);
    //AudioSource.clip.SetData(srcClipData, 0);
    //AudioSource.clip.GetData(AudioClipDataArr, 0);
    //AudioSource.clip = srcAudioClip;

    ExtractedTex = sourceFrameTex;
    CurrentFrameCounter = (int)frameIdx;
    Debug.Log($"Current Video Frame Count : {CurrentFrameCounter} / {source.frameCount}", this);
  } // OnReceivedNewFrame()

  #region UNUSED
/// <summary>
  /// Save the extracted image into the real file (jpg).
  /// </summary>
  /// <param name="extractedImg"></param>
  void SaveImageToJPG(Texture2D extractedImg, string dirName, string fileName) {
    if (extractedImg == null) return;

    byte[] savedImg = extractedImg.EncodeToJPG();
    File.WriteAllBytes(
        $"{Application.dataPath}/Resources/Video2Img/{ExtractedImgFolderDirName}/{ExtractedImgFileName}_frame_{CurrentFrameCounter}.jpg",
        savedImg);
  }

  void SaveTexture2dsToJPG(Texture2D[] tex2dArr, string dirName, string fileName) {
    if (tex2dArr.Length == 0) return;

    Texture2D fwdTex2d = new Texture2D(tex2dArr[0].width, tex2dArr[0].height);
    for (int i = 0; i < tex2dArr.Length; ++i) {
      SaveImageToJPG(fwdTex2d, dirName, fileName);
    }
  }

  void SaveRenderTexturesToJPG(RenderTexture[] rtArr, string dirName, string fileName) {
    if (rtArr.Length == 0) return;

    Texture2D fwdTex2d = new Texture2D(rtArr[0].width, rtArr[0].height, TextureFormat.RGB24, false);
    for (int i = 0; i < rtArr.Length; ++i) {
      RenderTexture prevRT = RenderTexture.active;
      RenderTexture.active = rtArr[i];
      fwdTex2d.ReadPixels(new Rect(0, 0, rtArr[i].width, rtArr[i].height), 0, 0);
      fwdTex2d.Apply();
      RenderTexture.active = prevRT;

      SaveImageToJPG(fwdTex2d, dirName, fileName);
    }
  }

  /// <summary>
  /// This method and OnRenderImage() are called at every frame in parellel
  /// </summary>
  /// <param name="extractedTexturesList"></param>
  /// <returns></returns>
  // IEnumerator ConvertSavedImagesToPredistortedImages(Texture2D[] extractedTexturesArr,
  //     RenderTexture[] distortedRenderTexturesArr)
  // {
  //     Debug.Log($"Image is being converted to the distorted image");
  //     for (int i = 0; i < VideoFrameNumbersForOneTimeConversion; ++i)
  //     {
  //         // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
  //         OnInitCreateDistortedImage(extractedTexturesArr[i]);
  //
  //         // Wait until the predistorted image is created but yield immediately when the image isn't ready.
  //         // The number of sampling for rendering is 30
  //         // If bPredistortedImageReady is false -> yield return immediately and then OnRenderImage() will be called!
  //         // otherwise go to the next line.
  //         yield return WaitUntilPredistortedImageReady;
  //
  //         // to solve the overwritten problem of the new distorted images as RenderTexture (ConvergedRenderTexForNewImage),
  //         // TEST #1: Copy the image from ConvergedRnederTexForNewImage to the new RenderTexture declared as a local variable.
  //         //RenderTexture copied = new RenderTexture(ConvergedRenderTexForNewImage);
  //         //copied.enableRandomWrite = true;
  //         //Graphics.CopyTexture(ConvergedRenderTexForNewImage, CopiedResultTexArr[i]);
  //         Graphics.CopyTexture(extractedTexturesArr[i], CopiedResultTexArr[i]);
  //         // the predistorted image (ConvergedRenderTexForNewImage) is ready.
  //         distortedRenderTexturesArr[i] = ConvergedRenderTexForNewImage;
  //     }
  //
  //     yield break;
  //     //SaveRenderTexturesToJPG(DistortedRenderTexturesList.ToArray(), DistortedImgFolderDirName, DistortedImgFileName);
  // }
}

//IEnumerator ConvertRenderTexturesToPredistortedImages(RenderTexture[] renderTexturesArr,
//                                                      List<RenderTexture> distortedRenderTexturesList) {
//  for (int i = 0; i < VideoFrameNumbersForOneTimeConversion; ++i) {
//    // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
//    // DanbiSimulatorMode (PREPARE -> CAPTURE).
//    OnInitCreateDistortedImage2(renderTexturesArr[i]);
//    // Wait until the predistorted image is created but yield immediately when the image isn't ready.
//    yield return WaitUntilRenderFinished; // move down if (!bPredistortedImageReady)
//    //
//    distortedRenderTexturesList.Add(ConvergedRenderTexForNewImage);
//    //RenderTexture.active = DistortedRenderTexturesList[i];
//  }
//  //SaveRenderTexturesToJPG(distortedRenderTexturesList.ToArray(), DistortedImgFolderDirName, DistortedImgFileName);
//}

// void EncodeVideoFromPredistortedImages(RenderTexture[] predistortedImages) {
//   //videoAttr.width = (uint)convertedToTex2d.width;
//   //videoAttr.height = (uint)convertedToTex2d.height;    
//   MediaEncoder videoEncoder = new MediaEncoder(encodedVideoFilePath, videoAttr); /*, audioAttr*/
//   // Unity.Collections.NativeArray<float> audioBuf;

//   //using (var encoder = new MediaEncoder(encodedVideoFilePath, videoAttr/*, audioAttr*/))
//   //using (var audioBuf = new Unity.Collections.NativeArray<float>(sampleFramesPerVideoFrame /*2*/, Unity.Collections.Allocator.Temp)) {
//   for (int i = 0; i < predistortedImages.Length; ++i) {
//     Debug.Log($"Current encoding idx {i} of {ExtractedTexturesArr.Length}");

//     RenderTexture prevRT = RenderTexture.active;
//     RenderTexture.active = predistortedImages[i];
//     convertedToTex2d.ReadPixels(new Rect(0, 0, predistortedImages[i].width, predistortedImages[i].height), 0, 0);
//     convertedToTex2d.Apply();
//     // TODO: CopyTexture().
//     RenderTexture.active = prevRT;
//     videoEncoder.AddFrame(convertedToTex2d);

//     // bool bAddFrameResult = videoEncoder.AddFrame(convertedToTex2d);
//     // if (!bAddFrameResult) {
//     //   Debug.Log($"Adding frame is failed at {i} frame!", this);
//     // }
//     //encoder.AddSamples(audioBuf);
//   }
//   // videoEncoder.Dispose();
//   // DestroyImmediate(convertedToTex2d);
//   //}

//   //string outputPath = $"";

//   //using (ITimeline timeline = new DefaultTimeline()) {
//   //  IGroup group = timeline.AddVideoGroup(32,
//   //                                        DistortedRenderTexturesArr[0].width,
//   //                                        DistortedRenderTexturesArr[0].height);
//   //  var firstVideoClip = group.AddTrack().AddVideo(encodedVideoFilePaths[i])
//   //}
// }

//   void EncodeVideoFromPredistortedImages(Texture2D[] predistortedImages) {
//     for (int i = 0; i < predistortedImages.Length; ++i) {
//       Debug.Log($"Current encoding idx {i} of {ExtractedTexturesArr.Length}");

//       bool bAddFrameResult = videoEncoder.AddFrame(predistortedImages[i]);

//       if (!bAddFrameResult) {
//         Debug.Log($"Adding frame is failed at {i} frame!", this);
//       }
//       //encoder.AddSamples(audioBuf);
//     }
//   }
// }
#endregion