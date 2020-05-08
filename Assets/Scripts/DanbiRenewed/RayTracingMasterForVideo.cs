using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor.Media;
using System.IO;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class RayTracingMasterForVideo : RayTracingMaster {

  #region Exposed Variables
  [SerializeField] VideoClip videoToPlay;

  [SerializeField, Space(20)] int MaxForceGCCollect_Count = 200;

  [SerializeField, Space(20)] string ExtractedImgFolderDirName;
  [SerializeField] string ExtractedImgFileName;

  [SerializeField, Space(20)] string DistortedImgFolderDirName;
  [SerializeField] string DistortedImgFileName;

  [SerializeField, Space(20)] string EncodedVideoFolderDirName;
  [SerializeField] string EncodedVideoFileName;

  [SerializeField, Space(20)] int CurrentFrameCounter;

  [SerializeField, Space(20)] int VideoFrameNumbersForOneTimeConversion = 100;
  #endregion

  #region Internal Variables
  public Texture2D TargetPanoramaTexFromVideoFrame { get; set; }

  VideoPlayer VideoPlayer;
  VideoSource VideoSource;

  //Renderer TargetPanoramaRenderer;
  //Texture TextureOfCurrentFrame;
  AudioSource AudioSource;

  public Texture2D[] ExtractedTexturesArr;
  public RenderTexture[] DistortedRenderTexturesArr;

  WaitUntil WaitUntilVideoPrepared;
  WaitUntil WaitUntilVideoBlockExtracted;
  WaitUntil WaitUntilRenderFinished;
  Coroutine CoroutineHandle_ProcessVideo;

  bool bSplitFromVideoToImgFinished = false;

  int CurrentForceGCCollect_Count = 0;

  VideoTrackAttributes videoAttr;
  AudioTrackAttributes audioAttr;
  int sampleFramesPerVideoFrame;
  string encodedVideoFilePath;  

  #endregion

  protected override void Start() {
    ExtractedTexturesArr = new Texture2D[VideoFrameNumbersForOneTimeConversion];
    DistortedRenderTexturesArr = new RenderTexture[VideoFrameNumbersForOneTimeConversion];

    Application.runInBackground = true;

    #region Bind yieldinstructions as lambdas expression.
    WaitUntilVideoPrepared = new WaitUntil(()
      => VideoPlayer.isPrepared);

    WaitUntilVideoBlockExtracted = new WaitUntil(()
      => CurrentFrameCounter != 0
      && CurrentFrameCounter % VideoFrameNumbersForOneTimeConversion == 0);

    WaitUntilRenderFinished = new WaitUntil(()
      => bStopDispatch);
    #endregion

    #region Prepare videos
    SimulatorMode = Danbi.EDanbiSimulatorMode.PREPARE;

    VideoPlayer = GetComponent<VideoPlayer>();
    AudioSource = GetComponent<AudioSource>();
    //CurrentScreenResolutions.x = (int)VideoPlayer.width;
    //CurrentScreenResolutions.y = (int)VideoPlayer.height;

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
    // event to invoke explicitly when a new frame is ready.
    VideoPlayer.frameReady += OnReceivedNewFrame;

    VideoPlayer.Prepare();

    videoAttr = new VideoTrackAttributes {
      frameRate = new MediaRational((int)VideoPlayer.frameRate),
      width = VideoPlayer.width,
      height = VideoPlayer.height,
    };

    audioAttr = new AudioTrackAttributes {
      sampleRate = new MediaRational(48000),
      channelCount = 2,
      language = "kr"
    };

    sampleFramesPerVideoFrame = audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
    encodedVideoFilePath = Path.Combine($"{Application.dataPath}/Resources/Video2Img/{EncodedVideoFolderDirName}/{EncodedVideoFileName}.mp4");    
    #endregion

    base.Start();

    OnInitCreateDistortedImageFromVideoFrame();
  }

  protected override void Update() {
    base.Update();

    //Debug.Log($"Video is {VideoPlayer.isPrepared}");
  }

  protected override void OnDisable() {
    StopCoroutine(CoroutineHandle_ProcessVideo);
    CoroutineHandle_ProcessVideo = null;

    base.OnDisable();
  }

  public void OnInitCreateDistortedImageFromVideoFrame() {
    CoroutineHandle_ProcessVideo = StartCoroutine(Coroutine_ProcessVideo());
  }

  IEnumerator Coroutine_ProcessVideo() {
    yield return WaitUntilVideoPrepared;

    // When CurrentFrameCounter has reached at the last frame, yield break.
    while (CurrentFrameCounter < (int)VideoPlayer.frameCount) {
      VideoPlayer.sendFrameReadyEvents = true;
      VideoPlayer.Play();
      AudioSource.Play();

      // Wait for the next frame for 1 frame is received.
      //yield return null;
      yield return WaitUntilVideoBlockExtracted;

      VideoPlayer.Pause();
      AudioSource.Pause();
      VideoPlayer.sendFrameReadyEvents = false;

      // Distort Img
      yield return StartCoroutine(ConvertSavedImagesToPredistortedImages(ExtractedTexturesArr, DistortedRenderTexturesArr));

      EncodeVideoFromPredistortedImages(DistortedRenderTexturesArr);
    }
    yield break;
  } // Coroutine_ProcessVideo()

  void OnReceivedNewFrame(VideoPlayer source, long frameIdx) {
    //++CurrentForceGCCollect_Count;
    //if (CurrentForceGCCollect_Count > MaxForceGCCollect_Count) {
    //  CurrentForceGCCollect_Count = 0;
    //  System.GC.Collect(0, System.GCCollectionMode.Forced, true, true);
    //  Debug.Log("System is in GC");
    //}

    // Secure the previous render texture.
    RenderTexture prevRT = RenderTexture.active;
    // Get the source texture (current frame).
    RenderTexture currentRT = source.texture as RenderTexture;
    // Set the current render texture as an active render texture.
    RenderTexture.active = currentRT;

    //if (CurrentFrameTexture2d == null) {
    //
    //}
    Texture2D CurrentFrameTexture2d = new Texture2D(currentRT.width, currentRT.height, TextureFormat.RGB24, false);

    // 1. Perform on GPU-side
    //Graphics.CopyTexture(RenderTexture.active, CurrentFrameTexture2d);

    // 2. Perform on CPU-side
    CurrentFrameTexture2d.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
    CurrentFrameTexture2d.Apply();

    //
    // NOTE: Since Texture2D.ReadPixels() is performed on CPU-side
    // it can be a great performant between choosing them.
    // 

    // restore the active render texture by the previous render texture.
    RenderTexture.active = prevRT;

    //SaveExtractedImagesToJPG(CurrentFrameTexture2d);

    // Add the current frame texture2d into the extracted textures list.
    ExtractedTexturesArr[CurrentFrameCounter % VideoFrameNumbersForOneTimeConversion]
      = CurrentFrameTexture2d;

    Debug.Log($"Current Video Frame Count : {++CurrentFrameCounter} / {source.frameCount}");

    DestroyImmediate(CurrentFrameTexture2d);
    CurrentFrameTexture2d = null;
  } // OnReceivedNewFrame

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
  IEnumerator ConvertSavedImagesToPredistortedImages(Texture2D[] extractedTexturesArr,
                                                     RenderTexture[] distortedRenderTexturesArr) {
    Debug.Log($"Image is being converted to the distorted image");
    for (int i = 0; i < VideoFrameNumbersForOneTimeConversion /** multiplier*/; ++i) {
      // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
      OnInitCreateDistortedImage(extractedTexturesArr[i]);
      // Wait until the predistorted image is created but yield immediately when the image isn't ready.
      // The number of sampling for rendering is 30
      // If bStopDispatch is false -> yield return immediately and then OnRenderImage() will be called!
      // otherwise go to the next line.
      yield return WaitUntilRenderFinished;

      // the predistorted image (ConvergedRenderTexForNewImage) is ready.
      distortedRenderTexturesArr[i] = ConvergedRenderTexForNewImage;
    }
    yield break;
    //SaveRenderTexturesToJPG(DistortedRenderTexturesList.ToArray(), DistortedImgFolderDirName, DistortedImgFileName);
  }

  //IEnumerator ConvertRenderTexturesToPredistortedImages(RenderTexture[] renderTexturesArr,
  //                                                      List<RenderTexture> distortedRenderTexturesList) {
  //  for (int i = 0; i < VideoFrameNumbersForOneTimeConversion; ++i) {
  //    // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
  //    // DanbiSimulatorMode (PREPARE -> CAPTURE).
  //    OnInitCreateDistortedImage2(renderTexturesArr[i]);
  //    // Wait until the predistorted image is created but yield immediately when the image isn't ready.
  //    yield return WaitUntilRenderFinished; // move down if (!bStopDispatch)
  //    //
  //    distortedRenderTexturesList.Add(ConvergedRenderTexForNewImage);
  //    //RenderTexture.active = DistortedRenderTexturesList[i];
  //  }
  //  //SaveRenderTexturesToJPG(distortedRenderTexturesList.ToArray(), DistortedImgFolderDirName, DistortedImgFileName);
  //}


  public void ApplyVideoFrameImageToPanoramaTexture(Texture2D targetPanoramaTex) {

  }

  public void EncodeVideoFromPredistortedImages(RenderTexture[] predistortedImages) {
    // Compose the video again to encode from the Images list.
    //MediaEncoder encoder = new MediaEncoder(encodedVideoFilePath, videoAttr, audioAttr);
    Texture2D convertedToTex2d = new Texture2D(predistortedImages[0].width, predistortedImages[0].height);
    videoAttr.width = (uint)convertedToTex2d.width;
    videoAttr.height = (uint)convertedToTex2d.height;

    using (var encoder = new MediaEncoder(encodedVideoFilePath, videoAttr, audioAttr))
    using (var audioBuf = new Unity.Collections.NativeArray<float>(2, Unity.Collections.Allocator.Temp)) {
      for (int i = 0; i < predistortedImages.Length; ++i) {
        Debug.Log($"Current encoding idx {i} of {ExtractedTexturesArr.Length}");
        //Graphics.CopyTexture(prewarpedImages[i], convertedToTex2d);

        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = predistortedImages[i];
        convertedToTex2d.ReadPixels(new Rect(0, 0, predistortedImages[i].width, predistortedImages[i].height), 0, 0);
        convertedToTex2d.Apply();
        RenderTexture.active = prevRT;

        encoder.AddFrame(convertedToTex2d);
        encoder.AddSamples(audioBuf);
      }
    }
    DestroyImmediate(convertedToTex2d);
  }
}
