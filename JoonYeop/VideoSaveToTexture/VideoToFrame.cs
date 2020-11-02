// using UnityEngine;
// using UnityEngine.Video;

// public class VideoToFrame : MonoBehaviour {
//   public VideoPlayer videoPlayer;
//   ConvertToImage converToImage;
//   public VideoClip videoClip;
//   VideoInfo videoInfo;
//   ulong TotalFrame;
//   [SerializeField]
//   public long CurrentFrame = 1;

//   public bool bPlay = true;
//   public bool bFrameSaveSuccess = true;

//   void SetValue_FrameSaveSuccess(bool _value) {
//     bFrameSaveSuccess = _value;
//   }

//   // Start is called before the first frame update
//   void Awake() {
//     videoPlayer = GetComponent<VideoPlayer>();
//     converToImage = GetComponent<ConvertToImage>();
//     videoPlayer.clip = videoClip;
//     TotalFrame = videoPlayer.frameCount;



//     //videoPlayer.Stop();
//   }
//   private void Start() {
//     videoPlayer.frame = 0;
//     videoInfo = VideoInfo.videoInfo;
//   }
//   public bool bFrameReady;
//   // Update is called once per frame

//   float Timer = 0;
//   void Update() {
//     if (CurrentFrame > (long)TotalFrame) {
//       return;
//     }


//     if (videoPlayer.frame >= 0 && !bFrameReady) {
//       bFrameReady = true;
//       converToImage.FrameReadyInit();
//       videoInfo.SetVideoInfo((int)videoPlayer.frameRate, videoPlayer.width, videoPlayer.height, false);
//     }


//     if (!bFrameReady)
//       return;


//     if (bFrameSaveSuccess) {
//       CurrentFrame++;

//       bFrameSaveSuccess = false;
//       //Debug.Log("Frame Save Success");
//       //Debug.Log("Save Frame num : " + CurrentFrame.ToString());
//       //Debug.Log("Video Frame num : " + videoPlayer.frame);
//     }

//     if (videoPlayer.frame > CurrentFrame) {
//       videoPlayer.Pause();
//     } else {
//       videoPlayer.Play();

//     }

//     Timer += Time.deltaTime;

//     if (Timer > 1 / videoPlayer.frameRate) {
//       if (videoPlayer.texture)
//         bFrameSaveSuccess = converToImage.AddTextureToListByFrame(videoPlayer.texture);

//       Timer = 0;
//     }

//     if (CurrentFrame > (long)TotalFrame) {
//       ImageConvertToVideo.EncodeVideo();
//     }


//   }
// }
