﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.Text;
using System.IO;

namespace Danbi
{
    // https://github.com/yasirkula/UnitySimpleFileBrowser            
    public static class DanbiFileSys
    {
        // static DanbiPersistantData<T>



        public static IEnumerator OpenLoadDialog(string startingPath,
                                                 IEnumerable<string> filters,
                                                 string title,
                                                 string buttonName,
                                                 bool folderMode = false)
        {
            FileBrowser.SetFilters(false, filters);
            yield return FileBrowser.WaitForLoadDialog(folderMode, false, startingPath, title, buttonName);

            if (!FileBrowser.Success)
            {
                // Debug.LogError($"<color=red>Failed to select the file from FileBrowser!</color>");
                yield break;
            }
        }

        public static IEnumerator OpenSaveDialog(string startingPath,
                                                 IEnumerable<string> filters,
                                                 string title,
                                                 string buttonName)
        {
            FileBrowser.SetFilters(false, filters);
            yield return FileBrowser.WaitForSaveDialog(false, false, startingPath, title, buttonName);

            if (!FileBrowser.Success)
            {
                // Debug.LogError($"<color=red>Failed to select the file from FileBrowser!</color>");
                yield break;
            }
        }

        public static void GetResourcePathIntact(out string intactPath,
                                                 out string resourceName)
        {
            if (!FileBrowser.Success)
            {
                intactPath = null;
                resourceName = null;
                return;
            }

            var res = FileBrowser.Result[0];
            if (string.IsNullOrEmpty(res))
            {
                intactPath = null;
                resourceName = null;
                return;
            }

            string[] splitted = res.Split('/');
            var sb = new StringBuilder();
            var _name = default(string);

            var target = splitted[splitted.Length - 1];
            string[] splittedLast = target.Split('\\');
            // Separate the \ character for getting a intact file path first.
            for (var i = 0; i < splittedLast.Length; ++i)
            {
                if (i != splittedLast.Length - 1)
                {
                    sb.Append($"{splittedLast[i]}/");
                }
                else
                {
                    // Last element.
                    _name = splittedLast[i];
                    sb.Append($"{_name}");
                }
            }
            // add the converted path to the last index of the splitted string.
            splitted[splitted.Length - 1] = sb.ToString();
            sb.Clear();

            // combine all the splitted strings into the one StringBuilder.
            for (var i = 0; i < splitted.Length; ++i)
            {
                if (i != splitted.Length - 1)
                {
                    sb.Append($"{splitted[i]}/");
                }
                else
                {
                    sb.Append(splitted[i]);
                }
            }

            intactPath = sb.ToString();
            resourceName = _name;
        }

        public static void GetResourcePathForResources(out string actualPath,
                                                       out string resourceName)
        {
            if (!FileBrowser.Success)
            {
                actualPath = null;
                resourceName = null;
                return;
            }

            var res = FileBrowser.Result[0];
            if (string.IsNullOrEmpty(res))
            {
                actualPath = null;
                resourceName = null;
                return;
            }

            string[] splitted = res.Split('\\');
            StringBuilder sb = new StringBuilder();
            var _name = default(string);

            for (int i = 0; i < splitted.Length; ++i)
            {
                if (splitted[i] != "Resources") continue;

                for (int j = i + 1; j < splitted.Length; ++j)
                {
                    if (j != splitted.Length - 1)
                    {
                        sb.Append(splitted[j] + '/');
                    }
                    else
                    {
                        var str = splitted[j].Split('.');
                        sb.Append(str[0]);
                        _name = str[0];
                    }
                }
                break;
            }
            actualPath = sb.ToString();
            resourceName = _name ?? default;
        }

        static Texture2D LoadImage(string filePath, (int width, int height) resolution)
        {
            Texture2D tex = default;
            byte[] fileDat;

            if (File.Exists(filePath))
            {
                fileDat = File.ReadAllBytes(filePath);
                tex = new Texture2D(resolution.width, resolution.height);
                tex.LoadImage(fileDat, false);
            }
            return tex;
        }

        static Texture2D ToTexture2D(RenderTexture renderTex, (int width, int height) resolution)
        {
            if (resolution.width != renderTex.width) 
            {
                renderTex.width = resolution.width;
            }

            if (resolution.height != renderTex.height)
            {
                renderTex.height = resolution.height;
            }

            var tex = new Texture2D(resolution.width, resolution.height, TextureFormat.RGB24, false);
            var prevRenderTex = RenderTexture.active;
            tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prevRenderTex;
            return tex;
        }

        static void SaveRenderTexture(RenderTexture renderTexture, string filePath, (int width, int height) resolution)
        {
            byte[] bytes = ToTexture2D(renderTexture, resolution).EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        public static bool SaveImage(ref EDanbiSimulatorMode simulatorMode,
                                     EDanbiImageType imageType,
                                     RenderTexture renderTex,
                                     string filePath,
                                     string name,
                                     (int width, int height) resolution)
        {
            string ext = default;
            switch (imageType)
            {
                case EDanbiImageType.jpg:
                    ext = ".jpg";
                    break;

                case EDanbiImageType.png:
                    ext = ".png";
                    break;
            }
            switch (simulatorMode)
            {
                case EDanbiSimulatorMode.CAPTURE:
                    var finalName = $"{filePath}/{name}{ext}";
                    SaveRenderTexture(renderTex, finalName, resolution);
                    Debug.Log($"File Saved! : {finalName}");
                    return true;                    
            }
            return false;
        }
    };
};
