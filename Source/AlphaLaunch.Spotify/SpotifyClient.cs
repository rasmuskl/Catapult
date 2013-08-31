using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaLaunch.Spotify
{
    internal class SpotifyClient
    {
        public void NextTrack()
        {
            AttemptSend(SpotifyAction.NextTrack);
        }        
        
        public void PreviousTrack()
        {
            AttemptSend(SpotifyAction.PreviousTrack);
        }        
        
        public void Stop()
        {
            AttemptSend(SpotifyAction.Stop);
        }        
        
        public void PlayPause()
        {
            AttemptSend(SpotifyAction.PlayPause);
        }

        private static void AttemptSend(SpotifyAction spotifyAction)
        {
            var spotifyHandle = Win32.FindWindow("SpotifyMainWindow", null);

            if (spotifyHandle == (IntPtr) 0)
            {
                return;
            }

            Win32.SendMessage(spotifyHandle, Win32.Constants.WmAppCommand, IntPtr.Zero, new IntPtr((long) spotifyAction));
        }

        private enum SpotifyAction : long
        {
            PlayPause = 0xe0000,
            Mute = 0x80000,
            VolumeDown = 0x90000,
            VolumeUp = 0xa0000,
            Stop = 0xd0000,
            PreviousTrack = 0xc0000,
            NextTrack = 0xb0000
        }

        private static class Win32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            internal static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


            [DllImport("user32.dll", SetLastError = true)]
            internal static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

            internal class Constants
            {
                internal const uint WmAppCommand = 0x0319;
            }
        }
    }
}