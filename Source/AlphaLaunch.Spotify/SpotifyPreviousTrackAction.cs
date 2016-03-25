using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Spotify
{
    public class SpotifyPreviousTrackAction : IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().PreviousTrack();
        }

        public string Name
        {
            get { return "Spotify: Previous track"; }
        }

        public string BoostIdentifier
        {
            get { return typeof(SpotifyPreviousTrackAction).Name; }
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