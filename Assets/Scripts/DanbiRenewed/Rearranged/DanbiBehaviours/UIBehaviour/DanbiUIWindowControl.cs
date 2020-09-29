using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIWindowControl : MonoBehaviour
    {
        internal enum EDanbiWindowState
        {
            Fullscreen, Window, Minimized
        };
        EDanbiWindowState WindowState = EDanbiWindowState.Fullscreen;

        // [DllImport("user32.dll")]
        // private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        // [DllImport("user32.dll")]
        // private static extern IntPtr GetActiveWindow();

        void Start()
        {
            var fullscreenButton = transform.GetChild(0).GetComponent<Button>();
            fullscreenButton.onClick.AddListener(
                () =>
                {
                    WindowState = EDanbiWindowState.Fullscreen;
                    Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                }
            );

            var windowButton = transform.GetChild(1).GetComponent<Button>();
            windowButton.onClick.AddListener(
                () =>
                {
                    WindowState = EDanbiWindowState.Window;
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                }
            );

            var minimizeButton = transform.GetChild(2).GetComponent<Button>();
            minimizeButton.onClick.AddListener(
                () =>
                {
                    WindowState = EDanbiWindowState.Minimized;
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    // ShowWindow(GetActiveWindow(), 2);
                    // TODO: Clicking the minizing button leads to crash the program.
                }
            );

            var exitButton = transform.GetChild(2).GetComponent<Button>();
            exitButton.onClick.AddListener(
                () =>
                {
                    Application.Quit();
                }
            );
        }
    };
};
