using System.Collections;
using System.IO;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
public class RayTracingMasterForVideo : RayTracingMaster
{
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
    RenderTexture[] CopiedResultRTArr;
    public Texture2D[] CopiedResultTexArr;

    WaitUntil WaitUntilVideoPrepared;
    WaitUntil WaitUntilVideoBlockExtracted;
    WaitUntil WaitUntilRenderFinished;
    WaitUntil WaitUntilFrameIsAdded;
    Coroutine CoroutineHandle_ProcessVideo;

    bool bSplitFromVideoToImgFinished = false;

    bool bFrameAdded = false;
    //int CurrentForceGCCollect_Count = 0;

    // VideoTrackAttributes videoAttr;
    // AudioTrackAttributes audioAttr;
    // int sampleFramesPerVideoFrame;
    // string encodedVideoFilePath;

    //MediaEncoder videoEncoder;
    Texture2D convertedToTex2d;

    #endregion

    protected override void Start()
    {
        Application.runInBackground = true;

        ExtractedTexturesArr = new Texture2D[VideoFrameNumbersForOneTimeConversion];
        DistortedRenderTexturesArr = new RenderTexture[VideoFrameNumbersForOneTimeConversion];

        //CopiedResultRTArr = new RenderTexture[DistortedRenderTexturesArr.Length];
        //for (int i = 0; i < CopiedResultRTArr.Length; ++i) {
        //  CopiedResultRTArr[i] = new RenderTexture(CurrentScreenResolutions.x, CurrentScreenResolutions.y, 32);
        //  CopiedResultRTArr[i].enableRandomWrite = true;
        //}

        CopiedResultTexArr = new Texture2D[ExtractedTexturesArr.Length];

        #region Bind yieldinstructions as lambdas expression.

        WaitUntilVideoPrepared = new WaitUntil(() => VideoPlayer.isPrepared);

        WaitUntilVideoBlockExtracted = new WaitUntil(() => CurrentFrameCounter != 0 &&
                                                           CurrentFrameCounter % VideoFrameNumbersForOneTimeConversion == 0 ||
                                                           CurrentFrameCounter == (int) VideoPlayer.frameCount - 1); // or When the current frame counter hits the last frame count of the video.

        WaitUntilRenderFinished = new WaitUntil(() => bStopDispatch);
        WaitUntilFrameIsAdded = new WaitUntil(() => bFrameAdded);

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
        // bind the event to invoke explicitly when a new frame is ready.
        VideoPlayer.frameReady += OnReceivedNewFrame;

        VideoPlayer.Prepare();

        #endregion

        base.Start();

        //audioBuf = new Unity.Collections.NativeArray<float>(sampleFramesPerVideoFrame, Unity.Collections.Allocator.Persistent);
        OnInitCreateDistortedImageFromVideoFrame();

        // Compose the video again to encode from the Images list.    
        convertedToTex2d = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y);
    } // Start()

    protected override void OnDisable()
    {
        base.OnDisable();
        if (CoroutineHandle_ProcessVideo != null)
        {
            StopCoroutine(CoroutineHandle_ProcessVideo);
            CoroutineHandle_ProcessVideo = null;
        }
    } // OnDisable()

    public void OnInitCreateDistortedImageFromVideoFrame()
    {
        CoroutineHandle_ProcessVideo = StartCoroutine(Coroutine_ProcessVideo());
    }

    int blockCounter = 0;

    IEnumerator Coroutine_ProcessVideo()
    {
        // Wait for (!videoPlayer.isPrepared)
        yield return WaitUntilVideoPrepared;
        //string encodedVideoFilePath = $"{Application.dataPath}/Resources/Video2Img/{EncodedVideoFolderDirName}/{EncodedVideoFileName}.mp4";
        string encodedFilePath = Path.Combine(Application.dataPath + "/Resources/ConvertVideo", "fiesta.mp4");

        var videoAttr = new VideoTrackAttributes
        {
            frameRate = new MediaRational((int) VideoPlayer.frameRate),
            width = VideoPlayer.width,
            height = VideoPlayer.height
        };

        var audioAttr = new AudioTrackAttributes
        {
            sampleRate = new MediaRational(48000),
            channelCount = 2,
            language = "fr"
        };

        int sampleFramesPerVideoFrame =
            audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;

        var videoEncoder = new MediaEncoder(encodedFilePath, videoAttr, audioAttr);
        // When CurrentFrameCounter has reached at the last frame, yield break.
        // CurrentFrameCount is increased in onReceiveFrame().
        while (CurrentFrameCounter < (int) VideoPlayer.frameCount)
        {
            VideoPlayer.sendFrameReadyEvents = true;
            VideoPlayer.Play();
            AudioSource.Play();

            yield return WaitUntilVideoBlockExtracted; // after yield return, OnReceivedFrame will be executed.
            Debug.Log($"{VideoFrameNumbersForOneTimeConversion} frames are extracted!");
            // for (int i = 0; i < CopiedResultTexArr.Length; ++i) {
            //   CopiedResultTexArr[i] = new Texture2D(ExtractedTexturesArr[i].width,
            //                                       ExtractedTexturesArr[i].height, TextureFormat.RGBAFloat, false, true);
            // }
            //yield return null;

            VideoPlayer.Pause();
            AudioSource.Pause();
            VideoPlayer.sendFrameReadyEvents = false;

            // Distort Img
            //yield return StartCoroutine(ConvertSavedImagesToPredistortedImages(ExtractedTexturesArr, DistortedRenderTexturesArr));
            // Make distorted image to the video.
            //EncodeVideoFromPredistortedImages(CopiedResultTexArr);

            
            for (int i = 0; i < ExtractedTexturesArr.Length; ++i)
            {
                //Debug.Log($"Current encoding idx {i} of {ExtractedTexturesArr.Length}");

                bFrameAdded = videoEncoder.AddFrame(ExtractedTexturesArr[i]);
                
                // if (!bFrameAdded) {
                //     yield break; // break out of the coroutine.
                // }
                // Since AddFrame() takes time, 'yield return null' to wait for the next frame.
                yield return null;

                //encoder.AddSamples(audioBuf);
            } 

            // 1. return back to while() after 100 frames are written
            Debug.Log($"{VideoFrameNumbersForOneTimeConversion} frames are encoded.");
            //Debug.Log($"Video #{blockCounter} is created!");
            //videoEncoder.Dispose();

            // for shortend test.
            if (CurrentFrameCounter > 50) break;

        } // (CurrentFrameCounter < (int) VideoPlayer.frameCount)

        videoEncoder.Dispose();
        Debug.Log("Convert all the frames To Video is Complete");
        // Prevent the call of OnRenderImage();
        bStopDispatch = true;
        yield break;
    } // Coroutine_ProcessVideo()

    void OnReceivedNewFrame(VideoPlayer source, long frameIdx /* UNUSED!*/)
    {
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

        Texture2D videoFrame = new Texture2D(1280, 720);
        if (videoFrame.width != currentRT.width || videoFrame.height != currentRT.height)
        {
            videoFrame.Resize(currentRT.width, currentRT.height);
        }

        // 1. Perform on GPU-side
        //Graphics.CopyTexture(RenderTexture.active, videoFrame);

        // 2. Perform on CPU-side
        videoFrame.ReadPixels(new Rect(0, 0, currentRT.width, currentRT.height), 0, 0);
        videoFrame.Apply();

        //
        // NOTE: Since Texture2D.ReadPixels() is performed on CPU-side
        // it can be a great performant between choosing them.
        // 

        // restore the active render texture by the previous render texture.
        RenderTexture.active = prevRT;

        //SaveExtractedImagesToJPG(videoFrame);

        // Add the current frame texture2d into the extracted textures list.
        ExtractedTexturesArr[CurrentFrameCounter % VideoFrameNumbersForOneTimeConversion] = videoFrame;                
        Debug.Log($"Current Video Frame Count : {CurrentFrameCounter} / {source.frameCount}");
        ++CurrentFrameCounter;
    } // OnReceivedNewFrame

    /// <summary>
    /// Save the extracted image into the real file (jpg).
    /// </summary>
    /// <param name="extractedImg"></param>
    void SaveImageToJPG(Texture2D extractedImg, string dirName, string fileName)
    {
        if (extractedImg == null) return;

        byte[] savedImg = extractedImg.EncodeToJPG();
        File.WriteAllBytes(
            $"{Application.dataPath}/Resources/Video2Img/{ExtractedImgFolderDirName}/{ExtractedImgFileName}_frame_{CurrentFrameCounter}.jpg",
            savedImg);
    }

    void SaveTexture2dsToJPG(Texture2D[] tex2dArr, string dirName, string fileName)
    {
        if (tex2dArr.Length == 0) return;

        Texture2D fwdTex2d = new Texture2D(tex2dArr[0].width, tex2dArr[0].height);
        for (int i = 0; i < tex2dArr.Length; ++i)
        {
            SaveImageToJPG(fwdTex2d, dirName, fileName);
        }
    }

    void SaveRenderTexturesToJPG(RenderTexture[] rtArr, string dirName, string fileName)
    {
        if (rtArr.Length == 0) return;

        Texture2D fwdTex2d = new Texture2D(rtArr[0].width, rtArr[0].height, TextureFormat.RGB24, false);
        for (int i = 0; i < rtArr.Length; ++i)
        {
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
        RenderTexture[] distortedRenderTexturesArr)
    {
        Debug.Log($"Image is being converted to the distorted image");
        for (int i = 0; i < VideoFrameNumbersForOneTimeConversion; ++i)
        {
            // Create Distorted Image (Refreshes RenderTextures and Pass parameters)
            //OnInitCreateDistortedImage(extractedTexturesArr[i]);

            // Wait until the predistorted image is created but yield immediately when the image isn't ready.
            // The number of sampling for rendering is 30
            // If bStopDispatch is false -> yield return immediately and then OnRenderImage() will be called!
            // otherwise go to the next line.
            //yield return WaitUntilRenderFinished;

            // to solve the overwritten problem of the new distorted images as RenderTexture (ConvergedRenderTexForNewImage),
            // TEST #1: Copy the image from ConvergedRnederTexForNewImage to the new RenderTexture declared as a local variable.
            //RenderTexture copied = new RenderTexture(ConvergedRenderTexForNewImage);
            //copied.enableRandomWrite = true;
            //Graphics.CopyTexture(ConvergedRenderTexForNewImage, CopiedResultTexArr[i]);
            Graphics.CopyTexture(extractedTexturesArr[i], CopiedResultTexArr[i]);
            // the predistorted image (ConvergedRenderTexForNewImage) is ready.
            //distortedRenderTexturesArr[i] = ConvergedRenderTexForNewImage;
        }

        yield break;
        //SaveRenderTexturesToJPG(DistortedRenderTexturesList.ToArray(), DistortedImgFolderDirName, DistortedImgFileName);
    }
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