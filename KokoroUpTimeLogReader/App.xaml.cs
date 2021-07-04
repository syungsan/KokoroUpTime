using KokoroUpTimeLogReader.ViewModels;
using KokoroUpTimeLogReader.Views;
using Prism.Ioc;
using System.Windows;

namespace KokoroUpTimeLogReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<TitleView, TitleViewModel>();
            containerRegistry.RegisterForNavigation<ReplayView, ReplayViewModel>();
        }
    }
}
