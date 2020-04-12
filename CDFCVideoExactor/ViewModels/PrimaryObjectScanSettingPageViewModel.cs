using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Controllers;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.Models;
using System;
using System.ComponentModel;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.ViewModels {
    public partial class PrimaryObjectScanSettingPageViewModel:ViewModelBase {
        public readonly MainWindowViewModel MainWindowViewModel;
        public PrimaryObjectScanSettingPageViewModel(MainWindowViewModel mainWindowViewModel):base(4) {
            if(mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("PrimaryObjectScanSettingPageViewModel ->构造方法出错:mainWindowViewModel为空");
            }
            this.MainWindowViewModel = mainWindowViewModel;
        }

        public ObjectScanSetting ObjectScanSetting { get; set; }
    }

    

    /// <summary>
    /// 设定页的命令绑定;
    /// </summary>
    public partial class PrimaryObjectScanSettingPageViewModel {
        #region 确定执行的命令;
        private RelayCommand sureDoCommand;
        public RelayCommand SureDoCommand {
            get {
                return sureDoCommand ??
                    (sureDoCommand = new RelayCommand(SureDoExecuted,() => ObjectScanSetting.ScanEnabled));
            }
        }
        private void SureDoExecuted() {
            if (!ObjectScanSetting.ScanEnabled) {
                return;
            }
            MainWindowViewModel.IsLoading = true;

            //部署后台工作器;
            BackgroundWorker worker = new BackgroundWorker();
            bool res = false;
            IScanningController controller = null;

            worker.DoWork += (sender, e) => {
                CDFCSetting.ScanSetting.DeviceType = ObjectScanSetting.SelectedDeviceTypeEnum;
                CDFCSetting.ScanSetting.IsMP4Class = ObjectScanSetting.SelectedVersionType.IsMp4Class;
                CDFCSetting.ScanSetting.ExtensionName = ObjectScanSetting.ExtensionName;
                CDFCSetting.ScanSetting.SectorSize = ObjectScanSetting.SectorSize;
                CDFCSetting.ScanSetting.DeviceTypeInfo = ObjectScanSetting.SelectedDeviceType;
                CDFCSetting.ScanSetting.VersionType = ObjectScanSetting.SelectedVersionType;
                CDFCSetting.ScanSetting.ScanMethod = ObjectScanSetting.ScanMethod;
                CDFCSetting.ScanSetting.StartingTime = DateTime.Now.ToString().Replace('\\', '-').Replace(':', '-');
                controller = new ScanningController(MainWindowViewModel, ObjectScanSetting.IObjectDevice, ObjectScanSetting);
                res = controller.Init();
            };

            worker.RunWorkerCompleted += (sender, e) => {
                MainWindowViewModel.IsLoading = false;
                if (res) {
                    MainWindowViewModel.CurPageViewModel = MainWindowViewModel.VideoItemListViewerPageViewModel;
                    controller.Start();
                }
                else {
                    CDFCMessageBox.Show(FindResourceString("FailedToInit"));
                }
                //Thread.Sleep(10000);
            };

            worker.RunWorkerAsync();
        }
        #endregion 
    }


}
