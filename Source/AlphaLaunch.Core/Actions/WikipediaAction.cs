using System;
using System.Diagnostics;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public class WikipediaAction : IStandaloneAction, IAction<string>
    {
        public void RunAction()
        {
            Process.Start("https://wikipedia.org/");
        }

        public void RunAction(string search)
        {
            Process.Start("https://wikipedia.org/wiki/Special:Search?search=" + Uri.EscapeUriString(search));
        }

        public string Name => "Wikipedia search";
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