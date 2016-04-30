using Catapult.Core.Actions;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
{
    public class SpotifyPlayPauseAction : IndexableBase, IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().PlayPause();
        }

        public override string Name => "Spotify: Play / Pause";
    }
}