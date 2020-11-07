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

using UnityEditor.Media;
// #if UNITY_EDITOR
// #endif

namespace Danbi
{
    [RequireComponent(typeof(VideoPlayer))]
    public class DanbiVideoControl : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        VideoClip loadedVideo;
        
        [SerializeField, Readonly]
        string outputVideoName;

        [SerializeField, Readonly]
        string outputVideoLocation;

        [SerializeField, Readonly, Space(15)]
        EDanbiVideoExt outputVideoExt;

        [SerializeField]
        string[] createdTemporaryVideoClips;

        [SerializeField, Readonly]
        int currentFrameCounter = 0;

        [SerializeField]
        int dividedMaxFrameCounterForOneBatch = 100;
        [SerializeField, Readonly]
        int totalFrameCounter;
        [SerializeField, Readonly]
        int batchCount;
        [SerializeField]
        int maxBatchNumberToConcatVideos = 10;

        [SerializeField, Readonly]
        string ffmpegExecutableLocation;

        // [Readonly]
        // int sampleFramesPerVideoFrame;

        [SerializeField, Readonly]
        Texture2D m_receivedFrame;

        VideoPlayer videoPlayer;
        // AudioSource audioSource;
        // AudioSampleProvider audioSampleProvider;
        AudioTrackAttributes audioAttr;
        VideoTrackAttributes videoAttr;

        // public float[] AudioClipDataArr;
        // public NativeArray<float> audioBuf;

        // WaitUntil WaitUntilPredistortedImageReady;
        // WaitUntil WaitUntilAudioSamplesAreEncoded;

        // Coroutine HandleProcessVideo;

        [SerializeField, Readonly]
        bool m_isNextFrameReceived = false;

        // [Readonly]
        // public bool isAudioSamplesReceived = false;

        // [ReadOnly]
        // public bool isCurrentAudioSampleEncoded = false;

        DanbiScreen ScreenControl;

        [SerializeField]
        RenderTexture m_distortedRT;

        void Awake()
        {
            // Set the app is able to be run in background
            Application.runInBackground = true;

            videoPlayer = GetComponent<VideoPlayer>();
            // audioSource = GetComponent<AudioSource>();
            ScreenControl = GetComponent<DanbiScreen>();
            // WaitUntilAudioSamplesAreEncoded = new WaitUntil(() => isCurrentAudioSampleEncoded);

            // bind the panel update
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
            DanbiComputeShaderControl.onSampleFinished += (RenderTexture converged_resultRT) => m_distortedRT = converged_resultRT;

            // sampleFramesPerVideoFrame = audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
            // AudioClipDataArr = new float[sampleFramesPerVideoFrame];
        }

        void Start()
        {
            // used to disable the script
        }

        public void StartMakingVideo(TMP_Text processDisplay, TMP_Text statusDisplay)
        {
            StartCoroutine(ProcessVideoInBatch(processDisplay, statusDisplay));
        }

        IEnumerator ProcessVideoInBatch(TMP_Text processDisplay, TMP_Text statusDisplay)
        {
            processDisplay.NullFinally(() => DanbiUtils.LogErr("no process display for generating video detected!"));
            statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating video detected!"));

            int currentBatchCount = 0;
            // trace all temporary video clips.
            createdTemporaryVideoClips = new string[maxBatchNumberToConcatVideos];
            var uniqueFileNames = new string[batchCount + 1];
            int unqFileCounter = 0;
            // until the last batch.
            while (currentBatchCount <= batchCount)
            {
                // last clip.
                if (currentFrameCounter >= totalFrameCounter)
                {
                    yield return StartCoroutine(DanbiVideoHelper.ConcatVideoClips(ffmpegExecutableLocation,
                                                                                  outputVideoLocation,
                                                                                  outputVideoName,
                                                                                  createdTemporaryVideoClips,
                                                                                  outputVideoExt));
                    DanbiVideoHelper.DisposeAllTemps(uniqueFileNames, outputVideoLocation);
                    yield break;
                }

                string uniqueFileName_only = $"{DanbiFileSys.GetUniqueName()}{DanbiFileExtensionHelper.getVideoExtString(outputVideoExt)}";
                string uniqueName = $"{outputVideoLocation}/{uniqueFileName_only}";
                uniqueFileNames[unqFileCounter++] = uniqueName;

                if (currentBatchCount >= maxBatchNumberToConcatVideos)
                {
                    yield return StartCoroutine(DanbiVideoHelper.ConcatVideoClips(ffmpegExecutableLocation,
                                                                                  outputVideoLocation,
                                                                                  outputVideoName,
                                                                                  createdTemporaryVideoClips,
                                                                                  outputVideoExt));
                    GC.Collect();
                    currentBatchCount = 0;
                    for (var i = 0; i < createdTemporaryVideoClips.Length; ++i)
                    {
                        createdTemporaryVideoClips[i] = default(string);
                    }
                }

                // TODO: update the text with DanbiStatusDisplayHelper
                // progressDisplayText.text = $"Start to warp" +
                //   "(500 / 25510) " +
                //   "(1.96001%)";
                // TODO: update the text with DanbiStatusDisplayHelper    
                // statusDisplayText.text = "Image generating succeed!";

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
            // yield return new WaitUntil(() => { return videoPlayer.isPrepared; });
            // TODO: DELETE 
            // while (!videoPlayer.isPrepared)
            // {
            //     yield return null;
            // }

            // 1. Start playing video at first.
            // until the video is played, OnReceivedFrame() won't be called.
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.Play();

            using (var encoder = new MediaEncoder(currentUniqueFileNameFullLocation, videoAttr, audioAttr))
            {
                // executed only once
                // var texForVideoFrame = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height, TextureFormat.RGBA32, true);
                // var texForVideoFrame = new Texture2D(ScreenControl.screenResolution.x, ScreenControl.screenResolution.y, TextureFormat.ARGB32, false);
                var texForVideoFrame = new Texture2D(ScreenControl.screenResolution.x, ScreenControl.screenResolution.y, TextureFormat.RGBA32, false);
                int currentBatchProgress = 0;
                createdTemporaryVideoClips[currentBatchCount] = currentUniqueFileName_only;

                // while (currentFrameCounter < (int)videoPlayer.frameCount)
                // while (currentFrameCounter < 1000)
                while (currentBatchProgress < dividedMaxFrameCounterForOneBatch && currentFrameCounter < totalFrameCounter)
                {
                    // 2. Wait until the next frame is ready (the next frame and the next audio samples is extracted from the video).
                    // if (m_isNextFrameReceived == true), then the video is paused!                     
                    while (!m_isNextFrameReceived) // && !isAudioSamplesReceived
                    {
                        yield return null;
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
                    // texForVideoFrame.Resize(texForVideoFrame.width, texForVideoFrame.height, TextureFormat.ARGB32, false);

                    yield return StartCoroutine(DistortCurrentFrame(texForVideoFrame));

                    // System.GC.Collect();
                    // 4. Encodinge process.
                    // Add(Encode) distorted one image to the predistorted video.

                    // texForVideoFrame.Resize(texForVideoFrame.width, texForVideoFrame.height, TextureFormat.RGBA32, false);

                    if (!encoder.AddFrame(texForVideoFrame))
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

                    // Debug.Log($"<color=green>{currentFrameCounter} frames are encoded.</color>");

                    // audioBuf.Dispose();
                    // yield return new WaitUntil(() => !audioBuf.IsCreated);

                    m_isNextFrameReceived = false;
                    // isAudioSamplesReceived = false;

                    // 4. RESUME the video!! -> m_isNextFrameReceived == false, from now on.
                    // Debug.Log($"Resume the video");

                    videoPlayer.sendFrameReadyEvents = true;
                    // audioSampleProvider.enableSampleFramesAvailableEvents = true;

                    // yield return null;
                    // audioSource.Play();
                    videoPlayer.Play();

                    ++currentBatchProgress;
                    ++currentFrameCounter;
                    // DanbiControl.Call_OnImageRendered?.Invoke(false);
                } // while (currentFrameCounter < (int)videoPlayer.frameCount)
                // System.GC.Collect();

                // Resource disposal
                Destroy(texForVideoFrame);
                // audioSampleProvider.Dispose();
            }
        }

        IEnumerator DistortCurrentFrame(Texture2D texForDistortedFrame)
        {
            // 1. distort the image.
            // Make the predistorted image ready!
            // received frame is used as a target texture for the ray-tracing master.
            // m_distortedRT is being filled with the result of CreateDistortedImage().
            DanbiManager.instance.onGenerateImage?.Invoke(m_receivedFrame);

            // 2. wait until the image is processed

            yield return new WaitUntil(() => m_distortedRT != null);

            // Profiler.BeginSample("Read Pixels into the Texture");
            //var prevRT = RenderTexture.active;
            RenderTexture.active = m_distortedRT;
            //Graphics.CopyTexture(sourceTexture, 0, 0, (int)r.x, (int)r.y, width, height, output, 0, 0, 0, 0);
            // Graphics.CopyTexture(m_distortedRT, 0, 0, 0, 0, m_distortedRT.width, m_distortedRT.height,
            //                      texForDistortedFrame, 0, 0, 0, 0);



            //Graphics.CopyTexture can only copy memory with the same size
            // (src=33177600 bytes dst=8294400 bytes), maybe the size (src=1920 * 1080 dst=1920 * 1080) 
            // or format (src=RGBA32 SFloat dst=RGBA8 sRGB) are not compatible.


            // 3. Get image
            texForDistortedFrame.ReadPixels(new Rect(0, 0, m_distortedRT.width, m_distortedRT.height), 0, 0);
            texForDistortedFrame.Apply();

            // Profiler.EndSample();
            RenderTexture.active = null;

            // 4. Restore the previous RenderTexture at the last frame.
            //RenderTexture.active = prevRT;

            // 5. Dispose lefts.
            // m_distortedRT.Release();
            // m_distortedRT = null;
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
            // var prevRT = RenderTexture.active;
            // Texture has Texture2D and RenderTexture as a subtype.
            // var curRT = source.texture as RenderTexture;
            // RenderTexture.active = curRT;

            // var srcFrameTex = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height);
            // if (srcFrameTex.width != curRT.width || srcFrameTex.height != curRT.height)
            // {
            //     srcFrameTex.Resize(curRT.width, curRT.height);
            // }

            // perform on GPU side
            // Graphics.CopyTexture(RenderTexture.active, )            

            // if (m_receivedFrame.width != curRT.width ||
            //     m_receivedFrame.width != curRT.height ||
            //     m_receivedFrame.graphicsFormat != curRT.graphicsFormat)
            // {
            //     m_receivedFrame.Resize(curRT.width, curRT.height, curRT.graphicsFormat, false);
            // }

            // Graphics.CopyTexture(source.texture, m_receivedFrame);
            Graphics.CopyTexture(source.texture, 0, 0, 0, 0, source.texture.width, source.texture.height, m_receivedFrame, 0, 0, 0, 0);
            // on CPU side
            // srcFrameTex.ReadPixels(new Rect(0, 0, curRT.width, curRT.height), 0, 0);
            // srcFrameTex.Apply();
            // m_receivedFrame.ReadPixels(new Rect(0, 0, curRT.width, curRT.height), 0, 0);
            // m_receivedFrame.Apply();

            // RenderTexture.active = prevRT;
            // RenderTexture.active = null;

            // m_receivedFrame = srcFrameTex;
            // currentFrameCounter = (int)frameIdx;
            // Debug.Log($"Current video frame count: {currentFrameCounter} / {source.frameCount}", this);
            m_isNextFrameReceived = true;

            source.Pause();
            source.sendFrameReadyEvents = false;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var videoPanel = control as DanbiUIVideoGeneratorVideoPanelControl;
                loadedVideo = videoPanel.loadedVid;

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
                videoPlayer.sendFrameReadyEvents = false;

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

                m_receivedFrame = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height);

                totalFrameCounter = (int)loadedVideo.frameCount;
                batchCount = totalFrameCounter / dividedMaxFrameCounterForOneBatch;
            }

            if (control is DanbiUIVideoGeneratorFileOptionPanelControl)
            {
                var fileSaveControl = control as DanbiUIVideoGeneratorFileOptionPanelControl;

                outputVideoName = fileSaveControl.vidNameOnly;
                outputVideoLocation = fileSaveControl.vidPathOnly;
                outputVideoExt = fileSaveControl.vidExtOnly;
            }

            // if (control is DanbiUIVideoGeneratorGeneratePanelControl)
            // {
            //     var generatePanel = control as DanbiUIVideoGeneratorGeneratePanelControl;

            //     // ffmpegExecutableLocation = generatePanel.FFMPEGexecutableLocation;
            // }
        }
    };
};
