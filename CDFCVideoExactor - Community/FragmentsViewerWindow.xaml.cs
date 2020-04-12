using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;

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
        }
        
        private void FragmentsViewerWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool isEnabled = false;
            if(bool.TryParse(e.NewValue.ToString(),out isEnabled) && !isEnabled) {
                this.Close();
            }
        }

       
    }
}
