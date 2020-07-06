using System.Collections;
using System.IO;
using Unity.Collections;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Experimental.Video;
using UnityEngine.Experimental.Audio;
using UnityEngine.Profiling;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class RayTracingMasterForVideo : RayTracingMaster {
  #region Exposed Variables

  [SerializeField, Header("Target Video to distort")]
  VideoClip videoToPlay;

  [SerializeField, Header("It's for Extracted Img"), Space(20)]
  string ExtractedImgFolderDirName;

  [SerializeField, Header("It's for Extracted Img")]
  string ExtractedImgFileName;

  [SerializeField, Header("It's for distorted video"), Space(20)]
  string EncodedVideoFolderDirName;

  [SerializeField, Header("It's for distorted video")]
  string EncodedVideoFileName;

  [SerializeField, Space(20)]
  int CurrentFrameCounter;

  [SerializeField, Header("threshold to make short of entire the processing time"), Space(20)]
  int Test_MaxFrameCounter = 120;

  #endregion

  #region Internal Variables

  /// <summary>
  /// 
  /// </summary>
  VideoPlayer VideoPlayer;

  /// <summary>
  /// 
  /// </summary>
  VideoSource VideoSource;

  /// <summary>
  /// 
  /// </summary>
  AudioSource AudioSource;

  /// <summary>
  /// 
  /// </summary>
  AudioSampleProvider audioSampleProvider;

  /// <summary>
  /// 
  /// </summary>
  public Texture2D ExtractedTex;

  /// <summary>
  /// 
  /// </summary>
  NativeArray<float> ExtractedAudioBuf;

  /// <summary>
  /// 
  /// </summary>
  RenderTexture[] CopiedResultRTArr;

  /// <summary>
  /// 
  /// </summary>
  public float[] AudioClipDataArr;

  /// <summary>
  /// 
  /// </summary>
  WaitUntil WaitUntilVideoPrepared;

  /// <summary>
  /// 
  /// </summary>
  WaitUntil WaitUntilVideoFrameExtracted;

  /// <summary>
  /// 
  /// </summary>
  WaitUntil WaitUntilAudioSamplesExtracted;

  /// <summary>
  /// 
  /// </summary>
  WaitUntil WaitUntilFrameIsEncoded;

  /// <summary>
  /// 
  /// </summary>
  WaitUntil WaitUntilPredistortedImageReady;

  /// <summary>
  /// 
  /// </summary>
  Coroutine CoroutineHandle_ProcessVideo;

  /// <summary>
  /// Check the next frame is ready (received) for only one frame.
  /// </summary>
  bool bNextFrameReady = false;

  /// <summary>
  /// 
  /// </summary>
  bool bNextAudioSamplesReady = false;

  /// <summary>
  /// 
  /// </summary>
  bool bCurrentFrameEncoded = false;

  /// <summary>
  /// 
  /// </summary>
  bool bCurrentSoundEncoded = false;

  /// <summary>
  /// 
  /// </summary>
  AudioTrackAttributes audioAttr;

  /// <summary>
  /// 
  /// </summary>
  VideoTrackAttributes videoAttr;

  /// <summary>
  /// 
  /// </summary>
  int sampleFramesPerVideoFrame;

  /// <summary>
  /// 
  /// </summary>
  string encodedFilePath;

  #endregion

  protected override void Start() {
    // 1. Set the appli is able to be run in background.
    Application.runInBackground = true;

    #region 2. Bind yieldinstructions as lambdas expression.

    WaitUntilVideoPrepared = new WaitUntil(() => VideoPlayer.isPrepared);
    WaitUntilVideoFrameExtracted = new WaitUntil(() => bNextFrameReady);
    WaitUntilAudioSamplesExtracted = new WaitUntil(() => bNextAudioSamplesReady);
    WaitUntilFrameIsEncoded = new WaitUntil(() => bCurrentFrameEncoded);
    WaitUntilPredistortedImageReady = new WaitUntil(() => bPredistortedImageReady);

    #endregion

    #region Prepare videos

    SimulatorMode = Danbi.EDanbiSimulatorMode.PREPARE;
    // 3. Initialise the resources
    VideoPlayer = GetComponent<VideoPlayer>();
    AudioSource = GetComponent<AudioSource>();
    // Disable Play on Awake for both Video and Audio
    VideoPlayer.playOnAwake = false;
    AudioSource.playOnAwake = false;
    VideoPlayer.source = VideoSource.VideoClip;
    VideoPlayer.controlledAudioTrackCount = 1;
    VideoPlayer.audioOutputMode = VideoAudioOutputMode.APIOnly;
    VideoPlayer.clip = videoToPlay;

    //VideoPlayer.sendFrameReadyEvents = true;
    // bind the event to invoke explicitly when a new frame is ready.
    VideoPlayer.prepareCompleted += OnVideoPlayerPrepared;

    //VideoPlayer.frameReady += OnVideoFrameReceived;


    //VideoPlayer.prepareCompleted += OnVideoPrepared;
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

    audioAttr = new AudioTrackAttributes { sampleRate = new MediaRational(48000), channelCount = 2, language = "fr" };

    sampleFramesPerVideoFrame =
      audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
    AudioClipDataArr = new float[sampleFramesPerVideoFrame];

    #endregion

    base.Start();

    OnInitCreateDistortedImageFromVideoFrame();
  } // Start()

  protected override void OnDisable() {
    base.OnDisable();

    // if (ExtractedAudioBuf != null) {
    //   ExtractedAudioBuf.Dispose();
    // }

    if (CoroutineHandle_ProcessVideo != null) {
      StopCoroutine(CoroutineHandle_ProcessVideo);
      CoroutineHandle_ProcessVideo = null;
    }
  } // OnDisable()

  public void OnInitCreateDistortedImageFromVideoFrame() {
    //CoroutineHandle_ProcessVideo = StartCoroutine(Coroutine_ProcessVideo());
  }

  IEnumerator Coroutine_ProcessVideo() {
    // Wait for (!videoPlayer.isPrepared)
    yield return WaitUntilVideoPrepared;

    VideoPlayer.sendFrameReadyEvents = true;
    audioSampleProvider.enableSampleFramesAvailableEvents = true;
    // VideoPlayer.Play();    

    var distortedToTex2D
      = new Texture2D((int)VideoPlayer.width, (int)VideoPlayer.height, TextureFormat.RGBA32, true);

    using (var videoEncoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr)) {
      // 1. While CurrentFrameCounter is lower than VideoPlayer.framecount(Total frame Count).
      while (CurrentFrameCounter < (int)VideoPlayer.frameCount) {
        //  2. Wait until the next frame is ready (the next frame is extracted from the video).
        while (!bNextFrameReady) {
          yield return null;
        }

        while (!bNextAudioSamplesReady) {
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

        bCurrentSoundEncoded = videoEncoder.AddSamples(ExtractedAudioBuf);
        if (!bCurrentSoundEncoded) {
          Debug.LogError("<color=red>Failed to VideoEncoder.AddSample()</color>", this);
        }

        Debug.Log($"<color=green>{CurrentFrameCounter} frames are encoded.</color>");

        //.5. Encoding 1 frame is finished!
        // Resume the OnVideoFrameReceived(). (Extracting one frame of the video)
        bNextFrameReady = false;
        // Resume the OnAudioFrameReceived(). (Extracting one audio samples of the current frame of the video)

        bNextAudioSamplesReady = false;

        VideoPlayer.sendFrameReadyEvents = true;
        audioSampleProvider.enableSampleFramesAvailableEvents = true;

        VideoPlayer.Play();
        //AudioSource.Play();

        // 6. Wait a frame
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
  }                                 // Coroutine_ProcessVideo()

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
    distortedResult.ReadPixels(new Rect(0, 0, ConvergedRenderTexForNewImage.width, ConvergedRenderTexForNewImage.height), 0, 0);
    distortedResult.Apply();

    // 4. Restore the previous RenderTexture at the last frame.
    RenderTexture.active = prevRT;
  } // Coroutine ProcessExtractedTexToPredistortedTex(Texture2D)

  void OnVideoPlayerPrepared(VideoPlayer video) {
    // in order to receive the audio samples during playback to initialized AudioSampleProvider.
    audioSampleProvider = video.GetAudioSampleProvider(0);
    // bind the event to invoke explicity when the video got prepared to process the audio samples!
    audioSampleProvider.sampleFramesAvailable += OnAudioFrameReceived;
    // turn on the receiver!
    audioSampleProvider.enableSampleFramesAvailableEvents = true;
    // Then the free sample count falls below this threshold, AudioSampleProvider.sampleFramesAvailable event
    // and the associated native is emitted.
    //
    // audioSampleProvider.maxSampleFrameCount => The maximum number of sample frames that can be accumulated inside the internal buffer before an overflow event is emitted.
    //
    // NOTE : to seek the efficiency of the audio sample buffer transferring, AudioSampleProvider start to transfer as soon as after the sample count is over the maxSampleFrameCount / 4.
    audioSampleProvider.freeSampleFrameCountLowThreshold = audioSampleProvider.maxSampleFrameCount / 4;

    // Enables the frameReady events, it will be invoked when a frame is ready to be drawn.
    // TEST : bind OnVideoFrameReceived after the binding of OnAudioFrameReceived.
    // video.frameReady += OnVideoFrameReceived;
    // video.sendFrameReadyEvents = true;

    video.Play();
  }

  /// <summary>
  /// Callback that is called when the video frame of the current one frame is available to be transferred.
  /// </summary>
  /// <param name="source"></param>
  /// <param name="frameIdx"></param>
  void OnVideoFrameReceived(VideoPlayer source,
                            long frameIdx) {
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
    VideoPlayer.sendFrameReadyEvents = false;
    bNextFrameReady = true;

    ExtractedTex = sourceFrameTex;
    CurrentFrameCounter = (int)frameIdx;
    Debug.Log($"Current Video Frame Count : {CurrentFrameCounter} / {source.frameCount}", this);
  } // OnVideoFrameReceived()

  /// <summary>
  /// Callback that is called when the audio samples of the current one frame are available to be transferred.
  /// </summary>
  /// <param name="provider">Provider emitting the event.</param>
  /// <param name="sampleFramesCount">How many sample frames are available, or were dropped, depending on the event.</param>
  void OnAudioFrameReceived(AudioSampleProvider provider,
                            uint sampleFramesCount) {
    // TODO : Check whether there is the counter of the current frame for audio samples. (06-30-2020)

    using (var audioBuf = new NativeArray<float>((int)sampleFramesCount * provider.channelCount, Allocator.Temp)) {
      // UnityEngine.Experimental.Audio.AudioSampleProvider.ConsumeSampleFrames(
      //  NativeArray<float> sampleFrames) -> uint
      //
      //  param (sampleFrames) -> a buf where the consumed smaples wil lbe transferred.
      //  return (uint) -> How many samples were written into the buffer passed in.
      //  
      //  If AudioSampleProvider.enableSilencePadding = true, then the buf passed in as a parameter
      //  will be completely filled with and padded with silence if there are less audio sample of the current frame is available.
      //  Otherwise, the extra sample frame in the buf will be left intact!
      uint totalProvidedSamplesCnt = provider.ConsumeSampleFrames(audioBuf);
      Debug.Log($"SetupSoftwareAudioOutput.Available got {totalProvidedSamplesCnt} sample counts in total!", this);

      // TODO : It needs to advance after the encoding process of the current frame with the video frame extraction!
      // If the size of both buffers is different!
      if (ExtractedAudioBuf.Length != audioBuf.Length) {
        // reallocate the NativeArr.
        Profiler.BeginSample("Start Copying AudioBuffer(NativeArray<float>)");
        ExtractedAudioBuf = new NativeArray<float>(audioBuf, Allocator.Temp);
        Profiler.EndSample();
        Debug.Log($"ExtractedAudioBuf is filled again <length : {ExtractedAudioBuf.Length}>");
      }
      else {
        ExtractedAudioBuf.CopyFrom(audioBuf);
      }

      audioSampleProvider.enableSampleFramesAvailableEvents = false;
      bNextAudioSamplesReady = true;
    }
  }

  #region UNUSED

  /// <summary>
  /// Save the extracted image into the real file (jpg).
  /// </summary>
  /// <param name="extractedImg"></param>
  void SaveImageToJPG(Texture2D extractedImg,
                      string dirName,
                      string fileName) {
    if (extractedImg == null) return;

    byte[] savedImg = extractedImg.EncodeToJPG();
    File.WriteAllBytes($"{Application.dataPath}/Resources/Video2Img/{ExtractedImgFolderDirName}/{ExtractedImgFileName}_frame_{CurrentFrameCounter}.jpg",
                       savedImg);
  }

  void SaveTexture2dsToJPG(Texture2D[] tex2dArr,
                           string dirName,
                           string fileName) {
    if (tex2dArr.Length == 0) return;

    Texture2D fwdTex2d = new Texture2D(tex2dArr[0].width, tex2dArr[0].height);
    for (int i = 0; i < tex2dArr.Length; ++i) {
      SaveImageToJPG(fwdTex2d, dirName, fileName);
    }
  }

  void SaveRenderTexturesToJPG(RenderTexture[] rtArr,
                               string dirName,
                               string fileName) {
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