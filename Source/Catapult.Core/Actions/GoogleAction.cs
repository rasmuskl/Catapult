using System;
using System.Diagnostics;
using Catapult.Core.Icons;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class GoogleAction : IStandaloneAction, IAction<StringIndexable>
    {
        public void RunAction()
        {
            Process.Start("https://www.google.com/");
        }

        public void RunAction(StringIndexable stringIndexable)
        {
            Process.Start("https://www.google.com/search?q=" + Uri.EscapeDataString(stringIndexable.Name));
        }

        public string Name => "Google search";
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