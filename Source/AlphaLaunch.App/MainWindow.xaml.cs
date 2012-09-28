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
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var dropbox = new DirectoryInfo(@"C:\Users\rasmuskl\Dropbox");

            var stopwatch = Stopwatch.StartNew();

            var fileItems = GetFiles(dropbox).ToArray();

            stopwatch.Stop();

            Status.Text += "Found " + fileItems.Count() + " items. [" + stopwatch.ElapsedMilliseconds + " ms]" + Environment.NewLine;
        }

        private IEnumerable<FileItem> GetFiles(DirectoryInfo directory)
        {
            return directory.GetFiles().Select(x => new FileItem(x.FullName))
                .Concat(directory.GetDirectories().SelectMany(GetFiles));
        }
    }
}
