using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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

            Left = (SystemParameters.PrimaryScreenWidth - Width)/2;
            Top = (SystemParameters.PrimaryScreenHeight - Height)/2;

            var debugWindow = new DebugWindow();

            debugWindow.Left = Left + Width + 20;
            debugWindow.Top = (SystemParameters.PrimaryScreenHeight - debugWindow.Height) / 2;
            
            debugWindow.Show();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            int count = Model.IndexDropbox();

            stopwatch.Stop();

            //Status.Text += "Found " + count + " items. [" + stopwatch.ElapsedMilliseconds + " ms]" + Environment.NewLine;
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
        }
    }
}
