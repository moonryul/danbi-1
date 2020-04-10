using System;
using System.IO;
using UnityEngine;

public static class DanbiImage {
  //public delegate void EvtOnSaveImage();
  //public static EvtOnSaveImage OnSaveImage;
  public static Vector2Int CurrentScreenResolutions;
  public static string filePath;
  public static string fileName;

  static DanbiImage() {
    filePath = Application.dataPath + "/Resources/RenderedImages/";
  }

  static Texture2D LoadPNG(string filePath) {
    Texture2D tex = default;
    byte[] fileData;

    if (File.Exists(filePath)) {
      fileData = File.ReadAllBytes(filePath);
      tex = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y);
      tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
    }
    return tex;
  }

  static void SaveRenderTexture(RenderTexture rt, string FilePath) {
    byte[] bytes = ToTexture2D(rt).EncodeToPNG();
    System.IO.File.WriteAllBytes(FilePath, bytes);
  }   //SaveRenderTexture()

  static Texture2D ToTexture2D(RenderTexture rt) {
    var tex = new Texture2D(CurrentScreenResolutions.x, CurrentScreenResolutions.y,
                            TextureFormat.RGB24, false);
    var savedRT = RenderTexture.active;
    RenderTexture.active = rt;
    //ReadPixels(Rect source, int destX, int destY);
    tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
    // read from the active  renderTexture to tex
    tex.Apply();
    RenderTexture.active = savedRT;
    return tex;
  }

  public static void CaptureScreenToFileName(ref EDanbiSimulatorMode currentSimulatorMode,
                                             ref RenderTexture convergedRT,
                                             out Texture2D distortedResult,
                                             string name) {
    
    switch (currentSimulatorMode) {
      case EDanbiSimulatorMode.CAPTURE:
      fileName = filePath + name + ".png";

      // save the renderTexture _converged which holds the result of cameraMain's rendering
      SaveRenderTexture(convergedRT, fileName);
      distortedResult = LoadPNG(fileName);

      #region unused
      // if (Input.GetKeyDown(KeyCode.F12)) {
      //if (Input.GetKeyDown(KeyCode.Space))
      //{
      // If the resolution of gameview = 1920 x 1080, superSize =2 ==>
      // 2 x2 larger image is obtained => 3840 x 2160
      // if the resolution of gameview = 960 x 540, super size = 4 => 4K
      // The compression ratio of TIFF= 2:1, That of png = 2.7:1 (lossless?)


      // Dump the current renderTexture to _PredistortedImage renderTexture which was set in the
      // Inspector.

      // Opens a file selection dialog for a PNG file and saves a selected texture to the file.
      // import System.IO;

      //function Attive()
      // {

      //var texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);

      //var textures = new Texture2D(128, 128, TextureFormat.ARGB32, false);
      //if (textures.Length == 0)
      //{
      //    EditorUtility.DisplayDialog("Select Textures",
      //                    "The selection must contain at least one texture!", "Ok");
      //    return;
      //}
      //  var path = UnityEditor.EditorUtility.SaveFolderPanel("Save textures to directory", "", "");
      //if (path.Length != 0)
      //{
      //    // for( var texture : Texture2D in textures) {
      //    // Convert the texture to a format compatible with EncodeToPNG
      //    if (texture.format != TextureFormat.DXT1 && texture.format != TextureFormat.RGB24)
      //    {
      //        var newTexture = Texture2D(texture.width, texture.height);
      //        newTexture.SetPixels(texture.GetPixels(0), 0);
      //        texture = newTexture;
      //    }
      //    var pngData = texture.EncodeToPNG();
      //    if (pngData != null)
      //        File.WriteAllBytes(path + "/" + nameTexture.text + ".png", pngData);
      //    else
      //        Debug.Log("Could not convert " + texture.name + " to png. Skipping saving texture");
      //}
      // UnityEditor.AssetDatabase.Refresh();
      //}
      // SaveTexture(_convergedForCreateImage, _PredistortedImage);
      // _PredistortedImage will be used by "Project Predistorted Image" task.
      //ScreenCapture.CaptureScreenshot(fileName, superSize);
      #endregion

      Debug.Log("The PredistortedImage Screen Captured to the Folder=" + filePath);
      // "-1" means no process is in progress
      currentSimulatorMode = EDanbiSimulatorMode.NONE;

      //CurrentPlaceHolder.SetActive(false);  // clean the path name box

      //StopPlay(); // stop the play of the current task and be ready for the next button command
      //mPauseNewRendering = true;
      break;

      case EDanbiSimulatorMode.PROJECTION:
      #region 
      ////string fileName = name + _currentSample + Time.time + ".png";

      ////string fileName = Application.persistentDataPath + name + _currentSample + Time.time + ".png";
      //filePath = Application.dataPath + "/Resources/RenderedImages/";
      //fileName = filePath + name + CurrentSamplingCount + "_" + Time.time + ".png";

      //// save the renderTexture _converged which holds the result of cameraMain's rendering
      //SaveRenderTexture(ConvergedRenderTexForProjecting, fileName);

      //// Dump the current renderTexture to _ProjectedImage renderTexture which was set in the
      //// Inspector.

      ////ProjectedResultImage = LoadPNG(fileName);
      //// _ProjectedImage will be used by "View Panorama Image" task.

      ////ScreenCapture.CaptureScreenshot(fileName, superSize);
      //Debug.Log("The Projected Image Screen Captured (View Independent Image to Folder="
      //                + filePath);

      //// _CaptureOrProjectOrView = -1; // "-1" means no process is in progress
      //CaptureOrProjectOrView = -1;
      //// Now that the screen image has been saved, enable Rendering


      //CurrentPlaceHolder.SetActive(false);  // clean the path name box

      ////StopPlay(); // stop the play of the current task and be ready for the next button command
      //// mPauseNewRendering = true;
      #endregion
      //break;

      case EDanbiSimulatorMode.VIEW:
      #region 
      ////string fileName = name + _currentSample + Time.time + ".png";

      ////string fileName = Application.persistentDataPath + name + _currentSample + Time.time + ".png";

      //string filePath = Application.dataPath + "/Resources/RenderedImages/";
      //string fileName = filePath + name + CurrentSamplingCount + "_" + Time.time + ".png";

      //// save the renderTexture _converged which holds the result of cameraMain's rendering
      //SaveRenderTexture(ConvergedRenderTexForPresenting, fileName);

      ////ScreenCapture.CaptureScreenshot(fileName, superSize);
      //Debug.Log("The Projected Image Screen Captured (View Dependent Image) to Folder="
      //           + filePath);

      ////_CaptureOrProjectOrView = -1; // "-1" means no process is in progress
      //CaptureOrProjectOrView = -1;
      //// Now that the screen image has been saved, enable Rendering


      //CurrentPlaceHolder.SetActive(false);  // clean the path name box

      ////StopPlay(); // stop the play of the current task and be ready for the next button command
      //// mPauseNewRendering = true;
      #endregion
      //break;

      case EDanbiSimulatorMode.NONE:
      default: {
        distortedResult = default;
        Utils.StopPlayManually();
      }
      break;
    }
  } // CaptureScreenToFileName
};
