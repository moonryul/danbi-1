//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using System;

//public class BakeTextureWindow : EditorWindow
//{

//    Material ImageMaterial;
//    string FilePath = "";//"Assets/.png";
//    Vector2Int Resolution;

//    bool hasMaterial;
//    bool hasResolution;
//    bool hasPath;
//    [MenuItem("Tools/Bake material to texture")]
//    static void OpenWindow()
//    {
//        //create window
//        BakeTextureWindow window = EditorWindow.GetWindow<BakeTextureWindow>();
//        window.Show();

//        window.CheckInput();
//    }

//    void OnGUI()
//    {
//        EditorGUILayout.HelpBox("Set the material" +
//            ",Size, Path then press the \"Bake\" button.", MessageType.None);

        

//        using (var check = new EditorGUI.ChangeCheckScope())
//        {
//            ImageMaterial = (Material)EditorGUILayout.ObjectField("Material", ImageMaterial, typeof(Material), false);
//            Resolution = EditorGUILayout.Vector2IntField("Image Resolution", Resolution);
//            FilePath = FileField(FilePath);

//            if (check.changed)
//            {
//                CheckInput();
//            }
//        }

//        GUI.enabled = hasMaterial && hasResolution;
//        if (GUILayout.Button("Bake"))
//        {
//            BakeTexture();
//        }
//        GUI.enabled = true;

//        //tell the user what inputs are missing
//        if (!hasMaterial)
//        {
//            EditorGUILayout.HelpBox("You're still missing a material to bake.", MessageType.Error);
//        }
//        if (!hasResolution)
//        {
//            EditorGUILayout.HelpBox("Please set a size bigger than zero.", MessageType.Error);
//        }
//        if(!hasPath)
//        {
//            EditorGUILayout.HelpBox("Can't find any file path.", MessageType.Error);
//        }
       
//    }

//    void CheckInput()
//    {
//        //check which values are entered already
//        hasMaterial = ImageMaterial != null;
//        hasResolution = Resolution.x > 0 && Resolution.y > 0;
//        hasPath = FilePath != "";
//        try
//        {
//            string ext = Path.GetExtension(FilePath);
//        }
//        catch (ArgumentException) { }
//    }

//    string FileField(string path)
//    {
//        //allow the user to enter output file both as text or via file browser
//        EditorGUILayout.LabelField("FileName");
//        using (new GUILayout.HorizontalScope())
//        {
           
//            if (GUILayout.Button("Set Path"))
//            {
//                //set default values for directory, then try to override them with values of existing path
//                string directory = "Assets";
//                string fileName;
//                if (ImageMaterial != null)
//                    fileName = ImageMaterial.name + ".png";
//                else if(Path.GetFileName(path)== "")
//                {
//                    fileName = "default" + ".png";
//                }
//                else
//                {
//                    fileName = Path.GetFileName(path);
//                }
                  
//                try
//                {
//                    //directory = Path.GetDirectoryName(path);
//                    //fileName = Path.GetFileName(path);
//                }
//                catch (ArgumentException) { }
//                EditorGUILayout.TextArea(fileName);

//                string chosenFile = EditorUtility.SaveFilePanelInProject("Choose image file", fileName,
//                        "png", "Please enter a file name to save the image to", directory);
//                if (!string.IsNullOrEmpty(chosenFile))
//                {
//                    path = chosenFile;
//                }
//                //repaint editor because the file changed and we can't set it in the textfield retroactively
//                Repaint();
//            }
//        }
//        return path;
//    }

//    void BakeTexture()
//    {
//        //render material to rendertexture
//        RenderTexture renderTexture = RenderTexture.GetTemporary(Resolution.x, Resolution.y);
//        Graphics.Blit(null, renderTexture, ImageMaterial);

//        //transfer image from rendertexture to texture
//        Texture2D texture = new Texture2D(Resolution.x, Resolution.y);
//        RenderTexture.active = renderTexture;
//        texture.ReadPixels(new Rect(Vector2.zero, Resolution), 0, 0);

//        //save texture to file
//        byte[] png = texture.EncodeToPNG();
//        File.WriteAllBytes(FilePath, png);
//        AssetDatabase.Refresh();

//        //clean up variables
//        RenderTexture.active = null;
//        RenderTexture.ReleaseTemporary(renderTexture);
//        DestroyImmediate(texture);
//    }
//}