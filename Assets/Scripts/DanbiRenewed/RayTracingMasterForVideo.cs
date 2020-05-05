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
  #endregion

  #region Internal Variables
  public Texture2D TargetPanoramaTexFromVideoFrame { get; set; }

  VideoPlayer VideoPlayer;
  VideoSource VideoSource;

  Renderer TargetPanoramaRenderer;
  Texture TextureOfCurrentFrame;
  AudioSource AudioSource;

  public List<Texture2D> ExtractedTexturesList = new List<Texture2D>();

  WaitUntil WaitUntilVideoHasPrepared;
  WaitUntil WaitUntilVideoHasFinished;
  WaitUntil WaitUntilRenderFinished;
  Coroutine CoroutineHandle_ProcessVideo;
  #endregion

  protected override void Start() {
    Application.runInBackground = true;
    VideoPlayer = GetComponent<VideoPlayer>();
    AudioSource = GetComponent<AudioSource>();
    //CurrentScreenResolutions.x = (int)VideoPlayer.width;
    //CurrentScreenResolutions.y = (int)VideoPlayer.height;

    // MJ:Remove "Yield" from the following two statements.
    WaitUntilVideoHasPrepared = new WaitUntil(() => VideoPlayer.isPrepared == false);
    WaitUntilVideoHasFinished = new WaitUntil(() => VideoPlayer.isPlaying == false);
    WaitUntilRenderFinished = new WaitUntil(() => bStopRender == true);
    SimulatorMode = Danbi.EDanbiSimulatorMode.PREPARE;
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
    //Disable Play on Awake for both Video and Audio
    VideoPlayer.playOnAwake = false;
    AudioSource.playOnAwake = false;

    VideoPlayer.source = VideoSource.VideoClip;
    VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    VideoPlayer.EnableAudioTrack(0, true);
    VideoPlayer.SetTargetAudioSource(0, AudioSource);

    //Set video To Play then prepare Audio to prevent Buffering
    VideoPlayer.clip = videoToPlay;
    VideoPlayer.Prepare();

    // Wait until video is prepared
    yield return WaitUntilVideoHasPrepared;
    WaitUntilVideoHasPrepared = null;

    // Assign the Texture from Video to Material texture
    // The texture is used to send the video content to the desired target. 
    // When the VideoPlayer.renderMode is set [[Video.VideoTarget.APIOnly],
    // the content is still accessible from scripts using this property.    
    TextureOfCurrentFrame = VideoPlayer.texture;

    // Rend = GetComponent<Renderer>();
    // TargetPanoramaRenderer.material.mainTexture = TextureOfCurrentFrame;

    VideoPlayer.sendFrameReadyEvents = true;
    VideoPlayer.frameReady += OnReceivedNewFrame;

    VideoPlayer.Play();
    AudioSource.Play();

    Debug.Log("Playing Video");

    //while (ExtractedTexturesList.Count != (int)VideoPlayer.frameCount) {
    //  yield return null;
    //}

    // Check if the video is finished but yield immediately if it's not finished yet.
    //while (VideoPlayer.isPlaying) {
    //  Debug.Log($"Current Playback Time of Video : { Mathf.FloorToInt((float)VideoPlayer.time).ToString()} (s)");
    //  yield return null;
    //}

    // Check until video has finished.
    yield return WaitUntilVideoHasFinished;

    // Set this yieldInstruction as null since it's no longer used.
    WaitUntilVideoHasFinished = null;

    // Stop the video and the audio.
    VideoPlayer.Stop();
    AudioSource.Stop();

    // Now the video playing is ended!
    Debug.Log("Done Playing of the Video Convert the saved frames to the predistorted video");

    SimulatorMode = Danbi.EDanbiSimulatorMode.CAPTURE;

    for (int i = 0; i < ExtractedTexturesList.Count; ++i) {
      
      // 1. Make sure you secure the previous active render texture
      //RenderTexture currentRT = RenderTexture.active;      

      // 2. Create Distorted Image (Refreshes RenderTextures and Pass parameters)
      OnInitCreateDistortedImage(ExtractedTexturesList[i]);

      // 3. Wait until the predistorted image is created but yield immediately when the image isn't ready.      
      yield return WaitUntilRenderFinished; //  Request from MJ: rename YieldUntilRenderFinished

      //Request from MJ: What are you doing here? The rendered predistorted image is 
      // contained in ConvergedRenderTextureForNewImage.

      //MJ ??
      RenderTexture rt = new RenderTexture(ExtractedTexturesList[i].width, ExtractedTexturesList[i].height, 32);
      Graphics.Blit(ExtractedTexturesList[i], rt);      //MJ ??
      // MJ: what do you do here? ExTractedTexturesList[i] is the frame of the original 
      // video. What you need to do here is to add the distortedImage to the new video file.

      // now the predistorted image is ready!
      //RenderTexture predistortedImage = ConvergedRenderTexForNewImage;
      //Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
      // //RenderTexture.active = predistortedImage;
      //tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
      //tex.Apply();
      RenderTexture.active = rt;       // MJ ??

      //VideoTrackAttributes videoAttr = new VideoTrackAttributes {
      //  frameRate = new MediaRational((int)VideoPlayer.frameRate),
      //  width = VideoPlayer.width,
      //  height = VideoPlayer.height,
      //};

      //AudioTrackAttributes audioAttr = new AudioTrackAttributes {
      //  sampleRate = new MediaRational(48000),
      //  channelCount = 2,
      //  language = ""
      //};

      //int sampleFramesPerVideoFrame = audioAttr.channelCount *
      //    audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;

      //// 1. Compose the video again from the Images list.
      //// 동영상 생성 경로
      //// TODO: need to expose to decide which name will be used for this video.
      //string encodedFilePath = Path.Combine(Application.dataPath + "/Resources/ConvertVideo", "my_movie.mp4");
      //MediaEncoder encoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr);

      //for (int j = 0; i < ExtractedTexturesList.Count; ++j) {
      //  Debug.Log("Current encoding texture index " + (j + 1) + " of " + ExtractedTexturesList.Count);
      //  encoder.AddFrame(tex);
      //  yield return null;
      //}
      //encoder.Dispose();
      // 
      
      //yield return null;
      rt.Release();  //MJ ???
      rt = null;     // MJ ???
      //RenderTexture.active = currentRT;
      //currentRT.Release();
      //currentRT = null;
    }
    Debug.Log("Conversion To Video Complete");

    yield break;
  }

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
    RenderTexture renderTexture = source.texture as RenderTexture;

    // Request from MJ: Create videoFrame object just once in Start() method
    Texture2D videoFrame = new Texture2D(renderTexture.width, renderTexture.height);

    if (videoFrame.width != renderTexture.width || videoFrame.height != renderTexture.height) {
      videoFrame.Resize(renderTexture.width, renderTexture.height);
    }

    // Request from MJ: save RenderTexture by currentRT = RenderTexture.active
    RenderTexture.active = renderTexture;
    videoFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    videoFrame.Apply();

    // Request from MJL restore RenderTexture by  RenderTexture.active = currentRT
        RenderTexture.active = null;

    ExtractedTexturesList.Add(videoFrame);
    Debug.Log("Save Texture To List : " + ExtractedTexturesList.Count + " / " + VideoPlayer.frameCount);

  }

  void ConvertSavedImagesToPredistortedImages(Texture targetTex) {

  }


  public void ApplyVideoFrameImageToPanoramaTexture(ref Texture2D targetPanoramaTex) {

  }
}
