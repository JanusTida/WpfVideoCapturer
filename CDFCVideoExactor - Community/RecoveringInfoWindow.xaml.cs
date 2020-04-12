using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for RecoveringInfoWindow.xaml
    /// </summary>
    public partial class RecoveringInfoWindow : MetroWindow {
        private RecoveringInfoWindowViewModel vm;
        public RecoveringInfoWindow() {
            InitializeComponent();
        }
        public RecoveringInfoWindow(RecoveringInfoWindowViewModel vm) {
            if (vm == null) {
                EventLogger.Logger.WriteLine("ScanningInfoDialogWindow构造出错:scanningInfoDialongWindowView不得为空!");
                throw new NullReferenceException("scanningInfoDialongWindowView Can't be null");
            }
            this.vm = vm;
            InitializeComponent();
            this.DataContext = vm;
        }
        
        //窗体关闭时的处理动作;
        private void RecoveringInfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if(vm != null && (vm.IsRecovering || vm.IsEnabled)) {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 处理窗体可用的状态，用于关闭本窗体;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecoveringInfoDialogWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool val;
            if (Boolean.TryParse(e.NewValue.ToString(), out val)) {
                if (val == false) {
                    this.Close();
                }
            }
        }
    }
}
