using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for ScanningInfoDialogWindow.xaml
    /// </summary>
    public partial class ScanningInfoDialogWindow : MetroWindow {
        private ScanningInfoDialogWindowViewModel vm;

        public ScanningInfoDialogWindow() {
            InitializeComponent();
        }
        /// <summary>
        /// 扫描对话框的构造方法;
        /// </summary>
        /// <param name="scanningInfoDialongWindowViewModel"></param>
        public ScanningInfoDialogWindow(ScanningInfoDialogWindowViewModel scanningInfoDialongWindowViewModel) {
            if(scanningInfoDialongWindowViewModel == null) {
                EventLogger.Logger.WriteLine("ScanningInfoDialogWindow构造出错:scanningInfoDialongWindowView不得为空!");
                throw new NullReferenceException("scanningInfoDialongWindowView Can't be null");
            }
            vm = scanningInfoDialongWindowViewModel;
            this.DataContext = vm;
            InitializeComponent();
        }

        /// <summary>
        /// 处理窗体关闭时的动作,防止在未完成时关闭;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanningInfoDialogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (vm != null && vm.IsScanning) {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 处理窗体可用的状态，用于关闭本窗体;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScanningInfoDialogWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool val;
            if(Boolean.TryParse(e.NewValue.ToString(),out val)){
                if(val == false) {
                    this.Close();
                }
            }
        }

    }
}
