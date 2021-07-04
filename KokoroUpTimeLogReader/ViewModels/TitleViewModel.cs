using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace KokoroUpTimeLogReader.ViewModels
{
    class TitleViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;


        public DelegateCommand StartCommand { get; private set; }

        public TitleViewModel(IRegionManager regionManager)
        {
            this._regionManager = regionManager;
            this.StartCommand = new DelegateCommand(Start);
        }

        private void Start()
        {
            _regionManager.RequestNavigate("ContentRegion",nameof(Views.ReplayView));
        }
    }
}
