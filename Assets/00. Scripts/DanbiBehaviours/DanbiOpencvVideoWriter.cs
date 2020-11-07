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
        int m_maxFrameCount;

        [SerializeField, Readonly]
        int m_currentFrameCount;

        [SerializeField]
        int m_dbgMaxFrameCount;

        [SerializeField, Readonly, Space(10)]
        bool m_isSaving;

        RenderTexture m_distortedRT;
        VideoCapture m_vidCapturer;
        VideoWriter m_vidWriter;



        void Awake()
        {
            Application.runInBackground = true;
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
            DanbiComputeShaderControl.onSampleFinished += OnSampleFinished;
            DanbiUIVideoGeneratorGeneratePanelControl.onVideoSave += () => m_isSaving = true;
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

            if (!(m_distortedRT is null))
            {
                m_distortedRT.Release();
                m_distortedRT = null;
            }
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var vidControl = control as DanbiUIVideoGeneratorVideoPanelControl;

                m_vidName = vidControl.vidPathFull;
            }

            if (control is DanbiUIVideoGeneratorFileOptionPanelControl)
            {
                var saveVidControl = control as DanbiUIVideoGeneratorFileOptionPanelControl;

                m_videoExt = saveVidControl.vidExtOnly;
                m_videoCodec = saveVidControl.vidCodec;
                m_targetFrameRate = saveVidControl.targetFrameRate;
                m_savedVidName = saveVidControl.savePathAndNameFull;
                m_savedVidLocation = saveVidControl.vidPathOnly;
            }
        }

        void OnSampleFinished(RenderTexture converged_resultRT)
        {
            m_distortedRT = converged_resultRT;
        }

        void OnVideoSave()
        {
            m_isSaving = true;
        }

        public IEnumerator MakeVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            progressDisplay.NullFinally(() => DanbiUtils.LogErr("no process display for generating video detected!"));
            statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating video detected!"));

            m_vidCapturer = new VideoCapture(m_vidName);

            if (!m_vidCapturer.isOpened())
            {
                m_vidCapturer.release();
                m_vidCapturer = null;
                DanbiUtils.LogErr($"Failed to open the selected video at {m_vidName}");
                yield break;
            }

            // 3. init persistant resources
            var receivedFrameMat = new Mat();
            var distortedFrameMat = new Mat();
            var texForVideoFrame = new Texture2D((int)m_vidCapturer.get(3), (int)m_vidCapturer.get(4), TextureFormat.RGBA32, false);
            // 4. calc video frame counts.
            m_currentFrameCount = 0;
            m_maxFrameCount = (int)m_vidCapturer?.get(DanbiOpencvVideoCapturePropID.frame_count);

            int codec_fourcc = DanbiOpencvVideoCodec_fourcc.get_fourcc_videoCodec(m_videoCodec);
            if (codec_fourcc == -999)
            {
                DanbiUtils.LogErr($"codec is invalid! codec propID -> {codec_fourcc}");
                yield break;
            }

            var frameSize = new Size(m_vidCapturer.get(3), m_vidCapturer.get(4)); // width , height

            m_vidWriter = new VideoWriter(m_savedVidName, codec_fourcc, m_targetFrameRate, frameSize);

            while (m_currentFrameCount < m_maxFrameCount - 1 || !m_isSaving)
            // while (m_currentFrameCount < m_dbgMaxFrameCount)
            {
                // read the new Frame into 'newFrameMat'.
                if (!m_vidCapturer.read(receivedFrameMat))
                {
                    DanbiUtils.LogErr($"Failed to read the current video frame! <No next frame>");
                    break;
                }

                if (receivedFrameMat.empty())
                {
                    DanbiUtils.LogErr("Frame failed to receive the captured frame from the video!");
                    break;
                }

                Utils.matToTexture2D(receivedFrameMat, texForVideoFrame);

                yield return StartCoroutine(DistortCurrentFrame(texForVideoFrame));

                if (distortedFrameMat.width() != texForVideoFrame.width || distortedFrameMat.height() != texForVideoFrame.height)
                {
                    distortedFrameMat = new Mat(texForVideoFrame.height, texForVideoFrame.width, CvType.CV_8UC4);
                }

                Utils.texture2DToMat(texForVideoFrame, distortedFrameMat, false);

                if (distortedFrameMat.empty())
                {
                    DanbiUtils.LogErr("Frame failed to receive the distorted result!");
                    break;
                }
                // write the newFrameMat into the video writer
                m_vidWriter.write(distortedFrameMat);

                // TODO: update the text with DanbiStatusDisplayHelper
                // progressDisplayText.text = $"Start to warp" +
                //   "(500 / 25510) " +
                //   "(1.96001%)";
                // TODO: update the text with DanbiStatusDisplayHelper    
                // statusDisplayText.text = "Image generating succeed!";

                ++m_currentFrameCount;
            }

            // dispose resources.
            m_vidCapturer.release();
            m_vidWriter.release();
            receivedFrameMat.release();
            distortedFrameMat.release();
            texForVideoFrame = null;

            Application.runInBackground = false;

            yield return new WaitUntil(() => new System.IO.FileInfo(m_savedVidName).Exists);
            DanbiUtils.Log($"Save operation is completed!");
            System.Diagnostics.Process.Start(@"" + m_savedVidLocation);

            m_isSaving = false;
        }

        IEnumerator DistortCurrentFrame(Texture2D texForDistortedFrame)
        {
            // 1. distort the image.
            // Make the predistorted image ready!
            // received frame is used as a target texture for the ray-tracing master.
            // m_distortedRT is being filled with the result of CreateDistortedImage().
            DanbiManager.instance.onGenerateImage?.Invoke(texForDistortedFrame);

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
            texForDistortedFrame.ReadPixels(new UnityEngine.Rect(0, 0, m_distortedRT.width, m_distortedRT.height), 0, 0);
            texForDistortedFrame.Apply();

            // Profiler.EndSample();
            RenderTexture.active = null;

            // 4. Restore the previous RenderTexture at the last frame.
            //RenderTexture.active = prevRT;

            // 5. Dispose lefts.
            // m_distortedRT.Release();
            // m_distortedRT = null;
        }
    };
};
