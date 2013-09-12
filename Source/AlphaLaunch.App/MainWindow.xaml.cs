using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace AlphaLaunch.App
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SearchBarPreviewKeyUp(object sender, KeyEventArgs e)
        {
            AnimateSearchItemsHeight();

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

            var toValue = 4 + SearchItems.Items.Count*38;
            if (SearchItems.Items.Count == 0)
            {
                toValue = 0;
            }
            var doubleAnimation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(50)));
            SearchItems.BeginAnimation(HeightProperty, doubleAnimation);
        }

        private void SearchBar_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                Model.ListModel.SelectedIndex = Math.Min(Model.ListModel.Items.Count, Model.ListModel.SelectedIndex + 1);
            }
            else if (e.Key == Key.Up)
            {
                Model.ListModel.SelectedIndex = Math.Max(0, Model.ListModel.SelectedIndex - 1);
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
    }
}
