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
  AudioSource audioSource;

  public List<Texture2D> ExtractedTexturesList = new List<Texture2D>();
  #endregion

  void Start() {
    Application.runInBackground = true;
    VideoPlayer = GetComponent<VideoPlayer>();
    audioSource = GetComponent<AudioSource>();
    //CurrentScreenResolutions.x = (int)VideoPlayer.width;
    //CurrentScreenResolutions.y = (int)VideoPlayer.height;

    base.Start();

    OnInitCreateDistortedImageFromVideoFrame();
  }

  public void OnInitCreateDistortedImageFromVideoFrame() {
    StartCoroutine(Coroutine_ProcessVideo());
    //Application.Quit();
  }

  IEnumerator Coroutine_ProcessVideo() {
    Debug.Log(Application.dataPath);

    //if (ReferenceEquals(videoPlayer, null)) {
    //}
    //if (ReferenceEquals(audioSource, null)) {
    //}

    // Move out from Coroutine since it doesn't need to get component at runtime.
    // videoPlayer = gameObject.AddComponent<VideoPlayer>();
    // audioSource = gameObject.AddComponent<AudioSource>();

    //Disable Play on Awake for both Video and Audio
    VideoPlayer.playOnAwake = false;
    audioSource.playOnAwake = false;

    VideoPlayer.source = VideoSource.VideoClip;
    VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    VideoPlayer.EnableAudioTrack(0, true);
    VideoPlayer.SetTargetAudioSource(0, audioSource);

    //Set video To Play then prepare Audio to prevent Buffering
    VideoPlayer.clip = videoToPlay;
    VideoPlayer.Prepare();

    //Wait until video is prepared
    while (!VideoPlayer.isPrepared) {
      yield return null;
    }

    //Assign the Texture from Video to Material texture
    //The texture is used to send the video content to the desired target. 
    // When the VideoPlayer.renderMode is set [[Video.VideoTarget.APIOnly],
    // the content is still accessible from scripts using this property.    
    TextureOfCurrentFrame = VideoPlayer.texture;

    //Rend = GetComponent<Renderer>();
    //TargetPanoramaRenderer.material.mainTexture = TextureOfCurrentFrame;

    VideoPlayer.sendFrameReadyEvents = true;
    VideoPlayer.frameReady += OnReceivedNewFrame;
    VideoPlayer.Play();
    audioSource.Play();

    Debug.Log("Playing Video");

    //while (ExtractedTexturesList.Count != (int)VideoPlayer.frameCount) {
    //  yield return null;
    //}

    // Check if the video is finished but yield immediately if it's not finished yet.
    while (VideoPlayer.isPlaying) {
      Debug.Log($"Current Playback Time of Video : { Mathf.FloorToInt((float)VideoPlayer.time).ToString()} (s)");
      yield return null;
    }

    // Now the video playing is ended!
    Debug.Log("Done Playing of the Video Convert the saved frames to the predistorted video");

    for (int i = 0; i < ExtractedTexturesList.Count; ++i) {
      //////////////////////////////////////
      // Process the Pre-distorted Image  //
      //////////////////////////////////////
      ///      ////////////////////////////////////// 
      RenderTexture predistortedImage;

      OnInitCreateDistortedImage(ExtractedTexturesList[i]);

      // Wait until the predistorted image is created but yield immediately when the image isn't ready.
      if (!bStopRender) {
        yield return null;
      }

      // now the predistorted image is ready!
      predistortedImage = ConvergedRenderTexForNewImage;
      Texture2D tex = new Texture2D((int)VideoPlayer.width, (int)VideoPlayer.height, TextureFormat.RGB24, false);
      RenderTexture.active = predistortedImage;
      tex.ReadPixels(new Rect(0, 0, (int)VideoPlayer.width, (int)VideoPlayer.height), 0, 0);
      tex.Apply();
      RenderTexture.active = null;
      
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
    }
    Debug.Log("Convert To Video Complete");
  }

  // TODO: Do we need to Use Convertion from RenderTexture to Texture2D
  void OnReceivedNewFrame(VideoPlayer source, long frameIdx) {
    //RenderTexture renderTexture = source.texture as RenderTexture;
    //Texture2D videoFrame = new Texture2D(renderTexture.width, renderTexture.height);

    // 1. This codes should theorically work since Texture2D class is derived of Texture class.
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
    Texture2D videoFrame = new Texture2D(renderTexture.width, renderTexture.height);

    if (videoFrame.width != renderTexture.width || videoFrame.height != renderTexture.height) {
      videoFrame.Resize(renderTexture.width, renderTexture.height);
    }
    RenderTexture.active = renderTexture;
    videoFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    videoFrame.Apply();
    RenderTexture.active = null;

    ExtractedTexturesList.Add(videoFrame);
    Debug.Log("Save Texture To List : " + ExtractedTexturesList.Count + " / " + VideoPlayer.frameCount);

  }

  void ConvertSavedImagesToPredistortedImages(Texture targetTex) {

  }


  public void ApplyVideoFrameImageToPanoramaTexture(ref Texture2D targetPanoramaTex) {

  }
}
