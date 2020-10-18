using System.Collections;
using System.IO;

using Unity.Collections;

using UnityEditor.Media;

using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UnityEngine.Video;

using TMPro;

namespace Danbi
{
    public class DanbiUIVideoGeneratorVideoPanelControl : DanbiUIPanelControl
    {
        EDanbiVideoType videoType = EDanbiVideoType.mp4;

        [Readonly]
        public VideoClip loadedVideo;

        [Readonly]
        public string videoPath;

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("videoGeneratorVideo-videoPath", videoPath);
            PlayerPrefs.SetInt("videoGeneratorVideo-videoType", (int)videoType);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevVideoPath = PlayerPrefs.GetString("videoGeneratorVideo-videoPath", default);
            if (!string.IsNullOrEmpty(prevVideoPath))
            {
                videoPath = prevVideoPath;
                loadedVideo = Resources.Load<VideoClip>(videoPath);

                DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
            }

            var prevVideoType = PlayerPrefs.GetInt("videoGeneratorVideo-videoType", default);
            videoType = (EDanbiVideoType)prevVideoType;
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;

            // 1. bind the select target video button
            var selectTargetVideoButton = panel.GetChild(0).GetComponent<Button>();
            selectTargetVideoButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetVideo(panel)));

            // var videoPreviewRawImage = panel.GetChild(4).GetComponent<RawImage>();

            LoadPreviousValues(selectTargetVideoButton);
        }

        IEnumerator Coroutine_SelectTargetVideo(Transform panel)
        {
            // https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
            var filters = new string[] { ".mp4", ".avi", "m4v", ".mov", ".webm", ".wmv" };
            string startingPath = default;
#if UNITY_EDITOR
            startingPath = Application.dataPath + "/Resources/";
#else
            startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
#endif

            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filters,
                                                     "Load Video",
                                                     "Select");

            DanbiFileSys.GetResourcePathForResources(out var actualPath, out var resourceName);

            // Load the video.
            loadedVideo = Resources.Load<VideoClip>(actualPath);
            yield return new WaitUntil(() => !loadedVideo.Null());

            // Update the video inspector.
            var videoNameText = panel.GetChild(1).GetComponent<TMP_Text>();
            videoNameText.text = $"Name: {resourceName}";

            var frameCountText = panel.GetChild(2).GetComponent<TMP_Text>();
            frameCountText.text = $"Frame Count: {loadedVideo.frameCount}";

            var lengthText = panel.GetChild(3).GetComponent<TMP_Text>();
            double minutes = loadedVideo.length / 60.0;
            double seconds = loadedVideo.length - (minutes * 60.0);
            lengthText.text = $"Length : {minutes}m {seconds}s";

            // update the preview video player.
            var previewVideoPlayer = GetComponent<VideoPlayer>();
            previewVideoPlayer.clip = loadedVideo;
            if (previewVideoPlayer.isPlaying)
            {
                previewVideoPlayer.Play();
            }

            var previewVideoRawImage = panel.GetChild(4).GetComponent<RawImage>();
            previewVideoRawImage.texture = previewVideoPlayer.targetTexture;

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }
    };
};
