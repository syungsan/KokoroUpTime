using System;
using System.Linq;
using System.Windows;
using KokoroUpTime;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace KokoroUpTimeLogReader.ViewModels
{
    class ReplayViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public string CurrentScene { get; set; }

        public DelegateCommand BackCommand { get; private set; }

        public ReplayViewModel(IRegionManager regionManager)
        {
            this._regionManager = regionManager;
            this.BackCommand = new DelegateCommand(Back);
        }

        private void Back()
        {
            _regionManager.RequestNavigate("ContentRegion", nameof(Views.TitleView));
        }
    }
}
