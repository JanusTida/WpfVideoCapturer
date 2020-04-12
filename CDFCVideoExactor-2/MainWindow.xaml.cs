using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Input;
using CDFCVideoExactor.ViewModels;
using CDFCMessageBoxes.MessageBoxes;
using CDFCVideoExactor.Enums;
using System.Windows.Controls;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        public MainWindowViewModel VM { get; private set; }
        
        public MainWindow() {
            IniModels();
            EventLogger.Logger.WriteLine("初始化模型执行正确!");
            InitializeComponent();
            EventLogger.Logger.WriteLine("初始化视图执行正确!");
            BindModel();
            EventLogger.Logger.WriteLine("命令绑定执行正确!");
            //VM.ComObject.Exit();
            //MessageBox.GetOne("构造函数2执行正确!");
        }

        /// <summary>
        /// 初始化主窗体视图模型;
        /// </summary>
        private void IniModels() {
            try {
                EventLogger.Logger.WriteLine("开始初始化模型!");
                VM = new MainWindowViewModel(this.Dispatcher);
                VM.SelectedEntranceType = EntranceType.CPAndMultiMedia;

            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("MainWindow初始化模型出错:" + ex.Message);
                CDFCMessageBox.Show("获取设备失败!", "错误", MessageBoxButton.OK);
                this.Close();
            }
        }
        /// <summary>
        /// 绑定视图控件的模型;s
        /// </summary>
        private void BindModel() {
            this.DataContext = VM;
            
        }

        

        private void MenuItemOpen(object sender, ExecutedRoutedEventArgs e) {
            switch (e.Parameter.ToString()) {
                case "File":
                    //miFile.IsSubmenuOpen = true;
                    break;
                case "Setting":
                    //miSetting.IsSubmenuOpen = true;
                    break;
                default:
                    break;
            }
        }

        //当窗体的可用状态发生变化时的处理;
        private void MainWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool res = true;
            if(bool.TryParse(e.NewValue.ToString(),out res) && !res) {
                this.Close();
            }
        }

        
    }
    
}
