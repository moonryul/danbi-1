using System.Collections;
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
                                                 out string resourceName,
                                                 bool isFileNameIncluded = true)
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
                    if (isFileNameIncluded)
                    {
                        // Last element.
                        _name = splittedLast[i];
                        sb.Append($"{_name}");
                    }
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

        static Texture2D ToTexture2D(RenderTexture rt, (int width, int height) resolution)
        {
            var tex = new Texture2D(resolution.width, resolution.height, TextureFormat.RGB24, false);
            var prevRenderTex = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prevRenderTex;
            return tex;
        }

        static void SaveRenderTexture(EDanbiImageType imgType, RenderTexture renderTexture, string fileSaveLocationAndName, string filePath, (int width, int height) resolution)
        {
            RenderTexture sRGBRT = RenderTexture.GetTemporary(renderTexture.width, renderTexture.height, 0, renderTexture.format, RenderTextureReadWrite.sRGB);
            Graphics.CopyTexture(renderTexture, sRGBRT);

            byte[] imgAsByteArr = default;
            var rtToTex2D = ToTexture2D(sRGBRT, resolution);
            switch (imgType)
            {
                case EDanbiImageType.png:
                    imgAsByteArr = rtToTex2D.EncodeToPNG();
                    break;

                case EDanbiImageType.jpg:
                    imgAsByteArr = rtToTex2D.EncodeToJPG();
                    break;
            }

            File.WriteAllBytes(fileSaveLocationAndName, imgAsByteArr);
            // Open the image after saving!
            System.Diagnostics.Process.Start(@"" + filePath);
            sRGBRT.Release();
        }

        public static bool SaveImage(EDanbiImageType imgType,
                                     RenderTexture renderTex,
                                     string fileSaveLocation,
                                     string filePath,
                                     (int width, int height) resolution)
        {
            SaveRenderTexture(imgType, renderTex, fileSaveLocation, filePath, resolution);
            Debug.Log($"File Saved! : {fileSaveLocation} and file save location is opened!");
            return true;
        }

        public static System.Diagnostics.Process OpenProcess()
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo();
            var process = new System.Diagnostics.Process();

            // start "cmd" process.
            processStartInfo.FileName = @"cmd";
            processStartInfo.CreateNoWindow = false;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardError = true;

            process.StartInfo = processStartInfo;
            process.Start();

            return process;
        }

        public static System.Diagnostics.Process OpenProcessWithArguments(string path, string arg)
        {
            var process = DanbiFileSys.OpenProcess();
            if (process is null)
            {
                Debug.LogError($"<color=red>Failed to open the process : {arg}!</color>");
                return default;
            }

            // process.StandardInput.Write(@"dir/w" + System.Environment.NewLine);
            process.StandardInput.Write(@"" + "cd " + path + System.Environment.NewLine);
            process.StandardInput.Write(@"" + arg + System.Environment.NewLine);
            process.StandardInput.Close();
            string res = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            process.Close();
            // Debug.Log(res);

            return process;
        }

        /// <summary>
        /// Each name is only used once.
        /// </summary>
        static string[] uniqueNames;
        static int _uniqueNameUsedIndex;
        static int uniqueNameUsedIndex
        {
            get => Mathf.Min(_uniqueNameUsedIndex, uniqueNames.Length);
            set => _uniqueNameUsedIndex = Mathf.Min(value, uniqueNames.Length);
        }

        static void populateUniqueNames()
        {
            int counts = 1024;
            uniqueNames = new string[counts];

            for (var i = 0; i < counts; ++i)
            {
                uniqueNames[i] = $"temp-{i}";
            }
        }

        public static string GetUniqueName()
        {
            // init unique names.
            if (uniqueNames is null)
            {
                populateUniqueNames();
            }
            return uniqueNames[uniqueNameUsedIndex++];
        }
    };
};
