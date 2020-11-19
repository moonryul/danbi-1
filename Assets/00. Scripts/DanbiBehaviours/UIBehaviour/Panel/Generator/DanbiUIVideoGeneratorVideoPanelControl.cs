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
    public class DanbiUIVideoGeneratorVideoPanelControl : DanbiUIPanelControl
    {
        EDanbiVideoType m_vidType = EDanbiVideoType.Regular;
        public EDanbiVideoType vidType => m_vidType;
        EDanbiVideoExt m_vidExt = EDanbiVideoExt.mp4;
        VideoPlayer m_previewVidPlayer;

        [Readonly]
        public string m_vidPath;

        int m_currentMinutes;
        float m_currentSeconds;
        int m_totalMinutes;
        float m_totalSeconds;

        RawImage m_previewVideoRawImage;
        TMP_Text m_videoNameText;
        TMP_Text m_frameCountText;
        TMP_Text m_lengthText;

        bool m_isDisplayPlaybackPaused = false;

        Coroutine m_CoroutineHandle_DisplayPlaybackTime;

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            // Pause the preview video while the panel is turned off.
            if (m_previewVidPlayer != null)
            {
                m_previewVidPlayer.Pause();
                m_isDisplayPlaybackPaused = true;
            }

            base.OnMenuButtonSelected(lastClicked);
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("videoGeneratorVideo-vidPath", m_vidPath);
            PlayerPrefs.SetInt("videoGeneratorVideo-vidExt", (int)m_vidExt);
            PlayerPrefs.SetInt("videoGeneratorVideo-vidType", (int)m_vidType);
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            var prevVidPath = PlayerPrefs.GetString("videoGeneratorVideo-vidPath", default);
            if (!string.IsNullOrEmpty(prevVidPath))
            {
                m_vidPath = prevVidPath;

                UpdateVideoPreview();
            }

            var prevVidType = PlayerPrefs.GetInt("videoGeneratorVideo-vidExt", default);
            m_vidExt = (EDanbiVideoExt)prevVidType;


            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        protected override void AddListenerForPanelFields()
        {
            base.AddListenerForPanelFields();

            var panel = Panel.transform;
            panel.GetComponent<RectTransform>().anchoredPosition += new Vector2(0.0f, 70.0f);
            // bind video type
            var videoTypeOptions = new List<string> { "Regular", "Panorama" };
            var videoTypeDropdown = panel.GetChild(0).GetComponent<TMP_Dropdown>();
            videoTypeDropdown.AddOptions(videoTypeOptions);
            videoTypeDropdown.onValueChanged.AddListener(
                (int option) =>
                {
                    m_vidType = (EDanbiVideoType)option;
                    DanbiUISync.onPanelUpdate?.Invoke(this);
                }
            );

            // bind the select target video button
            var selectTargetVideoButton = panel.GetChild(1).GetComponent<Button>();
            selectTargetVideoButton.onClick.AddListener(() => StartCoroutine(this.Coroutine_SelectTargetVideo()));

            m_videoNameText = panel.GetChild(2).GetComponent<TMP_Text>();
            m_frameCountText = panel.GetChild(3).GetComponent<TMP_Text>();
            m_lengthText = panel.GetChild(4).GetComponent<TMP_Text>();
            m_previewVideoRawImage = panel.GetChild(5).GetComponent<RawImage>();

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
                        m_isDisplayPlaybackPaused = false;
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
                        m_isDisplayPlaybackPaused = true;
                    }
                }
            );

            LoadPreviousValues();
        }

        IEnumerator Coroutine_SelectTargetVideo()
        {
            // https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
            var filters = new string[] { ".mp4", ".avi", "m4v", ".mov", ".webm", ".wmv" };
            string startingPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            yield return DanbiFileSys.OpenLoadDialog(startingPath,
                                                     filters,
                                                     "Load Video",
                                                     "Select");
            DanbiFileSys.GetResourcePathIntact(out m_vidPath, out _);
            UpdateVideoPreview();
            DanbiUISync.onPanelUpdate?.Invoke(this);
        }

        void UpdateVideoPreview()
        {
            // 5. update the preview video player.
            // init preview video player.
            if (m_previewVidPlayer.Null())
            {
                m_previewVidPlayer = GetComponent<VideoPlayer>();
                m_previewVidPlayer.playOnAwake = false;
            }

            m_previewVidPlayer.sendFrameReadyEvents = true;
            // put the received frame into the raw image.
            m_previewVidPlayer.frameReady += (VideoPlayer source, long frameIdx) =>
            {
                m_previewVideoRawImage.texture = source.texture as RenderTexture;
            };

            // set the using video clip
            // TODO: error sompetimes.

            // yield return new WaitUntil(() => !loadedVid.Null());
            // previewVidPlayer.clip = loadedVid;
            m_previewVidPlayer.url = m_vidPath;
            m_previewVidPlayer.Prepare();
            m_previewVidPlayer.prepareCompleted += (VideoPlayer vp) =>
            {
                // play the preview video.
                vp.Play();
                if (m_CoroutineHandle_DisplayPlaybackTime != null)
                {
                    StopCoroutine(m_CoroutineHandle_DisplayPlaybackTime);
                    m_CoroutineHandle_DisplayPlaybackTime = null;
                }

                // Update the video inspector.
                // 1. video name
                m_videoNameText.text = $"Name: {vp.url}";
                // 2. total frame count
                m_frameCountText.text = $"Frame Count: {vp.frameCount}";
                // 3. playback time (minutes, seconds)
                m_totalMinutes = (int)vp.length / 60;
                // to round = 반올림.
                m_totalSeconds = (int)System.Math.Round(vp.length - (m_totalMinutes * 60), 2);
                m_lengthText.text = $"0m 0s / {m_totalMinutes}m {m_totalSeconds}s";

                // sync the video before displaying the playback times.
                DanbiUISync.onPanelUpdate?.Invoke(this);
                m_isDisplayPlaybackPaused = false;
                m_CoroutineHandle_DisplayPlaybackTime = StartCoroutine(this.Coroutine_DisplayPlaytime());
            };
        }

        IEnumerator Coroutine_DisplayPlaytime()
        {
            // run while the current minutes and the current seconds are below the total lengths.
            while (m_currentMinutes <= m_totalMinutes && m_currentSeconds <= m_totalSeconds)
            {
                yield return new WaitUntil(() => !m_isDisplayPlaybackPaused);

                if (m_currentSeconds >= 60.0f)
                {
                    if (m_currentMinutes <= m_totalMinutes)
                    {
                        m_currentMinutes += 1;
                        m_currentSeconds = 0.0f;
                    }
                }
                else
                {
                    m_currentSeconds += 1.0f;
                }

                m_lengthText.text = $"{m_currentMinutes}m {m_currentSeconds}s / {m_totalMinutes}m {m_totalSeconds}s";
                yield return new WaitForSeconds(1.0f);
            }
        }

    };
};
