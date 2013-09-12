using System;
using System.Collections.Generic;
using System.Linq;
using AlphaLaunch.Core.Actions;
using AlphaLaunch.Core.Indexes;
using AlphaLaunch.Spotify;

namespace AlphaLaunch.App
{
    public class MainViewModel
    {
        private readonly ActionRegistry _actionRegistry;
        private readonly ListViewModel _listModel;

        public MainViewModel()
        {
            _actionRegistry = new ActionRegistry();

            _actionRegistry.RegisterAction<OpenAction>();
            _actionRegistry.RegisterAction<OpenAsAdminAction>();

            RegisterStandaloneAction<SpotifyNextTrackAction>();
            RegisterStandaloneAction<SpotifyPlayPauseAction>();
            RegisterStandaloneAction<SpotifyPreviousTrackAction>();
            RegisterStandaloneAction<SpotifyStopAction>();

            _listModel = new ListViewModel();
        }

        private void RegisterStandaloneAction<T>() where T : IStandaloneAction, new()
        {
            _actionRegistry.RegisterAction<T>();
            IndexStore.Instance.IndexAction(new T());
        }

        public ListViewModel ListModel
        {
            get { return _listModel; }
        }

        public void OpenSelected()
        {
            if (!_listModel.Items.Any())
            {
                return;
            }

            var searchItemModel = _listModel.Items[_listModel.SelectedIndex];

            var standaloneAction = searchItemModel.TargetItem as IStandaloneAction;
            if (standaloneAction != null)
            {
                standaloneAction.RunAction();
                IndexStore.Instance.AddBoost(_listModel.Search, searchItemModel.TargetItem.BoostIdentifier);
                return;
            }

            var actionList = _actionRegistry.GetActionFor(searchItemModel.TargetItem.GetType());

            var firstActionType = actionList.First();

            var actionInstance = Activator.CreateInstance(firstActionType);
            var runMethod = firstActionType.GetMethod("RunAction");
            runMethod.Invoke(actionInstance, new[] { searchItemModel.TargetItem });

            IndexStore.Instance.AddBoost(_listModel.Search, searchItemModel.TargetItem.BoostIdentifier);
        }
    }
}