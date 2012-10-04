using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlphaLaunch.App.Debug;

namespace AlphaLaunch.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SearchBar.Focus();

            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

            var debugWindow = new DebugWindow();

            debugWindow.Left = Left + Width + 20;
            debugWindow.Top = (SystemParameters.PrimaryScreenHeight - debugWindow.Height) / 2;

            debugWindow.Show();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SearchBarPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            else if (e.Key == Key.Down)
            {
                Model.SelectedIndex = Math.Min(Model.Items.Count, Model.SelectedIndex + 1);
            }
            else if (e.Key == Key.Up)
            {
                Model.SelectedIndex = Math.Max(0, Model.SelectedIndex - 1);
            }
            else if (e.Key == Key.Enter)
            {
                Model.OpenSelected();
            }
        }
    }
}
