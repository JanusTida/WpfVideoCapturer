using CDFCVideoRegister.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

namespace CDFCVideoRegister {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        private MainWindowViewModel vm;
        public MainWindow() {
            InitializeComponent();
            vm = new MainWindowViewModel(this.Dispatcher);
            this.DataContext = vm;
        }

        /// <summary>
        /// 当窗体可用状态变化时的处理;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindowViewModel_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool res = true;
            if (bool.TryParse(e.NewValue.ToString(), out res) && !res) {
                this.Close();
            }
        }
    }
}
