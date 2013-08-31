using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.Core.Actions;

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
    }
}