using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class File_Manager : MonoBehaviour
{
    public static File_Manager m_FileManager;
    public string FilePath;
    public List<string> ImagesName;

    
    // Start is called before the first frame update
    void Start()
    {
        m_FileManager = this;
        ImagesName = new List<string>();
        LoadImages();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadImages()
    {
        
        FilePath = Application.dataPath + "/Images";
        if (System.IO.Directory.Exists(FilePath))
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(FilePath);
            int c = 0;
            int i = 0;
            foreach (var item in di.GetFiles())
            {
                if (c % 2 == 0)
                {
                    ImagesName.Add(item.Name);
                    Debug.Log(ImagesName[i]);
                    i++;
                }
                c++;
            }
        }
    }

    public void BindImages()
    {

    }

}
