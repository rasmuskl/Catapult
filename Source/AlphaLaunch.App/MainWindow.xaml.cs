using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

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

        }


        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            int count = Model.IndexDropbox();

            stopwatch.Stop();

            Status.Text += "Found " + count + " items. [" + stopwatch.ElapsedMilliseconds + " ms]" + Environment.NewLine;

        }
    }
}
