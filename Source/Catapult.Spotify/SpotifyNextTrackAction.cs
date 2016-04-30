using Catapult.Core.Actions;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
{
    public class SpotifyNextTrackAction : IndexableBase, IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().NextTrack();
        }

        public override string Name => "Spotify: Next track";
    }
}