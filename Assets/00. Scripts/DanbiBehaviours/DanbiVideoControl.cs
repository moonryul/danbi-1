using System;
using System.Collections;
using System.IO;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Profiling;
using UnityEngine.Video;
using TMPro;

#if UNITY_EDITOR
using UnityEditor.Media;
#endif

namespace Danbi
{
    [RequireComponent(typeof(VideoPlayer))]
    public class DanbiVideoControl : MonoBehaviour
    {
        [SerializeField, Readonly]
        VideoClip loadedVideo;
        [SerializeField, Readonly]
        string outputVideoName;

        [SerializeField, Readonly]
        string outputVideoLocation;

        [SerializeField, Readonly, Space(15)]
        EDanbiVideoType outputVideoExt;

        [SerializeField]
        string[] createdTemporaryVideoClips;

        [SerializeField, Readonly]
        int currentFrameCounter;

        [SerializeField]
        int dividedMaxFrameCounterForOneBatch = 100;
        [SerializeField, Readonly]
        int totalFrameCounter;
        [SerializeField, Readonly]
        int batchCount;
        [SerializeField]
        int maxBatchToConcatVideos = 10;

        [SerializeField, Readonly]
        string ffmpegExecutableLocation;

        // [Readonly]
        // int sampleFramesPerVideoFrame;

        [SerializeField, Readonly]
        Texture2D extractedTex;

        VideoPlayer videoPlayer;
        // AudioSource audioSource;
        // AudioSampleProvider audioSampleProvider;
        AudioTrackAttributes audioAttr;
        VideoTrackAttributes videoAttr;

        // public float[] AudioClipDataArr;
        // public NativeArray<float> audioBuf;

        WaitUntil WaitUntilVideoPrepared;
        // WaitUntil WaitUntilPredistortedImageReady;
        // WaitUntil WaitUntilAudioSamplesAreEncoded;

        // Coroutine HandleProcessVideo;

        [SerializeField, Readonly]
        bool isFrameReceived;

        // [Readonly]
        // public bool isAudioSamplesReceived = false;

        [SerializeField, Readonly]
        bool isCurrentFrameEncoded;

        // [ReadOnly]
        // public bool isCurrentAudioSampleEncoded = false;

        [SerializeField]
        RenderTexture RenderedRT;

        void Awake()
        {
            // Set the app is able to be run in background
            Application.runInBackground = true;

            videoPlayer = GetComponent<VideoPlayer>();
            // audioSource = GetComponent<AudioSource>();
            // ScreenControl = GetComponent<DanbiScreen>();

            WaitUntilVideoPrepared = new WaitUntil(() => videoPlayer.isPrepared);
            // WaitUntilAudioSamplesAreEncoded = new WaitUntil(() => isCurrentAudioSampleEncoded);

            // bind the panel update
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
            DanbiControl.Call_OnImageRenderedForVideoFrame += (RenderTexture res) => RenderedRT = res;

            // sampleFramesPerVideoFrame = audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
            // AudioClipDataArr = new float[sampleFramesPerVideoFrame];
        }

        void OnDisable()
        {
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        public IEnumerator StartProcessVideo(TMP_Text processDisplay, TMP_Text statusDisplay)
        {
            processDisplay.NullFinally(() => DanbiUtils.LogErr("no process display for generating video detected!"));
            statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating video detected!"));

            int currentBatchCount = 0;
            // trace all temporary video clips.
            createdTemporaryVideoClips = new string[maxBatchToConcatVideos];
            // until the last batch.0
            while (currentBatchCount <= batchCount)
            {
                if (currentBatchCount >= maxBatchToConcatVideos)
                {
                    yield return StartCoroutine(DanbiVideoHelper.ConcatVideoClips(ffmpegExecutableLocation,
                                                                                  outputVideoLocation,
                                                                                  outputVideoName,
                                                                                  createdTemporaryVideoClips,
                                                                                  outputVideoExt));
                    System.GC.Collect();
                    currentBatchCount = 0;
                }

                // TODO: update the text with DanbiStatusDisplayHelper
                // progressDisplayText.text = $"Start to warp" +
                //   "(500 / 25510) " +
                //   "(1.96001%)";
                // TODO: update the text with DanbiStatusDisplayHelper    
                // statusDisplayText.text = "Image generating succeed!";

                string uniqueFileName_only = $"{DanbiFileSys.GetUniqueName()}{DanbiFileExtensionHelper.getVideoExtString(outputVideoExt)}";
                string uniqueName = $"{outputVideoLocation}/{uniqueFileName_only}";
                // perform
                yield return StartCoroutine(Coroutine_MakeVideoClipPart(currentBatchCount, uniqueName, uniqueFileName_only));

                // wait until the current batch is completed.
                yield return new WaitUntil(() => new FileInfo(uniqueName).Exists);

                ++currentBatchCount;
                // yield return null;
            }

            DanbiUIVideoGeneratorGeneratePanelControl.Call_OnAllVideoClipBatchesCompleted?.Invoke();
            // DanbiUtils.Log($"All {currentBatchCount} videos are generated!", EDanbiStringColor.teal, this);
            System.Diagnostics.Process.Start(@"" + outputVideoLocation);
        }

        IEnumerator Coroutine_MakeVideoClipPart(int currentBatchCount, string currentUniqueFileNameFullLocation, string currentUniqueFileName_only)
        {
            yield return WaitUntilVideoPrepared;

            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.Play();

            using (var encoder = new MediaEncoder(currentUniqueFileNameFullLocation, videoAttr, audioAttr))
            {
                var processedTex = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height, TextureFormat.RGBA32, true);
                int currentBatchProgress = 0;
                createdTemporaryVideoClips[currentBatchCount] = currentUniqueFileName_only;

                // while (currentFrameCounter < (int)videoPlayer.frameCount)
                while (currentBatchProgress < dividedMaxFrameCounterForOneBatch)
                {
                    // 2. Wait until the next frame is ready (the next frame and the next audio samples is extracted from the video).
                    while (!isFrameReceived) // && !isAudioSamplesReceived
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    // bool isAudioSampleForwarded = false;
                    // for (int i = 0; i < audioBuf.Length; ++i)
                    // {
                    //     isAudioSampleForwarded = audioBuf[i] != 0.0f;
                    // }

                    // while (!isAudioSampleForwarded)
                    // {
                    //     Debug.Log($"<color=red>Audio samples are still being forwarded!</color>");
                    //     yield return null;
                    // }

                    yield return DistortFrameTexture(processedTex);
                    // System.GC.Collect();
                    // 4. Encodinge process.
                    // Add(Encode) predistorted images to the predistorted video.
                    isCurrentFrameEncoded = encoder.AddFrame(processedTex);
                    // System.GC.Collect();

                    if (!isCurrentFrameEncoded)
                    {
                        Debug.LogError($"<color=red>Failed to AddFrame the predistorted image to the current video encoder.</color>");
                        yield break;
                    }

                    // while (audioBuf.Length == 0)
                    // {
                    //     yield return null;
                    // }
                    // isCurrentAudioSampleEncoded = encoder.AddSamples(audioBuf);
                    // if (!isCurrentAudioSampleEncoded)
                    // {
                    //     Debug.LogError($"<color=red>Failed to AddSample the intact audio samples to the current video encoder.</color>");
                    // }

                    // Debug.Log($"<color=green>{currentFrameCounter++} frames are encoded.</color>");

                    // audioBuf.Dispose();
                    // yield return new WaitUntil(() => !audioBuf.IsCreated);

                    isFrameReceived = false;
                    // isAudioSamplesReceived = false;

                    videoPlayer.sendFrameReadyEvents = true;
                    // audioSampleProvider.enableSampleFramesAvailableEvents = true;

                    // Debug.Log($"Resume the video");
                    // yield return null;
                    // audioSource.Play();
                    videoPlayer.Play();
                    ++currentBatchProgress;
                    ++currentFrameCounter;
                    // DanbiControl.Call_OnImageRendered?.Invoke(false);
                }
                System.GC.Collect();

                // Resource disposal
                Destroy(processedTex);
                // audioSampleProvider.Dispose();
                // Debug.Log($"Convert all the frames to video is complete");                
            }
        }

        IEnumerator DistortFrameTexture(Texture2D res)
        {
            // 1. distort the image.
            // Make the predistorted image ready!
            DanbiUIControl.GenerateImage(extractedTex);

            // 2. wait until the image is processed
            // yield return WaitUntilPredistortedImageReady;
            yield return new WaitUntil(() => RenderedRT != null);

            //var prevRT = RenderTexture.active;
            RenderTexture.active = RenderedRT;

            // 3. Get image
            res.ReadPixels(new Rect(0, 0, RenderedRT.width, RenderedRT.height), 0, 0);
            res.Apply();

            // 4. Restore the previous RenderTexture at the last frame.
            //RenderTexture.active = prevRT;

            // 5. Dispose lefts.
            RenderedRT.Release();
            RenderedRT = null;

            // yield return null;
        }

        void OnVideoPrepareComplete(VideoPlayer vp)
        {
            #region audio part
            // 1. In order to receive audio samples 
            // during playback to the initted AudioSampleProvider.
            // audioSampleProvider = vp.GetAudioSampleProvider(0);
            // 2. Bind the event to invoke explicitly when video is ready to process audio samples.
            // audioSampleProvider.sampleFramesAvailable += (AudioSampleProvider provider, uint sampleFrameCount) =>
            // {
            //     using (var buf = new NativeArray<float>((int)sampleFrameCount * provider.channelCount, Allocator.Temp))
            //     {
            //         Profiler.BeginSample("Start Copying AudioBuffer(NativeArray<float>)");
            //         uint totalProvidedSampleCount = provider.ConsumeSampleFrames(buf);
            //         Debug.Log($"<color=teal>SetupSoftwareAudioOutput.Available got {totalProvidedSampleCount} smaple counts in total!</color>", this);

            //         audioBuf = new NativeArray<float>(buf, Allocator.Persistent);
            //         Profiler.EndSample();
            //         Debug.Log($"Audio buffer is filled againt < length: {audioBuf.Length}");

            //         provider.enableSampleFramesAvailableEvents = false;
            //         isAudioSamplesReceived = true;
            //         // bPredistortedImageReady = false;
            //     }
            // };
            // audioSampleProvider.enableSampleFramesAvailableEvents = true;
            #endregion audio part
        }

        void OnVideoFrameReceived(VideoPlayer source, long frameIdx)
        {
            var prevRT = RenderTexture.active;
            var curRT = source.texture as RenderTexture;
            RenderTexture.active = curRT;

            var srcFrameTex = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height);
            if (srcFrameTex.width != curRT.width || srcFrameTex.height != curRT.height)
            {
                srcFrameTex.Resize(curRT.width, curRT.height);
            }

            // perform on GPU side
            // Graphics.CopyTexture(RenderTexture.active, )

            // on CPU side
            srcFrameTex.ReadPixels(new Rect(0, 0, curRT.width, curRT.height), 0, 0);
            srcFrameTex.Apply();

            RenderTexture.active = prevRT;

            extractedTex = srcFrameTex;
            currentFrameCounter = (int)frameIdx;
            Debug.Log($"Current video frame count: {currentFrameCounter} / {source.frameCount}", this);
            isFrameReceived = true;

            source.Pause();
            source.sendFrameReadyEvents = false;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var videoPanel = control as DanbiUIVideoGeneratorVideoPanelControl;
                loadedVideo = videoPanel.loadedVideo;

                if (videoPlayer.Null())
                {
                    return;
                }

                videoPlayer.playOnAwake = false;
                // audioSource.playOnAwake = false;
                videoPlayer.source = VideoSource.VideoClip;
                // videoPlayer.controlledAudioTrackCount = 1;
                // videoPlayer.audioOutputMode = VideoAudioOutputMode.APIOnly;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                videoPlayer.clip = loadedVideo;

                // bind the event to invoke explicitly when a new fram is ready.
                // videoPlayer.prepareCompleted += OnVideoPrepareComplete;
                videoPlayer.frameReady += OnVideoFrameReceived;

                // setup
                videoAttr = new VideoTrackAttributes
                {
                    frameRate = new MediaRational((int)videoPlayer.frameRate),
                    width = videoPlayer.width,
                    height = videoPlayer.height,
                    includeAlpha = false
                };

                audioAttr = new AudioTrackAttributes
                {
                    sampleRate = new MediaRational(48000),
                    channelCount = 2,
                    language = "en"
                };

                videoPlayer.Prepare();

                totalFrameCounter = (int)loadedVideo.frameCount;
                batchCount = totalFrameCounter / dividedMaxFrameCounterForOneBatch;
            }

            if (control is DanbiUIVideoGeneratorFileSavePathPanelControl)
            {
                var fileSaveControl = control as DanbiUIVideoGeneratorFileSavePathPanelControl;

                outputVideoName = fileSaveControl.fileName;
                outputVideoLocation = fileSaveControl.filePath;
                outputVideoExt = fileSaveControl.videoExt;
            }

            if (control is DanbiUIVideoGeneratorGeneratePanelControl)
            {
                var generatePanel = control as DanbiUIVideoGeneratorGeneratePanelControl;

                ffmpegExecutableLocation = generatePanel.FFMPEGexecutableLocation;
            }
        }
    };
};
