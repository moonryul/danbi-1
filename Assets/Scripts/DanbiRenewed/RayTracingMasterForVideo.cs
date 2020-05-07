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

  public List<Texture2D> ExtractedTexturesList = new List<Texture2D>();
  public List<RenderTexture> DistortedRenderTexturesList = new List<RenderTexture>();

  WaitUntil WaitUntilVideoPrepared;
  WaitUntil WaitUntilVideoFinished;
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
    Application.runInBackground = true;
    VideoPlayer = GetComponent<VideoPlayer>();
    AudioSource = GetComponent<AudioSource>();
    //CurrentScreenResolutions.x = (int)VideoPlayer.width;
    //CurrentScreenResolutions.y = (int)VideoPlayer.height;

    // MJ:Remove "Yield" from the following two statements.
    WaitUntilVideoPrepared = new WaitUntil(()
      => VideoPlayer.isPrepared == true);

    WaitUntilVideoFinished = new WaitUntil(()
      => VideoPlayer.isPlaying == false);

    WaitUntilVideoBlockExtracted = new WaitUntil(()
      => ExtractedTexturesList.Count != 0 &&
         ExtractedTexturesList.Count % VideoFrameNumbersForOneTimeConversion != 0);

    WaitUntilRenderFinished = new WaitUntil(()
      => bStopRender == true);

    SimulatorMode = Danbi.EDanbiSimulatorMode.PREPARE;

    videoAttr = new VideoTrackAttributes {
      frameRate = new MediaRational((int)VideoPlayer.frameRate),
      width = VideoPlayer.width,
      height = VideoPlayer.height,
    };

    audioAttr = new AudioTrackAttributes {
      sampleRate = new MediaRational(48000),
      channelCount = 2,
      language = ""
    };

    sampleFramesPerVideoFrame = audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
    encodedVideoFilePath = Path.Combine($"{Application.dataPath}/{EncodedVideoFolderDirName}/{EncodedVideoFileName}.mp4");

    base.Start();

    OnInitCreateDistortedImageFromVideoFrame();
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
    // Disable Play on Awake for both Video and Audio
    VideoPlayer.playOnAwake = false;
    AudioSource.playOnAwake = false;

    VideoPlayer.source = VideoSource.VideoClip;
    VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    VideoPlayer.EnableAudioTrack(0, true);
    VideoPlayer.SetTargetAudioSource(0, AudioSource);

    // Enables the frameReady events, it will be invoked when a frame is ready to be drawn.
    VideoPlayer.sendFrameReadyEvents = true;
    // event to invoke explicitly when a new frame is ready.
    VideoPlayer.frameReady += OnReceivedNewFrame;

    // Set video To Play then prepare Audio to prevent Buffering
    VideoPlayer.clip = videoToPlay;
    VideoPlayer.Prepare();

    // Wait until video is prepared
    yield return WaitUntilVideoPrepared;

    //while (!VideoPlayer.isPrepared) {
    //  yield return null;
    //}

    // Process the distorted image with one batch of Video Frame Numbers

    yield return WaitUntilVideoBlockExtracted;

    VideoPlayer.Pause();
    AudioSource.Pause();
    VideoPlayer.sendFrameReadyEvents = false;

    // Distort Img
    ConvertSavedImagesToPredistortedImages(ExtractedTexturesList);

    VideoPlayer.Play();
    AudioSource.Play();
    VideoPlayer.sendFrameReadyEvents = true;

    if (CurrentFrameCounter == (int)VideoPlayer.frameCount) {
      // Finish the current coroutine.
      yield break;
    }
    // TODO: iterations till all the frame is processed.
    yield break;
  }

  //IEnumerator SafeNewFrameProgress(VideoPlayer vp) {
  //  vp.sendFrameReadyEvents = true;
  //  vp.frameReady += OnReceivedNewFrame;
  //}

  // TODO: Do we need to Use Conversion from RenderTexture to Texture2D
  void OnReceivedNewFrame(VideoPlayer source, long frameIdx) {
    //RenderTexture renderTexture = source.texture as RenderTexture;
    //Texture2D videoFrame = new Texture2D(renderTexture.width, renderTexture.height);

    // 1. This codes should theoretically work since Texture2D class is derived of Texture class.
    //Texture2D videoFrame = source.texture as Texture2D;

    // 2. 
    //Texture2D videoFrame = Texture2D.CreateExternalTexture(width: source.texture.width,
    //                                                       height: source.texture.height,
    //                                                       format: TextureFormat.RGB24,
    //                                                       mipChain: false,
    //                                                       linear: true,
    //                                                       nativeTex: source.texture.GetNativeTexturePtr());

    //if (videoFrame.width != CurrentScreenResolutions.x || videoFrame.height != CurrentScreenResolutions.y) {
    //  videoFrame.Resize(CurrentScreenResolutions.x, CurrentScreenResolutions.y);
    //}

    ////RenderTexture.active = renderTexture;
    ////videoFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    ////videoFrame.Apply();
    ////RenderTexture.active = null;

    //ExtractedTexturesList.Add(videoFrame);
    //Debug.Log("Save Texture To List : " + ExtractedTexturesList.Count + " / " + VideoPlayer.frameCount);

    //targetColor = CalculateAverageColorFromTexture(videoFrame);
    //lSource.color = targetColor;
    //     

    ++CurrentForceGCCollect_Count;
    if (CurrentForceGCCollect_Count > MaxForceGCCollect_Count) {
      CurrentForceGCCollect_Count = 0;
      System.GC.Collect(0, System.GCCollectionMode.Forced, true, true);
      Debug.Log("System is in GC");
    }

    // Request from MJ: save RenderTexture by currentRT = RenderTexture.active
    RenderTexture prevRT = RenderTexture.active;
    RenderTexture currentRT = source.texture as RenderTexture;
    RenderTexture.active = currentRT;
    // Request from MJ: Create videoFrame object just once in Start() method
    Texture2D tex2d = new Texture2D(currentRT.width, currentRT.height);

    // 1. Perform on GPU-side
    Graphics.CopyTexture(RenderTexture.active, tex2d);

    // 2. Perform on CPU-side
    tex2d.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
    tex2d.Apply();

    //
    // NOTE: Since Texture2D.ReadPixels() is performed on CPU-side
    // it can be a great performant between choosing them.
    // 

    // Request from MJL restore RenderTexture by  RenderTexture.active = currentRT
    RenderTexture.active = prevRT;

    //byte[] savedImg = tex2d.EncodeToJPG();

    //File.WriteAllBytes($"{Application.dataPath}/Resources/ImgFromVideo/{NewFolderName}/{NewFileName}_frame_{CurrentFrameCounter}.jpg",
    //                   savedImg);
    //savedImg = null;

    ExtractedTexturesList.Add(tex2d);
    //FwdTex = videoFrame;

    ++CurrentFrameCounter;

    //DestroyImmediate(videoFrame);
    //videoFrame = null;

    Debug.Log($"Current Video Frame Count : {CurrentFrameCounter} / {source.frameCount}");
  }

  void SaveExtractedImagesToJPEG(List<Texture2D> targetTex) {

  }

  IEnumerator ConvertSavedImagesToPredistortedImages(List<Texture2D> extractedTexturesList) {
    for (int i = 0; i < VideoFrameNumbersForOneTimeConversion; ++i) {
      // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
      OnInitCreateDistortedImage(extractedTexturesList[i]);
      // Wait until the predistorted image is created but yield immediately when the image isn't ready.
      yield return WaitUntilRenderFinished;
      //
      DistortedRenderTexturesList.Add(ConvergedRenderTexForNewImage);
    }
  }

  IEnumerator ConvertRenderTexturesToPredistortedImages(List<RenderTexture> renderTextures) {
    for (int i = 0; i < VideoFrameNumbersForOneTimeConversion; ++i) {
      // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
      OnInitCreateDistortedImage2(renderTextures[i]);
      // Wait until the predistorted image is created but yield immediately when the image isn't ready.
      yield return WaitUntilRenderFinished;
      //
      DistortedRenderTexturesList.Add(ConvergedRenderTexForNewImage);
    }
  }


  public void ApplyVideoFrameImageToPanoramaTexture(Texture2D targetPanoramaTex) {

  }

  public void EncodeVideoFromPrewarpedImages(List<RenderTexture> prewarpedImages) {
    // Compose the video again to encode from the Images list.
    MediaEncoder encoder = new MediaEncoder(encodedVideoFilePath, videoAttr, audioAttr);
    Texture2D convertedToTex2d = new Texture2D(prewarpedImages[0].width, prewarpedImages[0].height);

    for (int i = 0; i < prewarpedImages.Count; ++i) {
      Debug.Log($"Current encoding idx {i} of {ExtractedTexturesList.Count}");
      Graphics.CopyTexture(prewarpedImages[i], convertedToTex2d);
      encoder.AddFrame(convertedToTex2d);
    }
    encoder.Dispose();
    DestroyImmediate(convertedToTex2d);
  }
}
