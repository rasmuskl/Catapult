using System;
using System.Linq;
using System.Collections.Generic;
using AlphaLaunch.Core.Actions;

namespace AlphaLaunch.Spotify
{
    public class SpotifyNextTrackAction : IStandaloneAction
    {
        public void RunAction()
        {
            var spotifyHandle = Win32.FindWindow("SpotifyMainWindow", null);

            if (spotifyHandle == (IntPtr)0)
            {
                return;
            }

            Win32.SendMessage(spotifyHandle, Win32.Constants.WmAppCommand, IntPtr.Zero, new IntPtr((long)SpotifyAction.NextTrack));
        }

        public string Name
        {
            get { return "Spotify: Next track"; }
        }

        public string BoostIdentifier
        {
            get { return typeof(SpotifyNextTrackAction).Name; }
        }
    }
}