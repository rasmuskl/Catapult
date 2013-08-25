using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AlphaLaunch.App
{
    public partial class MainWindow
    {
        private readonly DebugWindow _debugWindow;

        public MainWindow()
        {
            InitializeComponent();
            _debugWindow = new DebugWindow();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            // TODO Add in debug?
            //Application.Current.Shutdown();
        }

        private void SearchBarPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
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
                Hide();
                Model.OpenSelected();
            }
        }

        private void WindowLoaded(object sender, EventArgs eventArgs)
        {
            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

            _debugWindow.Left = Left + Width + 20;
            _debugWindow.Top = (SystemParameters.PrimaryScreenHeight - _debugWindow.Height) / 2;
            _debugWindow.Show();

            Activate();
            SearchBar.SelectAll();
            SearchBar.Focus();
        }

        private void WindowDeactivated(object sender, EventArgs e)
        {
            _debugWindow.Hide();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
            {
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
    }
}
