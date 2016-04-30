using System;
using System.Diagnostics;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class PathOfExileWikiAction : IStandaloneAction, IAction<StringIndexable>
    {
        public void RunAction()
        {
            Process.Start("http://pathofexile.gamepedia.com/");
        }

        public void RunAction(StringIndexable stringIndexable)
        {
            Process.Start("http://pathofexile.gamepedia.com/index.php?search=" + Uri.EscapeDataString(stringIndexable.Name));
        }

        public string Name => "Path of Exile wiki search";
        public string BoostIdentifier => Name;

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