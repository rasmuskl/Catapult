using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Serilog;

namespace Catapult.App;

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

        Height = 75 + SearchItems.Height;
    }

    private void SearchBarPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            HideAndClearWindow();
        }
        else if (e.Key == Key.Enter)
        {
            Model.AddIntent(new ExecuteIntent(SearchBar.Text, HideAndClearWindow));
        }
    }

    private void SearchBarPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var ctrlDown = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

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
        else if (e.Key == Key.Left && ctrlDown)
        {
            e.Handled = true;
            Model.AddIntent(new FastActionIntent(FastAction.Left));
        }
        else if (e.Key == Key.Right && ctrlDown)
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
        Log.Information("MainWindow: Loaded");

        RepositionToPrimaryScreen();

        SearchItems.Height = 0;

        Activate();
    }

    private void RepositionToPrimaryScreen()
    {
        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
    }

    private void MainWindow_OnActivated(object sender, EventArgs e)
    {
        Log.Information("MainWindow: Activated");

        RepositionToPrimaryScreen();

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

    private void MainWindow_OnDeactivated(object sender, EventArgs eventArgs)
    {
        Log.Information("MainWindow: Deactivated");
        HideAndClearWindow();
    }

    private void HideAndClearWindow()
    {
        if (Visibility != Visibility.Visible)
        {
            return;
        }

        Log.Information("MainWindow Hide and Clear");
        Model.AddIntent(new ClearIntent());
        Hide();
    }

    private void SearchItems_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var dependencyObject = e.OriginalSource as DependencyObject;

        if (dependencyObject == null)
        {
            return;
        }

        var listBox = sender as ListBox;

        if (listBox == null)
        {
            return;
        }

        var item = ItemsControl.ContainerFromElement(listBox, dependencyObject) as ListBoxItem;

        if (item == null)
        {
            return;
        }

        var itemIndex = listBox.Items.IndexOf(item.Content);
        Model.AddIntent(new MoveSelectionIntent(MoveDirection.SetIndex, itemIndex));
        Model.AddIntent(new ExecuteIntent(SearchBar.Text, HideAndClearWindow));
    }
}