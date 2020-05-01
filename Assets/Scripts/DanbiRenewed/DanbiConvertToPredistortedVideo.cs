using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor.Media;
using System.IO;



[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class ConvertToPredistortedVideo : MonoBehaviour {
  public VideoClip videoToPlay;

  private VideoPlayer videoPlayer;
  private VideoSource videoSource;
  /// <summary>
  /// 
  /// </summary>
  private Renderer TargetPanoramaRenderer;
  private Texture TextureOfCurrentFrame;
  private AudioSource audioSource;

  public List<Texture2D> ExtractedTexturesList = new List<Texture2D>();


  void Start() {
    Application.runInBackground = true;

    videoPlayer = GetComponent<VideoPlayer>();
    audioSource = GetComponent<AudioSource>();

    StartCoroutine(Coroutine_ProcessVideo());

    Application.Quit();

  }

  public void ProcessVideo() {
    Application.runInBackground = true;

    videoPlayer = GetComponent<VideoPlayer>();
    audioSource = GetComponent<AudioSource>();

    StartCoroutine(Coroutine_ProcessVideo());

    Application.Quit();
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
    videoPlayer.playOnAwake = false;
    audioSource.playOnAwake = false;

    videoPlayer.source = VideoSource.VideoClip;
    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
    videoPlayer.EnableAudioTrack(0, true);
    videoPlayer.SetTargetAudioSource(0, audioSource);

    //Set video To Play then prepare Audio to prevent Buffering
    videoPlayer.clip = videoToPlay;
    videoPlayer.Prepare();

    //Wait until video is prepared
    while (!videoPlayer.isPrepared) {
      yield return null;
    }

    //Assign the Texture from Video to Material texture
    //The texture is used to send the video content to the desired target. 
    // When the VideoPlayer.renderMode is set [[Video.VideoTarget.APIOnly],
    // the content is still accessible from scripts using this property.    
    TextureOfCurrentFrame = videoPlayer.texture;

    //Rend = GetComponent<Renderer>();
    //TargetPanoramaRenderer.material.mainTexture = TextureOfCurrentFrame;
    
    videoPlayer.sendFrameReadyEvents = true;
    videoPlayer.frameReady += OnReceivedNewFrame;
    videoPlayer.Play();
    audioSource.Play();

    Debug.Log("Playing Video");

    while (ExtractedTexturesList.Count != (int)videoPlayer.frameCount) {
      yield return null;
    }
    Debug.Log("Done Playing Video");

    ///////////////////////////////////////////
    ////
    ///
    (TargetPanoramaRenderer as MeshRenderer).sharedMaterial.SetTexture("_MainTex", TextureOfCurrentFrame);    
    // 왜곡이미지 처리///

    ///////////////////////////////////////////

    VideoTrackAttributes videoAttr = new VideoTrackAttributes {
      frameRate = new MediaRational((int)videoPlayer.frameRate),
      width = videoPlayer.width,
      height = videoPlayer.height,
    };

    AudioTrackAttributes audioAttr = new AudioTrackAttributes {
      sampleRate = new MediaRational(48000),
      channelCount = 2,
      language = "fr"
    };


    int sampleFramesPerVideoFrame = audioAttr.channelCount *
        audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;

    // 동영상 생성 경로
    string encodedFilePath = Path.Combine(Application.dataPath + "/Resources/ConvertVideo", "my_movie.mp4");

    MediaEncoder encoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr);

    for (int i = 0; i < ExtractedTexturesList.Count; ++i) {
      Debug.Log("Total encoding texture num " + (i + 1) + " / " + ExtractedTexturesList.Count);
      encoder.AddFrame(ExtractedTexturesList[i]);
      yield return null;
    }
    encoder.Dispose();

    Debug.Log("Convert To Video Complete");
  }

  void OnReceivedNewFrame(VideoPlayer source, long frameIdx) {
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
    Debug.Log("Save Texture To List : " + ExtractedTexturesList.Count + " / " + videoPlayer.frameCount);

    //targetColor = CalculateAverageColorFromTexture(videoFrame);
    //lSource.color = targetColor;
  }

  void ConvertSavedImagesToPredistortedImages(Texture targetTex) {

  }


  public void ApplyVideoFrameImageToPanoramaTexture(ref Texture2D targetPanoramaTex) {

  }
};