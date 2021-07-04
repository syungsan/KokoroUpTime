using Prism.Mvvm;
using Prism.Regions;
using CsvReadWrite;

namespace KokoroUpTimeLogReader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";



        private readonly IRegionManager _regionManager;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string[] RecodedScenes { get; private set; }

        public MainWindowViewModel(IRegionManager regionManager)
        {


            this._regionManager = regionManager;
            this._regionManager.RegisterViewWithRegion("ContentRegion", typeof(KokoroUpTime.Chapter3));
        }
    }
}
