using Catapult.Core.Actions;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
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