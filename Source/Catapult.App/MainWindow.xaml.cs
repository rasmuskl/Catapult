using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Catapult.Core.Indexes;

namespace Catapult.App
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Model.MainListModel.Items.CollectionChanged += Items_CollectionChanged;
            Model.StackPushed += Model_StackPushed;
        }

        private void Model_StackPushed()
        {
            SearchBar.Text = string.Empty;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SearchItems.Items.Count == 0)
            {
                SearchItems.Height = 0;
            }
            else
            {
                SearchItems.Height = 3 + Math.Min(10, SearchItems.Items.Count) * 50;
            }

            Height = 300 + SearchItems.Height;
        }

        private void SearchBarPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Model.AddIntent(new ClearIntent());
                Hide();
            }
            else if (e.Key == Key.Enter)
            {
                Model.AddIntent(new ExecuteIntent(SearchBar.Text));
                Hide();
            }
        }

        private void SearchBarPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                Model.AddIntent(new MoveSelectionIntent(MoveDirection.Down));
            }
            if (e.Key == Key.PageDown)
            {
                e.Handled = true;
                Model.AddIntent(new MoveSelectionIntent(MoveDirection.Down, 10));
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                Model.AddIntent(new MoveSelectionIntent(MoveDirection.Up));
            }
            else if (e.Key == Key.PageUp)
            {
                e.Handled = true;
                Model.AddIntent(new MoveSelectionIntent(MoveDirection.Up, 10));
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                Model.AddIntent(new FastActionIntent(FastAction.Left));
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                Model.AddIntent(new FastActionIntent(FastAction.Right));
            }
            else if (e.Key == Key.Tab)
            {
                e.Handled = true;
                Model.AddIntent(new PushStackIntent(SearchBar.Text));
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

        private void SearchBar_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var source = e.OriginalSource as TextBox;
            Model.AddIntent(new SearchIntent(source?.Text ?? string.Empty));
        }

        private void SearchItems_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchItems.ScrollIntoView(SearchItems.SelectedItem);
        }
    }
}
