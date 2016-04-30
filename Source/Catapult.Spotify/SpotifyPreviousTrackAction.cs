using Catapult.Core.Actions;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
{
    public class SpotifyPreviousTrackAction : IndexableBase, IStandaloneAction
    {
        public void RunAction()
        {
            new SpotifyClient().PreviousTrack();
        }

        public override string Name => "Spotify: Previous track";
    }
}