using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlphaLaunch.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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
