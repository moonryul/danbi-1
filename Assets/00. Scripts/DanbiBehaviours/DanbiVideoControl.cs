using System.Collections;
using System.IO;

using Unity.Collections;

using UnityEditor.Media;

using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Profiling;
using UnityEngine.Video;

namespace Danbi
{
    [RequireComponent(typeof(VideoPlayer))]
    public class DanbiVideoControl : MonoBehaviour
    {
        [Readonly]
        public VideoClip loadedVideo;

        [Readonly, Space(15)]
        public string EncodedVideoFolderDir;

        [Readonly]
        public string EncodedVideoFileName;

        [Readonly]
        int currentFrameCounter;

        [SerializeField]
        int dbg_maxFrameCounter = 100;

        [Readonly]
        int sampleFramesPerVideoFrame;

        [Readonly]
        public Texture2D extractedTex;

        VideoPlayer videoPlayer;
        // AudioSource audioSource;
        // AudioSampleProvider audioSampleProvider;
        AudioTrackAttributes audioAttr;
        VideoTrackAttributes videoAttr;

        // public float[] AudioClipDataArr;
        // public NativeArray<float> audioBuf;

        WaitUntil WaitUntilVideoPrepared;
        WaitUntil WaitUntilFrameIsEncoded;
        WaitUntil WaitUntilPredistortedImageReady;
        WaitUntil WaitUntilAudioSamplesAreEncoded;

        Coroutine HandleProcessVideo;

        [Readonly]
        public bool isFrameReceived = false;

        [Readonly]
        public bool isAudioSamplesReceived = false;

        [Readonly]
        public bool isCurrentFrameEncoded = false;

        [ReadOnly]
        public bool isCurrentAudioSampleEncoded = false;

        DanbiScreen ScreenControl;

        void Awake()
        {
            // Set the app is able to be run in background
            Application.runInBackground = true;

            videoPlayer = GetComponent<VideoPlayer>();
            // audioSource = GetComponent<AudioSource>();
            ScreenControl = GetComponent<DanbiScreen>();

            // bind the panel update
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;

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

            // sampleFramesPerVideoFrame = audioAttr.channelCount * audioAttr.sampleRate.numerator / videoAttr.frameRate.numerator;
            // AudioClipDataArr = new float[sampleFramesPerVideoFrame];
        }

        void OnDisable()
        {
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        void Start()
        {
            videoPlayer.playOnAwake = false;
            // audioSource.playOnAwake = false;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.APIOnly;
            videoPlayer.clip = loadedVideo;
            // bind the event to invoke explicitly when a new fram is ready.
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.prepareCompleted += OnVideoPrepareComplete;
            videoPlayer.frameReady += OnVideoFrameReceived;

            WaitUntilVideoPrepared = new WaitUntil(
                () => videoPlayer.isPrepared
            );

            WaitUntilFrameIsEncoded = new WaitUntil(
                () => isCurrentFrameEncoded
            );

            WaitUntilAudioSamplesAreEncoded = new WaitUntil(
                () => isCurrentAudioSampleEncoded
            );

            WaitUntilPredistortedImageReady = new WaitUntil(
                () => true // TODO : Wait until the image is finally generated.
            );

            videoPlayer.Prepare();

            // TODO: Sync the result resolution with the video dimension.
            // CurrentScreenResolutions.x = (int)VideoPlayer.width;
            // CurrentScreenResolutions.y = (int)VideoPlayer.height;            

            // HandleProcessVideo = StartCoroutine("Coroutine_ProcessVideo");
        }

        public void StartProcessVideo() => StartCoroutine(Coroutine_ProcessVideo());

        IEnumerator Coroutine_ProcessVideo()
        {
            yield return WaitUntilVideoPrepared;

            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.Play();
            var processedTex = new Texture2D((int)videoPlayer.width, (int)videoPlayer.height, TextureFormat.RGBA32, true);

            using (var encoder = new MediaEncoder(EncodedVideoFolderDir, videoAttr, audioAttr))
            {
                // 1. while CurrentFrameCounter is lower than videoPlayer.frameCount
                while (currentFrameCounter < (int)videoPlayer.frameCount)
                {
                    // 2. Wait until the next frame is ready (the next frame and the next audio samples is extracted from the video).
                    while (!isFrameReceived) // && !isAudioSamplesReceived
                    {
                        yield return null;
                        // if (videoPlayer.isPlaying)
                        // {
                        //     videoPlayer.Pause();
                        // }
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
                    // 4. Encodinge process.
                    // Add(Encode) predistorted images to the predistorted video.
                    isCurrentFrameEncoded = encoder.AddFrame(processedTex);

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

                    Debug.Log($"<color=green>{currentFrameCounter} frames are encoded.</color>");

                    // audioBuf.Dispose();

                    // yield return new WaitUntil(() => !audioBuf.IsCreated);

                    isFrameReceived = false;
                    // isAudioSamplesReceived = false;

                    videoPlayer.sendFrameReadyEvents = true;
                    // audioSampleProvider.enableSampleFramesAvailableEvents = true;

                    // Debug.Log($"Resume the video");
                    videoPlayer.Play();
                    // audioSource.Play();



                    System.GC.Collect();
                    if (currentFrameCounter > dbg_maxFrameCounter)
                        break;
                }
            }

            // Resource disposal
            Destroy(processedTex);
            // audioSampleProvider.Dispose();
            Debug.Log($"Convert all the frames to video is complete");

            // TODO:
        }

        IEnumerator DistortFrameTexture(Texture2D res)
        {
            // 1. distort the image.
            // Make the predistorted image ready!
            DanbiUIControl.GenerateImage(res);
            // 2. wait until the image is processed
            yield return WaitUntilPredistortedImageReady;

            var prevRT = RenderTexture.active;
            // TODO: Connect the upsampled result render texture.
            // var curRT = ConvergedRT_highRes;
            var curRT = default(RenderTexture);
            RenderTexture.active = curRT;

            // 3. Get image
            res.ReadPixels(new Rect(0, 0, curRT.width, curRT.height), 0, 0);
            res.Apply();

            // 4. Restore the previous RenderTexture at the last frame.
            RenderTexture.active = prevRT;
        }

        void OnVideoPrepareComplete(VideoPlayer vp)
        {
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

            // vp.sendFrameReadyEvents = true;
            // vp.Play();
        }

        void OnVideoFrameReceived(VideoPlayer source, long frameIdx)
        {
            var prevRT = RenderTexture.active;
            var curRT = source.texture as RenderTexture;
            RenderTexture.active = curRT;

            var srcFrameTex = new Texture2D(1920, 1080);
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
            source.Pause();
            source.sendFrameReadyEvents = false;

            extractedTex = srcFrameTex;
            currentFrameCounter = (int)frameIdx;
            Debug.Log($"Current video frame count: {currentFrameCounter} / {source.frameCount}", this);
            isFrameReceived = true;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var videoPanel = control as DanbiUIVideoGeneratorVideoPanelControl;

                if (!string.IsNullOrEmpty(videoPanel.videoPath))
                {
                    loadedVideo = videoPanel.loadedVideo;
                }
            }
        }
    };
};
