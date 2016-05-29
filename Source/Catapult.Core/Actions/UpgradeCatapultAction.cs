using System;
using Catapult.Core.Indexes;

namespace Catapult.Core.Actions
{
    public class UpgradeCatapultAction : IndexableBase, IStandaloneAction
    {
        public void Run()
        {
            UpgradeAction?.Invoke();
        }

        public override string Name => "Catapult: Upgrade to new version (restart)";

        public static Action UpgradeAction;
    }
}