using Catapult.Core.Actions;
using Catapult.Core.Indexes;

namespace Catapult.Spotify
{
    public class SpotifyStopAction : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            new SpotifyClient().Stop();
        }

        public override string Name => "Spotify: Stop";
    }
}