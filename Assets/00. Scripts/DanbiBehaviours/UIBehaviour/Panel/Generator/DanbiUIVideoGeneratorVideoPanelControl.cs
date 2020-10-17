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
    public class DanbiUIVideoGeneratorVideoPanelControl : DanbiUIPanelControl
    {
        EDanbiVideoType VideoType = EDanbiVideoType.mp4;
        
        [Readonly]
        public VideoClip loadedVideo;

        [Readonly, Space(15)]
        public string EncodedVideoFolderDir;

        [Readonly]
        public string EncodedVideoFileName;

        [Readonly]
        int currentFrameCounter;
        
        [SerializeField]
        int dbg_maxFrameCounter = 120;

        VideoPlayer videoPlayer;
        AudioSource audioSource;
        AudioSampleProvider audioSampleProvider;
        AudioTrackAttributes audioAttr;
        VideoTrackAttributes videoAttr;
        public NativeArray<float> audioBuf;

        WaitUntil WaitUntilVideoPrepared;
        WaitUntil WaitUntilFrameIsEncoded;
        WaitUntil WaitUntilPredistortedImageReady;

        Coroutine HandleProcessVideo;
        
        [Readonly]
        public bool isFrameReceived = false;

        [Readonly]
        public bool isAudioSamplesReceived = false;

        [Readonly]
        public bool bCurrentFrameEncoded = false;
        [ReadOnly]
        public bool bCurrentAudioSampleEncoded = false;

    };
};
