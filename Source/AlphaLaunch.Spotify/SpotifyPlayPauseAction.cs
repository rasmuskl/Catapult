using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.Core.Actions;

namespace AlphaLaunch.Spotify
{
    public class SpotifyPlayPauseAction : IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().PlayPause();
        }

        public string Name
        {
            get { return "Spotify: Play / Pause"; }
        }

        public string BoostIdentifier
        {
            get { return typeof(SpotifyPlayPauseAction).Name; }
        }
    }
}