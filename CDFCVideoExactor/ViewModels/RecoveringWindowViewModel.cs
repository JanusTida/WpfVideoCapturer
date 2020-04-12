using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Interfaces;
using System;
using System.Diagnostics;

namespace CDFCVideoExactor.ViewModels {

    //恢复文件的提示窗体;
    public partial class RecoveringInfoWindowViewModel:ViewModelBase {
        public RecoveringInfoWindowViewModel(IRecoveringController iRecoveringController):base(0) {
            if(iRecoveringController == null) {
                EventLogger.Logger.WriteLine("RecoveringWindowViewModel初始化出错:参数iRecoveringController为空");
                throw new NullReferenceException("iRecoveringController can't be null!");
            }
            this.iRecoveringController = iRecoveringController;
        }

        private IRecoveringController iRecoveringController;
    }

    //恢复文件的提示窗体状态;
    public partial class RecoveringInfoWindowViewModel {
        //总共文件大小;
        private long totalFileSize;
        public long TotalFileSize {
            get {
                return totalFileSize;
            }
            set {
                totalFileSize = value;
                NotifyPropertyChanging(nameof(TotalFileSize));
            }
        }

        //已恢复文件大小;
        private long recoveredFileSize;
        public long RecoveredFileSize {
            get {
                return recoveredFileSize;
            }
            set {
                recoveredFileSize = value;
                NotifyPropertyChanging(nameof(RecoveredFileSize));
            }
        }

        //总共的文件数目;
        private int totalFileCount;
        public int TotalFileCount {
            get {
                return totalFileCount;
            }
            set {
                totalFileCount = value;
                NotifyPropertyChanging(nameof(TotalFileCount));
            }
        }

        //已恢复的文件数目;
        private int recoveredFileCount;
        public int RecoveredFileCount {
            get {
                return recoveredFileCount;
            }
            set {
                recoveredFileCount = value;
                NotifyPropertyChanging(nameof(RecoveredFileCount));
            }
        }

        //恢复的百分比(以文件大小为准);
        private byte percentage;
        public byte Percentage {
            get {
                return percentage;
            }
            set {
                percentage = value;
                NotifyPropertyChanging(nameof(Percentage));
            }
        }

        //当前是否处于恢复状态;
        public bool IsRecovering {
            get {
                return iRecoveringController.IsRecovering;
            }
            set {
                NotifyPropertyChanging(nameof(IsRecovering));
            }
        }

        //控制窗体的可用状态，用于关闭窗体;
        private bool isEnabled = true;
        public bool IsEnabled {
            get {
                return isEnabled;
            }
            set {
                isEnabled = value;
                NotifyPropertyChanging(nameof(IsEnabled));
            }
        }
    }

        /// <summary>
        /// 恢复文件的提示窗体命令项;
        /// </summary>
    public partial class RecoveringInfoWindowViewModel {
        #region 确定命令;
        private RelayCommand confirmCommand;
        public RelayCommand ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand = new RelayCommand(ConfirmExecuted));
            }
        }

        //确定命令执行动作;
        private void ConfirmExecuted() {
            IsEnabled = false;
        }
        #endregion

        #region 打开文件夹的命令
        private RelayCommand openFolderCommand;
        public RelayCommand OpenFolderCommand {
            get {
                return openFolderCommand ??
                    (openFolderCommand = new RelayCommand(OpenFolderExecuted));
            }
        }

        private void OpenFolderExecuted() {
            try { 
                Process.Start("explorer.exe", iRecoveringController.RecoveringPath);
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("RecoveringWindowViewModel->打开文件夹错误:" + ex.Message);
            }
        }
        #endregion
    }
}
