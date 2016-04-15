using System;
using System.Diagnostics;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public class PathOfExileWikiAction : IStandaloneAction, IAction<string>
    {
        public void RunAction()
        {
            Process.Start("http://pathofexile.gamepedia.com/");
        }

        public void RunAction(string search)
        {
            Process.Start("http://pathofexile.gamepedia.com/index.php?search=" + Uri.EscapeUriString(search));
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