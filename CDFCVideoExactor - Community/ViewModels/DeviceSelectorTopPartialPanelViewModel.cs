using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using System;
using System.ComponentModel;

namespace CDFCVideoExactor.ViewModels {
    public partial class DeviceSelectorTopPartialPanelViewModel:ViewModelBase {
        private readonly MainWindowViewModel mainWindowViewModel;
        public DeviceSelectorTopPartialPanelViewModel(MainWindowViewModel mainWindowViewModel) {
            if(mainWindowViewModel == null) {
                throw new NullReferenceException("DeviceSelectorTopPartialPanelViewModel构造出错:参数mainWindowViewModel为空");
            }
            this.mainWindowViewModel = mainWindowViewModel;
        }
    }

    /// <summary>
    /// 磁盘选择的分部视图模型命令;
    /// </summary>
    public partial class DeviceSelectorTopPartialPanelViewModel {
        #region 刷新设备的命令;
        private RelayCommand refreshDeviceCommand;
        public RelayCommand RefreshDeviceCommand {
            get {
                return refreshDeviceCommand ??
                    (refreshDeviceCommand = new RelayCommand(RefreshDeviceExecuted));
            }
        }

        private void RefreshDeviceExecuted() {
            mainWindowViewModel.IsLoading = true;
            //后台加载刷新动作;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, e) => {
                mainWindowViewModel.DeviceSelectorPageViewModel.ReloadDocument();
            };
            worker.RunWorkerCompleted += (sender, e) => {
                mainWindowViewModel.IsLoading = false;
            };
            worker.RunWorkerAsync();
        }

        #endregion
    }
}
