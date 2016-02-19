using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using AlphaLaunch.Core.Debug;

namespace AlphaLaunch.App
{
    public partial class MainWindow
    {
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchBarPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
            else if (e.Key == Key.Enter)
            {
                Hide();
                Model.OpenSelected();
            }
        }

        private void AnimateSearchItemsHeight()
        {
            var fromValue = Double.IsNaN(SearchItems.Height) ? 0 : SearchItems.Height;

            var toValue = 4 + SearchItems.Items.Count * 38;
            if (SearchItems.Items.Count == 0)
            {
                toValue = 0;
            }
            var doubleAnimation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(50)));
            SearchItems.BeginAnimation(HeightProperty, doubleAnimation);
        }

        private void SearchBarPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                Model.MainListModel.SelectedIndex = Math.Min(Model.MainListModel.Items.Count, Model.MainListModel.SelectedIndex + 1);
            }
            else if (e.Key == Key.Up)
            {
                Model.MainListModel.SelectedIndex = Math.Max(0, Model.MainListModel.SelectedIndex - 1);
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

            SearchItems.Height = 0;

            Activate();
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            SearchBar.SelectAll();
            SearchBar.Focus();
        }

        private async void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var source = e.OriginalSource as TextBox;
            await Model.UpdateSearchAsync(source?.Text ?? string.Empty, _cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            AnimateSearchItemsHeight();
        }
    }
}
