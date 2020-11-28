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
    public class DanbiUIProjectionVideoPanelControl : DanbiUIPanelControl
    {
        EDanbiVideoType m_vidType = EDanbiVideoType.Regular;
        public EDanbiVideoType vidType => m_vidType;
        VideoPlayer m_previewVidPlayer;

        [Readonly]
        public string vidPath;

        int currentMinutes;
        float currentSeconds;
        int totalMinutes;
        float totalSeconds;

        RawImage m_previewVideoRawImage;
        TMP_Text m_lengthText;
        TMP_Text m_videoNameText;
        TMP_Text m_frameCountText;

        bool isDisplayPlaybackPaused = false;

        Coroutine CoroutineHandle_DisplayPlaybackTime;

        public delegate void OnProjectionVideoUpdate(VideoPlayer vp);
        public static OnProjectionVideoUpdate onProjectionVideoUpdate;

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            if (m_previewVidPlayer != null)
            {
                m_previewVidPlayer.Pause();
                isDisplayPlaybackPaused = true;
            }

            base.OnMenuButtonSelected(lastClicked);
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("projectionVideo-vidPath", vidPath);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevVideoPath = PlayerPrefs.GetString("projectionVideo-vidPath", default);
            if (!string.IsNullOrEmpty(prevVideoPath))
            {
                vidPath = prevVideoPath;

                UpdateVideoPreview();
                DanbiUISync.onPanelUpdate?.Invoke(this);
            }
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            panel.GetComponent<RectTransform>().anchoredPosition += new Vector2(0.0f, 70.0f);

            // bind video type
            var vidTypeOptions = new List<string> { "Regular", "360 Video" };
            var vidTypeDropdown = panel.GetChild(0).GetComponent<TMP_Dropdown>();
            vidTypeDropdown.AddOptions(vidTypeOptions);
            vidTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_vidType = (EDanbiVideoType)option;
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // 1. bind the select target video button
            var selectTargetVideoButton = panel.GetChild(1).GetComponent<Button>();
            selectTargetVideoButton.onClick.AddListener(() => StartCoroutine(this.Coroutine_SelectTargetVideo()));

            m_videoNameText = panel.GetChild(2).GetComponent<TMP_Text>();
            m_frameCountText = panel.GetChild(3).GetComponent<TMP_Text>();
            m_lengthText = panel.GetChild(4).GetComponent<TMP_Text>();

            // 2. bind the play the preview video button.
            var playPreviewVideoButton = panel.GetChild(6).GetComponent<Button>();
            playPreviewVideoButton.onClick.AddListener(
                () =>
                {
                    // Check preview video player has a url.
                    if (m_previewVidPlayer.url == default)
                    {
                        return;
                    }

                    // play(resume) the video.
                    if (m_previewVidPlayer.isPaused)
                    {
                        m_previewVidPlayer.Play();
                        isDisplayPlaybackPaused = false;
                    }
                }
            );

            // 3. bind the pause the previde bideo button.
            var pausePreviewVideoButton = panel.GetChild(7).GetComponent<Button>();
            pausePreviewVideoButton.onClick.AddListener(
                () =>
                {
                    // Check preview video player has a url.
                    if (m_previewVidPlayer.url == default)
                    {
                        return;
                    }

                    // pause the video
                    if (m_previewVidPlayer.isPlaying)
                    {
                        m_previewVidPlayer.Pause();
                        isDisplayPlaybackPaused = true;
                    }
                }
            );

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SelectTargetVideo()
        {
            // https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
            // var filters = new string[] { ".mp4", ".avi", "m4v", ".mov", ".webm", ".wmv" };
            string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     default,
                                                     "Select Video (.mp4, .avi, m4v, .mov, .webm, .wmv)",
                                                     "Select");
            DanbiFileSys.GetResourcePathIntact(out vidPath, out _);
            UpdateVideoPreview();
            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        void UpdateVideoPreview()
        {
            // retrieve the RayImage for preview video player.
            var previewVideoRawImage = Panel.transform.GetChild(5).GetComponent<RawImage>();

            // update the preview video player.
            // init preview video player.
            if (m_previewVidPlayer.Null())
            {
                m_previewVidPlayer = GetComponent<VideoPlayer>();
                m_previewVidPlayer.playOnAwake = false;
            }

            m_previewVidPlayer.sendFrameReadyEvents = true;
            m_previewVidPlayer.frameReady += (VideoPlayer source, long frameIdx) =>
            {
                previewVideoRawImage.texture = source.texture as RenderTexture;
            };
            // set the using video clip
            // TODO: exception.
            // previewVideoPlayer.clip = loadedVideo;
            m_previewVidPlayer.url = vidPath;
            m_previewVidPlayer.Prepare();
            m_previewVidPlayer.prepareCompleted += (VideoPlayer vp) =>
            {
                // onProjectionVideoUpdate?.Invoke(loadedVideo);
                // play the video.
                vp.Play();
                if (CoroutineHandle_DisplayPlaybackTime != null)
                {
                    StopCoroutine(CoroutineHandle_DisplayPlaybackTime);
                    CoroutineHandle_DisplayPlaybackTime = null;
                }

                // Update the video inspector.
                // 1. video name
                m_videoNameText.text = $"Name: {vp.url}";
                // 2. total frame count
                m_frameCountText.text = $"Frame Count: {vp.frameCount}";
                // 3. playback time (minutes, seconds)
                totalMinutes = (int)vp.length / 60;
                // to round = 반올림.
                totalSeconds = (int)System.Math.Round(vp.length - (totalMinutes * 60), 2);
                m_lengthText.text = $"0m 0s / {totalMinutes}m {totalSeconds}s";

                isDisplayPlaybackPaused = false;
                CoroutineHandle_DisplayPlaybackTime = StartCoroutine(this.Coroutine_DisplayPlaytime());
                // onProjectionVideoUpdate?.Invoke(vp);
            };
        }

        IEnumerator Coroutine_DisplayPlaytime()
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

                m_lengthText.text = $"{currentMinutes}m {currentSeconds}s / {totalMinutes}m {totalSeconds}s";
                yield return new WaitForSeconds(1.0f);
            }
        }
    };
};
