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
            new SpotifyClient().NextTrack();
        }

        public string Name
        {
            get { return "Spotify: Next track"; }
        }

        public string BoostIdentifier
        {
            get { return typeof(SpotifyNextTrackAction).Name; }
        }

        public object GetDetails()
        {
            return Name;
        }
    }
}