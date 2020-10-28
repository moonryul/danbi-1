using System.Collections;
using System.IO;

using Unity.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Experimental.Audio;
using UnityEngine.Experimental.Video;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UnityEngine.Video;

using TMPro;
#if UNITY_EDITOR
using UnityEditor.Media;
#endif

namespace Danbi
{
    public class DanbiUIInteractionVideoPanelControl : DanbiUIPanelControl
    {
        VideoPlayer previewVideoPlayer;

        [Readonly]
        public VideoClip loadedVideo;

        [Readonly]
        public string videoPath;

        int currentMinutes;
        float currentSeconds;
        int totalMinutes;
        float totalSeconds;

        TMP_Text lengthText;

        bool isDisplayPlaybackPaused = false;

        Coroutine CoroutineHandle_DisplayPlaybackTime;

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            if (previewVideoPlayer != null)
            {
                previewVideoPlayer.Pause();
                isDisplayPlaybackPaused = true;
            }

            base.OnMenuButtonSelected(lastClicked);
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("videoGeneratorVideo-videoPath", videoPath);
        }

        void updateVideo()
        {
            // Update the video inspector.
            var videoNameText = Panel.transform.GetChild(1).GetComponent<TMP_Text>();
            videoNameText.text = $"Name: {loadedVideo.name}";

            var frameCountText = Panel.transform.GetChild(2).GetComponent<TMP_Text>();
            frameCountText.text = $"Frame Count: {loadedVideo.frameCount}";

            totalMinutes = (int)loadedVideo.length / 60;
            totalSeconds = (int)System.Math.Round(loadedVideo.length - (totalMinutes * 60), 2);
            lengthText.text = $"0m 0s / {totalMinutes}m {totalSeconds}s";

            // retrieve the RayImage for preview video player.
            var previewVideoRawImage = Panel.transform.GetChild(4).GetComponent<RawImage>();

            // update the preview video player.
            // init preview video player.
            if (previewVideoPlayer.Null())
            {
                previewVideoPlayer = GetComponent<VideoPlayer>();
                previewVideoPlayer.playOnAwake = false;
            }

            previewVideoPlayer.sendFrameReadyEvents = true;
            previewVideoPlayer.frameReady += (VideoPlayer source, long frameIdx) =>
            {
                previewVideoRawImage.texture = source.texture as RenderTexture;
            };
            // set the using video clip
            // TODO: exception.
            previewVideoPlayer.clip = loadedVideo;

            // play the video.
            previewVideoPlayer.Play();
            if (CoroutineHandle_DisplayPlaybackTime != null)
            {
                StopCoroutine(CoroutineHandle_DisplayPlaybackTime);
                CoroutineHandle_DisplayPlaybackTime = null;
            }
            CoroutineHandle_DisplayPlaybackTime = StartCoroutine(Coroutine_DisplayPlaytime(Panel.transform));
            isDisplayPlaybackPaused = false;

        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevVideoPath = PlayerPrefs.GetString("videoGeneratorVideo-videoPath", default);
            if (!string.IsNullOrEmpty(prevVideoPath))
            {
                videoPath = prevVideoPath;
                loadedVideo = Resources.Load<VideoClip>(videoPath);

                updateVideo();
                DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
            }
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            panel.GetComponent<RectTransform>().anchoredPosition += new Vector2(0.0f, 70.0f);

            // 1. bind the select target video button
            var selectTargetVideoButton = panel.GetChild(0).GetComponent<Button>();
            selectTargetVideoButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetVideo(panel)));

            lengthText = panel.GetChild(3).GetComponent<TMP_Text>();

            // 2. bind the play the preview video button.
            var playPreviewVideoButton = panel.GetChild(5).GetComponent<Button>();
            playPreviewVideoButton.onClick.AddListener(
                () =>
                {
                    // check video player has a clip
                    if (previewVideoPlayer.clip.Null())
                    {
                        return;
                    }
                    // play(resume) the video.
                    if (!previewVideoPlayer.isPlaying || previewVideoPlayer.isPaused)
                    {
                        previewVideoPlayer.Play();
                        isDisplayPlaybackPaused = false;
                    }
                }
            );

            // 3. bind the pause the previde bideo button.
            var pausePreviewVideoButton = panel.GetChild(6).GetComponent<Button>();
            pausePreviewVideoButton.onClick.AddListener(
                () =>
                {
                    // check video player has a clip
                    if (previewVideoPlayer.clip.Null())
                    {
                        return;
                    }
                    // pause the video
                    if (previewVideoPlayer.isPlaying || !previewVideoPlayer.isPaused)
                    {
                        previewVideoPlayer.Pause();
                        isDisplayPlaybackPaused = true;
                    }
                }
            );

            LoadPreviousValues();
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

            DanbiFileSys.GetResourcePathForResources(out videoPath, out _);

            // Load the video.
            loadedVideo = Resources.Load<VideoClip>(videoPath);
            yield return new WaitUntil(() => !loadedVideo.Null());

            updateVideo();

            DanbiUISync.Call_OnPanelUpdate?.Invoke(this);
        }

        IEnumerator Coroutine_DisplayPlaytime(Transform panel)
        {
            while (currentMinutes <= totalMinutes && currentSeconds <= totalSeconds)
            {
                yield return new WaitUntil(() => !isDisplayPlaybackPaused);

                if (currentSeconds >= 60.0f)
                {
                    if (currentMinutes <= totalMinutes)
                    {
                        currentMinutes += 1;
                        currentSeconds = 0.0f;
                    }
                }
                else
                {
                    currentSeconds += 1.0f;
                }

                lengthText.text = $"{currentMinutes}m {currentSeconds}s / {totalMinutes}m {totalSeconds}s";
                yield return new WaitForSeconds(1.0f);
            }
        }
    };
};
