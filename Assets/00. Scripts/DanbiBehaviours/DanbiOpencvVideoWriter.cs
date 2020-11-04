using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoioModule;

namespace Danbi
{
    public class DanbiOpencvVideoWriter : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        string m_vidName;
        [SerializeField, Readonly]
        string m_savedVidName;
        [SerializeField, Readonly]
        string m_savedVidLocation;

        [SerializeField, Readonly, Space(10)]
        EDanbiVideoExt m_videoExt;

        [SerializeField, Readonly]
        EDanbiOpencvCodec_fourcc_ m_videoCodec;

        [SerializeField, Readonly]
        float m_targetFrameRate;

        [SerializeField, Readonly, Space(10)]
        int m_maxFrame;
        [SerializeField, Readonly]
        int m_currentFrameCount;

        RenderTexture m_distoretedRT;

        VideoCapture m_vidCapturer;
        VideoWriter m_vidWriter;

        void Awake()
        {
            Application.runInBackground = true;
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
            DanbiComputeShaderControl.onSampleFinished += OnSampleFinished;

        }

        void OnApplicationQuit()
        {
            if (!(m_vidCapturer is null))
            {
                m_vidCapturer.release();
                m_vidCapturer = null;
            }

            if (!(m_vidWriter is null))
            {
                m_vidWriter.release();
                m_vidWriter = null;
            }

            if (!(m_distoretedRT is null))
            {
                m_distoretedRT.Release();
                m_distoretedRT = null;
            }
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var vidControl = control as DanbiUIVideoGeneratorVideoPanelControl;

                #region init video capturer
                // 1. init videoCapture
                string vidPath = vidControl.vidPathFull;

                if (string.IsNullOrEmpty(vidPath))
                {
                    return;
                }

                if (m_vidCapturer is null)
                {
                    m_vidCapturer = new VideoCapture(vidPath);
                }
                else
                {
                    m_vidCapturer.open(vidPath);
                }

                if (!m_vidCapturer.isOpened())
                {
                    DanbiUtils.LogErr($"Failed to open the selected video at {vidPath}");
                    return;
                }
                #endregion init video capturer
            }

            if (control is DanbiUIVideoGeneratorFileOptionPanelControl)
            {
                var saveVidControl = control as DanbiUIVideoGeneratorFileOptionPanelControl;

                m_videoExt = saveVidControl.vidExtOnly;
                m_videoCodec = saveVidControl.vidCodec;
                m_targetFrameRate = saveVidControl.targetFrameRate;

                #region init video writer
                if (m_vidCapturer is null)
                {
                    // DanbiUtils.LogErr($"Video Capturer isn't intialized yet! set video file first at Generator->Video->Video");
                    return;
                }

                if (!m_vidCapturer.isOpened())
                {
                    return;
                }

                // init videoWriter
                string saveVidPath = saveVidControl.savePathAndNameFull;

                if (string.IsNullOrEmpty(saveVidPath))
                {
                    return;
                }

                int codec_fourcc = DanbiOpencvVideoCodec_fourcc.get_fourcc_videoCodec(m_videoCodec);
                if (codec_fourcc == -999)
                {
                    DanbiUtils.LogErr($"codec is invalid! codec propID -> {codec_fourcc}");
                    return;
                }

                var frameSize = new Size(m_vidCapturer.get(3), m_vidCapturer.get(4));

                if (m_vidWriter is null)
                {
                    m_vidWriter = new VideoWriter(saveVidPath, codec_fourcc, m_targetFrameRate, frameSize);
                }
                else
                {
                    m_vidWriter.open(saveVidPath, codec_fourcc, m_targetFrameRate, frameSize);
                }

                if (!m_vidWriter.isOpened())
                {
                    DanbiUtils.LogErr($"Failed to open Video Writer!");
                    return;
                }
                #endregion init video writer
            }
        }

        void OnSampleFinished(RenderTexture converged_resultRT)
        {
            m_distoretedRT = converged_resultRT;
        }

        public void MakeVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            progressDisplay.NullFinally(() => DanbiUtils.LogErr("no process display for generating video detected!"));
            statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating video detected!"));

            // 3. init persistant resources
            var newFrameMat = new Mat();
            // 4. calc video frame counts.
            m_maxFrame = (int)m_vidCapturer?.get(DanbiOpencvVideoCapturePropID.frame_count);
            m_currentFrameCount = 0;

            while (m_currentFrameCount <= m_maxFrame)
            {
                // read the new Frame into 'newFrameMat'.
                if (m_vidCapturer.read(newFrameMat))
                {
                    DanbiUtils.LogErr($"Failed to read the current video frame! <No next frame>");
                    return;
                }

                if (newFrameMat.empty())
                {
                    DanbiUtils.LogErr("Frame failed to receive the captured frame from the video!");
                    break;
                }
                // write the newFrameMat into the video writer
                m_vidWriter.write(newFrameMat);

                // TODO: update the text with DanbiStatusDisplayHelper
                // progressDisplayText.text = $"Start to warp" +
                //   "(500 / 25510) " +
                //   "(1.96001%)";
                // TODO: update the text with DanbiStatusDisplayHelper    
                // statusDisplayText.text = "Image generating succeed!";
            }
            // dispose resources.
            m_vidCapturer.release();
            m_vidWriter.release();
            newFrameMat.release();
            Application.runInBackground = false;
        }

        static Mat Tex2DToMat(Texture2D tex)
        {
            if (tex is null)
            {
                return default;
            }

            var resMat = new Mat(tex.height, tex.width, CvType.CV_8UC4); // == RGBA32
            Utils.texture2DToMat(tex, resMat);
            return resMat;
        }
    };
};
