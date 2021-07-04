using Prism.Mvvm;

namespace KokoroUpTimeReader1.ViewModels
{
    class TitlePageViewModel : BindableBase
    {
        private string _title = "TitlePageView";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public TitlePageViewModel()
        {

        }
    }
}
