using UnityEditor;

using UnityEngine;

public class ConvertToImage : MonoBehaviour {
  VideoToFrame videoFrame;

  VideoInfo videoInfo;
  void Start() {
    videoInfo = VideoInfo.videoInfo;
  }

  public void FrameReadyInit() {

    videoFrame = GetComponent<VideoToFrame>();
    Debug.Log("Frame Resolution Width : " + videoFrame.videoPlayer.texture.width);
    Debug.Log("Frame Resolution Height : " + videoFrame.videoPlayer.texture.height);
  }

  public bool AddTextureToListByFrame(Texture _tex) {
    return MaterialConvertToImage(_tex);
  }

  bool MaterialConvertToImage(Texture _tex) {

    Texture mainTexture = _tex;


    Texture2D tex = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);

    RenderTexture currentRT = RenderTexture.active;

    RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);

    Graphics.Blit(mainTexture, renderTexture);
    RenderTexture.active = renderTexture;
    tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
    tex.Apply();



    if (!tex.isReadable) {
      Debug.Log("Text Can't Read");
      return false;
    }


    videoInfo.TexList.Add(tex);

    /// Store file at PNG
    byte[] bytes;
    bytes = tex.EncodeToPNG();

    string filePath = Application.dataPath + "/Resources/ConvertImages/";
    string fileName = filePath + videoFrame.CurrentFrame.ToString() + ".png";

    System.IO.File.WriteAllBytes(fileName, bytes);
    AssetDatabase.ImportAsset(fileName);
    ///

    return true;
  }



}
