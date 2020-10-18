using UnityEngine;
using UnityEditor;


using System.IO;
using System;
using UnityEngine.UI;

public class BakeTextureProcess : MonoBehaviour
{

    public Material ImageMaterial;
    Vector2Int Resolution;
    //float TopLength;
    //public Slider topRatio;
    //public Text topRatioText;

    public Image previewImage;
    public InputField ResolutionX;
    public InputField ResolutionY;

    Texture2D g_tex;
    bool hasImage;
    //bool hasResolution;
    bool hasPath;

    public InputField pathString;
    string path;

   

    private void Update()
    {
        CheckInput();
    }
    string fileName = "";

    public void RoadImage()
    {
        //string chosenFile = EditorUtility.SaveFilePanelInProject("Choose image file", "ImageSelect",
        //              "png", "Please enter a file name to save the image to", "aaa");
        string chosenFile = EditorUtility.OpenFilePanel("Choose image file", "", "");
        
        Debug.Log(chosenFile);
        byte[] bytes = File.ReadAllBytes(chosenFile);
        Texture2D texture = new Texture2D(1,1, TextureFormat.RGB24, true);
        texture.filterMode = FilterMode.Trilinear;
        if(texture.LoadImage(bytes))
        {
            previewImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            ImageMaterial.SetTexture("_MainTex", previewImage.mainTexture);
            g_tex = texture;

            hasImage = true;
        }
       

       
      
    }

    public void UpdateResolution()
    {
        //Resolution.x = int.Parse(ResolutionX.text);
        //Resolution.y = int.Parse(ResolutionY.text);
        Resolution.x = 3840;
        Resolution.y = 2160;
    }

    public void OpenPathSetWindow()
    {
        string directory = "Assets";
        string fileName;
        if (ImageMaterial != null)
            fileName = ImageMaterial.name + ".png";
        else if (Path.GetFileName(path) == "")
        {
            fileName = "default" + ".png";
        }
        else
        {
            fileName = Path.GetFileName(path);
        }

        string chosenFile = EditorUtility.SaveFilePanelInProject("Choose image file", fileName,
                       "png", "Please enter a file name to save the image to", directory);
        if (!string.IsNullOrEmpty(chosenFile))
        {
            path = chosenFile;
            pathString.text = chosenFile;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(430, 20, 500, 1800));


        if(!hasImage)
        {
            EditorGUILayout.HelpBox("Select Image", MessageType.Error);
        }
        //if (!hasResolution)
        //{
        //    EditorGUILayout.HelpBox("Please set a 'Resolution' bigger than zero.", MessageType.Error);
        //}
        if (!hasPath)
        {
            EditorGUILayout.HelpBox("Can't find any file path. Please Set the file path", MessageType.Error);
        }
        GUILayout.EndArea();

    }

    void CheckInput()
    {
        //ImageMaterial.SetFloat("_Top", topRatio.value);
        //topRatioText.text =  "3. Top Ratio  " + (topRatio.value * 100).ToString() + "%";


        //hasResolution = Resolution.x > 0 && Resolution.y > 0;
        hasPath = path != null;
        Debug.Log(path);
        try
        {
            string ext = Path.GetExtension(path);
        }
        catch (ArgumentException) { }
    }

    string FileField(string path)
    {
        //allow the user to enter output file both as text or via file browser
        EditorGUILayout.LabelField("FileName");
        using (new GUILayout.HorizontalScope())
        {

            if (GUILayout.Button("Set Path"))
            {
                //set default values for directory, then try to override them with values of existing path
                string directory = "Assets";
                string fileName;
                if (ImageMaterial != null)
                    fileName = ImageMaterial.name + ".png";
                else if (Path.GetFileName(path) == "")
                {
                    fileName = "default" + ".png";
                }
                else
                {
                    fileName = Path.GetFileName(path);
                }

                try
                {
                    //directory = Path.GetDirectoryName(path);
                    //fileName = Path.GetFileName(path);
                }
                catch (ArgumentException) { }
               

                string chosenFile = EditorUtility.SaveFilePanelInProject("Choose image file", fileName,
                        "png", "Please enter a file name to save the image to", directory);
                if (!string.IsNullOrEmpty(chosenFile))
                {
                    path = chosenFile;
                }
                //repaint editor because the file changed and we can't set it in the textfield retroactively
                //Repaint();
            }
        }
        return path;
    }

    public void BakeTexture()
    {
        if(hasPath && hasImage)
        {
            //render material to rendertexture
            RenderTexture renderTexture = RenderTexture.GetTemporary(Resolution.x, Resolution.y);
            Graphics.Blit(null, renderTexture, ImageMaterial);

            //transfer image from rendertexture to texture
            Texture2D texture = new Texture2D(Resolution.x, Resolution.y);
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(Vector2.zero, Resolution), 0, 0);

            //save texture to file
            byte[] png = texture.EncodeToPNG();
            File.WriteAllBytes(path, png);
            AssetDatabase.Refresh();

            //clean up variables
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
            DestroyImmediate(texture);
        }
        
    }
}