namespace Danbi {
  [System.Serializable]
  public enum EDanbiKernelKey : uint {
    None,
    TriconeMirror_Img,
    GeoconeMirror_Img,
    ParaboloidMirror_Img,
    HemisphereMirror_Img,

    TriconeMirror_Proj,
    GeoconeMirror_Proj,
    ParaboloidMirror_Proj,
    HemisphereMirror_Proj,

    PanoramaScreen_View,

    TriconeMirror_Img_With_Lens_Distortion,
    GeoconeMirror_Img_With_Lens_Distortion,
    ParaboloidMirror_Img_With_Lens_Distortion,
    HemisphereMirror_Img_With_Lens_Distortion,

    HemisphereMirror_Img_RT,
  };
}; // namespace Danbi