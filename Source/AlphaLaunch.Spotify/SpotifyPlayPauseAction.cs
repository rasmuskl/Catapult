using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

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

        public object GetDetails()
        {
            return Name;
        }

        public IIconResolver GetIconResolver()
        {
            return new EmptyIconResolver();
        }
    }
}