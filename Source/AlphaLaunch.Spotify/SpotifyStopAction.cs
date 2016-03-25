using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Spotify
{
    public class SpotifyStopAction : IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().Stop();
        }

        public string Name
        {
            get { return "Spotify: Stop"; }
        }

        public string BoostIdentifier
        {
            get { return typeof(SpotifyStopAction).Name; }
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