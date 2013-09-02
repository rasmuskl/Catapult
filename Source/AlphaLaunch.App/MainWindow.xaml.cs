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
        //private readonly DebugWindow _debugWindow;

        public MainWindow()
        {
            InitializeComponent();
            //_debugWindow = new DebugWindow();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            // TODO Add in debug?
            //Application.Current.Shutdown();
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
                Model.SelectedIndex = Math.Min(Model.Items.Count, Model.SelectedIndex + 1);
            }
            else if (e.Key == Key.Up)
            {
                Model.SelectedIndex = Math.Max(0, Model.SelectedIndex - 1);
            }
        }

        private void WindowActivated(object sender, EventArgs eventArgs)
        {
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

            SearchItems.Height = 0;

            Activate();
            SearchBar.SelectAll();
            SearchBar.Focus();

            //_debugWindow.Left = Left + Width + 20;
            //_debugWindow.Top = (SystemParameters.PrimaryScreenHeight - _debugWindow.Height) / 2;
            //_debugWindow.Show();
        }

        private void WindowDeactivated(object sender, EventArgs e)
        {
            //_debugWindow.Hide();
        }
    }
}
