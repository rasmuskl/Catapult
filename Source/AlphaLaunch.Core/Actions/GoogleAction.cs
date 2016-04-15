using System.Diagnostics;
using AlphaLaunch.Core.Icons;
using AlphaLaunch.Core.Indexes;

namespace AlphaLaunch.Core.Actions
{
    public class GoogleAction : IStandaloneAction
    {
        public void RunAction()
        {
            Process.Start("https://www.google.com/");
        }

        public string Name => "Google Search";
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

    public class GoogleSearchAction : IAction<string>
    {
        public void RunAction(string search)
        {
            Process.Start("https://www.google.com/search?q=" + search);
        }

        public string Name => "Google Search";
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