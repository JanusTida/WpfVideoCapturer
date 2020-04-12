using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Input;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for FragmentsViewerWindow.xaml
    /// </summary>
    public partial class FragmentsViewerWindow : MetroWindow {
        private FragmentsViewerWindowViewModel vm;

        public FragmentsViewerWindow(FragmentsViewerWindowViewModel vm) {
            InitializeComponent();
            this.vm = vm;
            this.DataContext = vm;
            vm.IsLoadingChanged += Vm_IsLoadingChanged;
        }

        private void Vm_IsLoadingChanged(object sender, bool e) {
            if (e) {
                this.Cursor = Cursors.Wait;
            }
            else {
                this.Cursor = null;
            }
        }

        private void FragmentsViewerWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool isEnabled = false;
            if(bool.TryParse(e.NewValue.ToString(),out isEnabled) && !isEnabled) {
                this.Close();
            }
        }
        
    }
}
