using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using System;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 对象选择的视图模型;
    /// </summary>
    public partial class VideoObjectSelectorPageViewModel : ViewModelBase {
        public VideoObjectSelectorPageViewModel(MainWindowViewModel mainWindowViewModel):base(2){
            if(mainWindowViewModel == null) {
                throw new ArgumentNullException("mainWindowViewModel");
            }
            this.MainWindowViewModel = mainWindowViewModel;
        }
        public readonly MainWindowViewModel MainWindowViewModel;
       
    }

    /// <summary>
    /// 对象选择视图的命令绑定;
    /// </summary>
    public partial class VideoObjectSelectorPageViewModel {
        /// <summary>
        /// 加载镜像命令;
        /// </summary>
        private RelayCommand loadImgFileCommand;
        public RelayCommand LoadImgFileCommand {
            get {
                return loadImgFileCommand??
                    (loadImgFileCommand = new RelayCommand(MainWindowViewModel.LoadImgFileExecuted));
            }
        }

        //public void LoadImgFileExecuted() {
        //    VistaOpenFileDialog dialog = new VistaOpenFileDialog();
        //    dialog.Filter = "镜像文件|*.img;*.E01";
        //    dialog.Title = "打开镜像文件";
        //    dialog.Multiselect = false;
        //    dialog.ValidateNames = true;
        //    dialog.ReadOnlyChecked = false;
        //    if (dialog.ShowDialog() == true && dialog.CheckFileExists) {
        //        MainWindowViewModel.IsLoading = true;

        //        //部署后台工作器;
        //        BackgroundWorker worker = new BackgroundWorker();
        //        //后台加载
        //        worker.DoWork += (sender, e) => {
        //            //获得镜像文件对象;
        //            ImgFile imgFile = ImgFile.GetImgFile(dialog.FileName);
        //            MainWindowViewModel.ComObject.ImgFiles.Add(imgFile);
        //            //重置设定对象;
        //            var objectScanSetting = new ObjectScanSetting(imgFile);
        //            MainWindowViewModel.PrimaryObjectScanSettingPageViewModel.
        //            ObjectScanSetting = objectScanSetting;
        //        };

        //        //加载完毕后跳转页面;
        //        worker.RunWorkerCompleted += (sender, e) => {
        //            MainWindowViewModel.CurPageViewModel = MainWindowViewModel.PrimaryObjectScanSettingPageViewModel;
        //            MainWindowViewModel.IsLoading = false;
        //        };

        //        worker.RunWorkerAsync();
        //    }
        //}
        /// <summary>
        /// 选择磁盘设备时的命令;
        /// </summary>
        private RelayCommand selectDiskDeviceCommand;
        public RelayCommand SelectDiskDeviceCommand {
            get {
                if (selectDiskDeviceCommand == null) {
                    selectDiskDeviceCommand = new RelayCommand(SelectDiskDeviceExecuted);
                }
                return selectDiskDeviceCommand;
            }
        }

        private void SelectDiskDeviceExecuted() {
            EventLogger.Logger.WriteLine("选择了磁盘系统");
            MainWindowViewModel.CurPageViewModel = MainWindowViewModel.DeviceSelectorPageViewModel;
        }
    }
}
