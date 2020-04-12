using CDFCVideoExactor.Abstracts;
using System;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Enums;
using CDFCUIContracts.Commands;

namespace CDFCVideoExactor.ViewModels {
    public partial class HomePageViewModel:ViewModelBase {
        public MainWindowViewModel MainWindowViewModel { get; set; }
        /// <summary>
        /// 构造方法;
        /// </summary>
        /// <param name="mainWindowViewModel">需传入的主窗体模型</param>
        public HomePageViewModel(MainWindowViewModel mainWindowViewModel):base(1) {
            if(mainWindowViewModel == null) {
                throw new Exception("HomePageViewModel构造错误,传入参数不得为null");
            }
            else {
                this.MainWindowViewModel = mainWindowViewModel;
            }
        }

        
    }

    /// <summary>
    /// 首页的命令绑定;
    /// </summary>
    public partial class HomePageViewModel {
        private DelegateCommand<EntranceType> stepIntoEntranceCommand;
        public DelegateCommand<EntranceType> StepIntoEntranceCommand {
            get {
                if(stepIntoEntranceCommand == null) {
                    stepIntoEntranceCommand = new DelegateCommand<EntranceType>(StepIntoEntranceExecuted);
                }
                return stepIntoEntranceCommand;
            }
        }

        
        /// <summary>
        /// 进入某个入口的命令;
        /// </summary>
        /// <param name="pageParameter">目标页面名称</param>
        private void StepIntoEntranceExecuted(EntranceType entranceParameter) {
            switch (entranceParameter) {
                case EntranceType.Capturer:
                    MainWindowViewModel.CurPageViewModel = MainWindowViewModel.VideoObjectSelectorPageViewModel;
                    break;
                case EntranceType.MultiMedia:
                    MainWindowViewModel.CurPageViewModel = MainWindowViewModel.VideoObjectSelectorPageViewModel;
                    break;
                case EntranceType.Mobile:
//                    MainWindowViewModel.CurPageViewModel = MainWindowViewModel.VideoObjectSelectorPageViewModel;
                    break;
                case EntranceType.PC:
                   // MainWindowViewModel.CurPageViewModel = MainWindowViewModel.VideoObjectSelectorPageViewModel;
                    break;
                default:
                    break;
            }

            MainWindowViewModel.SelectedEntranceType = entranceParameter;
        }
    }

    /// <summary>
    /// 首页的状态;
    /// </summary>
    public partial class HomePageViewModel {
        
    }
    
}
