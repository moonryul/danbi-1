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
        EDanbiVideoExt vidExt = EDanbiVideoExt.mp4;
        VideoPlayer previewVidPlayer;

        [Readonly]
        public VideoClip loadedVid;

        [Readonly]
        public string vidPathUnity;

        [Readonly]
        public string vidPathFull;        

        int currentMinutes;
        float currentSeconds;
        int totalMinutes;
        float totalSeconds;

        RawImage previewVideoRawImage;
        TMP_Text videoNameText;
        TMP_Text frameCountText;
        TMP_Text lengthText;

        bool isDisplayPlaybackPaused = false;

        Coroutine CoroutineHandle_DisplayPlaybackTime;

        public override void OnMenuButtonSelected(Stack<Transform> lastClicked)
        {
            // Pause the preview video while the panel is turned off.
            if (previewVidPlayer != null)
            {
                previewVidPlayer.Pause();
                isDisplayPlaybackPaused = true;
            }

            base.OnMenuButtonSelected(lastClicked);
        }

        protected override void SaveValues()
        {
            PlayerPrefs.SetString("videoGeneratorVideo-vidPathUnity", vidPathUnity);
            PlayerPrefs.SetString("videoGeneratorVideo-vidPathFull", vidPathFull);
            PlayerPrefs.SetInt("videoGeneratorVideo-vidExt", (int)vidExt);
            PlayerPrefs.SetInt("videoGeneratorVideo-vidType", (int)m_vidType);
        }

        IEnumerator coroutine_updateVideoInfo()
        {
            // Update the video inspector.
            // 1. video name
            videoNameText.text = $"Name: {loadedVid.name}";
            // 2. total frame count
            frameCountText.text = $"Frame Count: {loadedVid.frameCount}";
            // 3. playback time (minutes, seconds)
            totalMinutes = (int)loadedVid.length / 60;
            // to round = 반올림.
            totalSeconds = (int)System.Math.Round(loadedVid.length - (totalMinutes * 60), 2);
            lengthText.text = $"0m 0s / {totalMinutes}m {totalSeconds}s";

            // 5. update the preview video player.
            // init preview video player.
            if (previewVidPlayer.Null())
            {
                previewVidPlayer = GetComponent<VideoPlayer>();
                previewVidPlayer.playOnAwake = false;
            }

            previewVidPlayer.sendFrameReadyEvents = true;
            // put the received frame into the raw image.
            previewVidPlayer.frameReady += (VideoPlayer source, long frameIdx) =>
            {
                previewVideoRawImage.texture = source.texture as RenderTexture;
            };

            // set the using video clip
            // TODO: error sompetimes.

            yield return new WaitUntil(() => !loadedVid.Null());
            previewVidPlayer.clip = loadedVid;

            // play the preview video.
            previewVidPlayer.Play();
            if (CoroutineHandle_DisplayPlaybackTime != null)
            {
                StopCoroutine(CoroutineHandle_DisplayPlaybackTime);
                CoroutineHandle_DisplayPlaybackTime = null;
            }

            // sync the video before displaying the playback times.
            DanbiUISync.onPanelUpdated?.Invoke(this);
            isDisplayPlaybackPaused = false;
            CoroutineHandle_DisplayPlaybackTime = StartCoroutine(Coroutine_DisplayPlaytime(Panel.transform));
        }

        protected override void LoadPreviousValues(params Selectable[] uiElements)
        {
            string prevVidPathUnity = PlayerPrefs.GetString("videoGeneratorVideo-vidPathUnity", default);
            if (!string.IsNullOrEmpty(prevVidPathUnity))
            {
                vidPathUnity = prevVidPathUnity;
                loadedVid = Resources.Load<VideoClip>(vidPathUnity);

                StartCoroutine(coroutine_updateVideoInfo());
            }

            var prevVidPathFull = PlayerPrefs.GetString("videoGeneratorVideo-vidPathFull", default);
            if (!string.IsNullOrEmpty(prevVidPathFull))
            {
                vidPathFull = prevVidPathFull;
            }

            var prevVidType = PlayerPrefs.GetInt("videoGeneratorVideo-vidExt", default);
            vidExt = (EDanbiVideoExt)prevVidType;


            DanbiUISync.onPanelUpdated?.Invoke(this);
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
                    DanbiUISync.onPanelUpdated?.Invoke(this);
                }
            );

            // bind the select target video button
            var selectTargetVideoButton = panel.GetChild(1).GetComponent<Button>();
            selectTargetVideoButton.onClick.AddListener(() => StartCoroutine(Coroutine_SelectTargetVideo(panel)));

            videoNameText = panel.GetChild(2).GetComponent<TMP_Text>();
            frameCountText = panel.GetChild(3).GetComponent<TMP_Text>();
            lengthText = panel.GetChild(4).GetComponent<TMP_Text>();
            previewVideoRawImage = panel.GetChild(5).GetComponent<RawImage>();

            // 2. bind the play the preview video button.
            var playPreviewVideoButton = panel.GetChild(6).GetComponent<Button>();
            playPreviewVideoButton.onClick.AddListener(
                () =>
                {
                    // check video player has a clip
                    if (previewVidPlayer.clip.Null())
                    {
                        return;
                    }

                    // play(resume) the video.
                    if (!previewVidPlayer.isPlaying || previewVidPlayer.isPaused)
                    {
                        previewVidPlayer.Play();
                        isDisplayPlaybackPaused = false;
                    }
                }
            );

            // 3. bind the pause the previde bideo button.
            var pausePreviewVideoButton = panel.GetChild(7).GetComponent<Button>();
            pausePreviewVideoButton.onClick.AddListener(
                () =>
                {
                    // check video player has a clip
                    if (previewVidPlayer.clip.Null())
                    {
                        return;
                    }
                    // pause the video
                    if (previewVidPlayer.isPlaying || !previewVidPlayer.isPaused)
                    {
                        previewVidPlayer.Pause();
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
            DanbiFileSys.GetResourcePathIntact(out vidPathFull, out _);
            DanbiFileSys.GetResourcePathForResources(out vidPathUnity, out _);

            // Load the video.
            loadedVid = Resources.Load<VideoClip>(vidPathUnity);
            yield return new WaitUntil(() => !loadedVid.Null());

            StartCoroutine(coroutine_updateVideoInfo());

            DanbiUISync.onPanelUpdated?.Invoke(this);
        }

        IEnumerator Coroutine_DisplayPlaytime(Transform panel)
        {
            // run while the current minutes and the current seconds are below the total lengths.
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
