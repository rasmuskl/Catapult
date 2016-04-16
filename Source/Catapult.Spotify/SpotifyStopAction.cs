using Catapult.Core.Actions;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
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