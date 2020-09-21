using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.Text;

namespace Danbi
{
    // https://github.com/yasirkula/UnitySimpleFileBrowser            
    public static class DanbiFileBrowser
    {
        public static IEnumerator OpenLoadDialog(string startingPath,
                                                 IEnumerable<string> filters,
                                                 string title,
                                                 string buttonName)
        {
            FileBrowser.SetFilters(false, filters);
            yield return FileBrowser.WaitForLoadDialog(false, false, startingPath, title, buttonName);

            if (!FileBrowser.Success)
            {
                Debug.LogError($"<color=red>Failed to select the file from FileBrowser!</color>");
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
                Debug.LogError($"<color=red>Failed to select the file from FileBrowser!</color>");
                yield break;
            }
        }

        public static void getActualResourcePath(out string actualPath,
                                                 out string resourceName)
        {
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
    };
};
