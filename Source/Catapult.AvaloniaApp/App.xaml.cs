using Avalonia.Markup.Xaml;
using Application = Avalonia.Application;

namespace Catapult.AvaloniaApp
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
