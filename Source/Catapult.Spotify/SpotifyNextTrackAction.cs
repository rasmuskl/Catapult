using Catapult.Core.Actions;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
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

        public IIconResolver GetIconResolver()
        {
            return new EmptyIconResolver();
        }
    }
}