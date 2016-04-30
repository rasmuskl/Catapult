using System;
using System.Diagnostics;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class GoogleAction : IndexableBase, IStandaloneAction, IAction<StringIndexable>
    {
        public void RunAction()
        {
            Process.Start("https://www.google.com/");
        }

        public void RunAction(StringIndexable stringIndexable)
        {
            Process.Start("https://www.google.com/search?q=" + Uri.EscapeDataString(stringIndexable.Name));
        }

        public override string Name => "Google search";
    }
}