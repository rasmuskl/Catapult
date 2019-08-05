using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Catapult.AvaloniaApp.ViewModels;
using ReactiveUI;

namespace Catapult.AvaloniaApp.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public ListBox SearchBox => this.FindControl<ListBox>("SearchBox");
        public TextBox SearchTextBox => this.FindControl<TextBox>("SearchTextBox");

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            HasSystemDecorations = false;
            Activated += MainWindow_Activated;

            this.WhenActivated(disposables =>
            {
                this.WhenAnyValue(x => x.ViewModel.SearchResults)
                    .Throttle(TimeSpan.FromMilliseconds(1))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        var textBoxHeight = SearchTextBox.Bounds.Height;
                        var searchBoxChildHeight = SearchBox.GetLogicalChildren().OfType<ListBoxItem>().FirstOrDefault()?.Bounds.Height ?? 0;
                        Height = textBoxHeight + Math.Min(searchBoxChildHeight * 8, SearchBox.Bounds.Height);
                    })
                    .DisposeWith(disposables);
            });

            SocketActivation.Activator = () =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Activate();
                });
            };
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideAndClearWindow();
            }
            else if (e.Key == Key.Enter)
            {
                ViewModel.AddIntent(new ExecuteIntent(SearchTextBox.Text, HideAndClearWindow));
            }
        }

        private void HideAndClearWindow()
        {
            if (!IsVisible)
            {
                return;
            }

            ViewModel.AddIntent(new ClearIntent());
            Hide();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //var ctrlDown = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

            if (e.Key == Key.Down)
            {
                e.Handled = true;
                ViewModel.AddIntent(new MoveSelectionIntent(MoveDirection.Down));
            }
            if (e.Key == Key.PageDown)
            {
                e.Handled = true;
                ViewModel.AddIntent(new MoveSelectionIntent(MoveDirection.Down, 10));
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                ViewModel.AddIntent(new MoveSelectionIntent(MoveDirection.Up));
            }
            else if (e.Key == Key.PageUp)
            {
                e.Handled = true;
                ViewModel.AddIntent(new MoveSelectionIntent(MoveDirection.Up, 10));
            }
            //else if (e.Key == Key.Left && ctrlDown)
            //{
            //    e.Handled = true;
            //    ViewModel.AddIntent(new FastActionIntent(FastAction.Left));
            //}
            //else if (e.Key == Key.Right && ctrlDown)
            //{
            //    e.Handled = true;
            //    ViewModel.AddIntent(new FastActionIntent(FastAction.Right));
            //}
            else if (e.Key == Key.Tab)
            {
                e.Handled = true;
                ViewModel.AddIntent(new PushStackIntent(SearchTextBox.Text));
            }
        }

        private void MainWindow_Activated(object sender, System.EventArgs e)
        {
            SearchTextBox.Focus();
        }
    }
}
