using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 碎片图表查看器的视图模型;
    /// </summary>
    public partial class FragmentsViewerWindowViewModel:ViewModelBase {
        /// <summary>
        /// 碎片查看器的构造方法;
        /// </summary>
        /// <param name="mainWindowViewModel">主窗体模型</param>
        public FragmentsViewerWindowViewModel(MainWindowViewModel mainWindowViewModel) {
            if (mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("FragmentsViewerWindowViewModel构造错误:参数不得为空");
                throw new ArgumentNullException("mainWindowViewModel");
            }
            this.mainWindowViewModel = mainWindowViewModel;
        }

        private MainWindowViewModel mainWindowViewModel;
        public ObservableCollection<CellFragment> CellFragments { get; set; }

        /// <summary>
        /// 选定的碎片行;
        /// </summary>
        private CellFragment selectedFragment;
        public CellFragment SelectedFragment {
            get {
                return selectedFragment;
            }
            set {
                selectedFragment = value;
                NotifyPropertyChanging(nameof(SelectedFragment));
            }
        }
    }

    
    /// <summary>
    /// 碎片图表查看器的视图状态;
    /// </summary>
    public partial class FragmentsViewerWindowViewModel {
        /// <summary>
        /// 通道号列是否可见;(当选择监控类型时);
        /// </summary>
        public bool IsChannelColVisible {
            get {
                if (mainWindowViewModel == null) {
                    EventLogger.Logger.WriteLine("VideoItemListViewerPage->IsChannelColVisible错误:参数为空!");
                    return true;
                }
                return mainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer;
            }
        }

        #region 控制窗体是否可用，以关闭窗体;
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
        #endregion
    }

    /// <summary>
    /// 碎片图表查看器的视图模型绑定项；
    /// </summary>
    public partial class FragmentsViewerWindowViewModel {
        #region 确定以退出的命令;
        private RelayCommand sureCommand;
        public RelayCommand SureCommand {
            get {
                return sureCommand ??
                    (sureCommand = new RelayCommand(SureExecuted));
            }
        }

        /// <summary>
        /// 确定命令所执行的动作;
        /// </summary>
        private void SureExecuted() {
            IsEnabled = false;
        }

        #endregion

        /// <summary>
        /// 恢复某碎片的命令；
        /// </summary>
        private RelayCommand recoverFragmentCommand;
        public RelayCommand RecoverFragmentCommand {
            get {
                return recoverFragmentCommand ??
                    (recoverFragmentCommand = new RelayCommand(
                        () => {
                            CDFCMessageBox.Show("欲使用此功能，请联系厂商!");
                        }
                    ));
            }
        }
    }
}
