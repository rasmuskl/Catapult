using System;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class CheckForUpdatesAction : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            UpdateCheckAction?.Invoke();
        }

        public override string Name => "Catapult: Check for updates";

        public static Action UpdateCheckAction;
    }
}