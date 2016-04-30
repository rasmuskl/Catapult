using System;
using System.Diagnostics;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class WikipediaAction : IStandaloneAction, IAction<StringIndexable>
    {
        public void RunAction()
        {
            Process.Start("https://wikipedia.org/");
        }

        public void RunAction(StringIndexable stringIndexable)
        {
            Process.Start("https://wikipedia.org/wiki/Special:Search?search=" + Uri.EscapeDataString(stringIndexable.Name));
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