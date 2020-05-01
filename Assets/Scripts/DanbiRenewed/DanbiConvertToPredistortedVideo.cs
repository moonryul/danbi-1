using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor.Media;
using System.IO;



public class ConvertToPredistortedVideo : MonoBehaviour
{
    public VideoClip videoToPlay;

    private VideoPlayer videoPlayer;
    private VideoSource videoSource;
    private Renderer rend;
    private Texture tex;
    private AudioSource audioSource;

    public List<Texture2D> texList = new List<Texture2D>();


    void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(playVideo());

        Application.Quit();

    }

    IEnumerator playVideo()
    {
        Debug.Log(Application.dataPath);
        rend = GetComponent<Renderer>();

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        audioSource = gameObject.AddComponent<AudioSource>();

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
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        //Assign the Texture from Video to Material texture
        tex = videoPlayer.texture;
        rend.material.mainTexture = tex;

        videoPlayer.sendFrameReadyEvents = true;

        videoPlayer.frameReady += OnNewFrame;

        videoPlayer.Play();

        audioSource.Play();

        Debug.Log("Playing Video");

        while (texList.Count != (int)videoPlayer.frameCount)
        {
            yield return null;
        }
        Debug.Log("Done Playing Video");

        ///////////////////////////////////////////
        ////
        ///

        // 왜곡이미지 처리///

        ///////////////////////////////////////////

        VideoTrackAttributes videoAttr = new VideoTrackAttributes
        {
            frameRate = new MediaRational((int)videoPlayer.frameRate),
            width = videoPlayer.width,
            height = videoPlayer.height,
        };

        AudioTrackAttributes audioAttr = new AudioTrackAttributes
        {
            sampleRate = new MediaRational(48000),
            channelCount = 2,
            language = "fr"
        };


        int sampleFramesPerVideoFrame = audioAttr.channelCount *
            audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;

        // 동영상 생성 경로
        string encodedFilePath = Path.Combine(Application.dataPath + "/Resources/ConvertVideo", "my_movie.mp4");

        MediaEncoder encoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr);

        for (int i = 0; i < texList.Count; ++i)
        {
            Debug.Log("Encoding tex num " + (i + 1) + " / " + texList.Count);
            encoder.AddFrame(texList[i]);
            yield return null;
        }
        encoder.Dispose();

        Debug.Log("Convert To Video Complete");
    }


   

    void OnNewFrame(VideoPlayer source, long frameIdx)
    {
        RenderTexture renderTexture = source.texture as RenderTexture;
        Texture2D videoFrame = new Texture2D(renderTexture.width, renderTexture.height);
       
        if (videoFrame.width != renderTexture.width || videoFrame.height != renderTexture.height)
        {
            videoFrame.Resize(renderTexture.width, renderTexture.height);
        }
        RenderTexture.active = renderTexture;
        videoFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        videoFrame.Apply();
        RenderTexture.active = null;

        texList.Add(videoFrame);
        Debug.Log("Save Texture To List : " + texList.Count + " / " + videoPlayer.frameCount);


        //targetColor = CalculateAverageColorFromTexture(videoFrame);
        //lSource.color = targetColor;
    }


}