using System.Collections.Generic;

using UnityEngine;

public class VideoInfo : MonoBehaviour {
  public static VideoInfo videoInfo;

  public List<Texture2D> TexList = new List<Texture2D>();



  void Awake() {
    videoInfo = this;
  }

  public long TotalFrameCount;
  public int FrameRate;

  public uint width;
  public uint height;
  public bool includeAlpha;

  public void SetVideoInfo(int _FrameRate, uint _width, uint _height, bool _alpha) {
    FrameRate = _FrameRate;
    width = _width;
    height = _height;
    includeAlpha = _alpha;
  }


}
