using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for SeniorScanSettingWindow.xaml
    /// </summary>
    public partial class SeniorScanSettingWindow : MetroWindow {
        private SeniorObjectScanSettingViewModel vm;
        public SeniorScanSettingWindow() {
            InitializeComponent();
        }
        /// <summary>
        /// 高级扫描设定的构造方法;
        /// </summary>
        /// <param name="vm"></param>
        public SeniorScanSettingWindow(SeniorObjectScanSettingViewModel vm) {
            if(vm == null) {
                EventLogger.Logger.WriteLine("SeniorScanSettingWindow 构造出错:参数vm不得为空!");
            }
            InitializeComponent();
            this.DataContext = vm;
            this.vm = vm;
        }

        //控制窗体关闭时的动作;
        private void SeniorScanSettingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if(vm != null && vm.IsEnabled) {
                e.Cancel = true;
            }
        }

        //当窗体不可用时，关闭窗体;s
        private void SeniorScanSettingWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                if (!Convert.ToBoolean(e.NewValue)) {
                    this.Close();
                }
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("SeniorScanSettingWindow->SeniorScanSettingWindow_IsEnabledChanged出错" + ex.Message);
            }
        }
    }
}
