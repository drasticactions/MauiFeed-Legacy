// <copyright file="WinUIExtensions.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Windows.Storage.Streams;
using Windows.System;

namespace MauiFeed.WinUI.Tools
{
    /// <summary>
    /// WinUI Extensions.
    /// </summary>
    public static class WinUIExtensions
    {
        /// <summary>
        /// Small Icon.
        /// </summary>
        public const int ICONSMALL = 0;

        /// <summary>
        /// Big Icon.
        /// </summary>
        public const int ICONBIG = 1;

        /// <summary>
        /// Icon Small 2.
        /// </summary>
        public const int ICONSMALL2 = 2;

        /// <summary>
        /// Get Icon.
        /// </summary>
        public const int WMGETICON = 0x007F;

        /// <summary>
        /// Set Icon.
        /// </summary>
        public const int WMSETICON = 0x0080;

        /// <summary>
        /// Send Message to App.
        /// </summary>
        /// <param name="hWnd">Pointer.</param>
        /// <param name="msg">Message.</param>
        /// <param name="wParam">W Parameter.</param>
        /// <param name="lParam">L Parameter.</param>
        /// <returns>Int.</returns>
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);

        /// <summary>
        /// Create a random access stream from a byte array.
        /// </summary>
        /// <param name="array">The byte array.</param>
        /// <returns><see cref="IRandomAccessStream"/>.</returns>
        public static IRandomAccessStream ToRandomAccessStream(this byte[] array)
        {
            InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
            using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(array);
                writer.StoreAsync().GetResults();
            }

            return ms;
        }

        /// <summary>
        /// Set the Icon for this <see cref="Window"/> out from the current process, which is the same as the ApplicationIcon set in the *.csproj.
        /// </summary>
        /// <param name="window">Window.</param>
        public static void SetIconFromApplicationIcon(this Window window)
        {
            // https://learn.microsoft.com/en-us/answers/questions/822928/app-icon-windows-app-sdk.html
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            string sExe = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
            var ico = System.Drawing.Icon.ExtractAssociatedIcon(sExe);
            SendMessage(hWnd, WMSETICON, ICONBIG, ico!.Handle);
        }
    }
}