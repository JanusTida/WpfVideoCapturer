using CDFCVideoExactor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CDFCVideoExactor.Controls {
    /// <summary>
    /// Interaction logic for DeviceSelectorPage.xaml
    /// </summary>
    public partial class DeviceSelectorPage : UserControl {
        public DeviceSelectorPage() {
            InitializeComponent();
            var vm = this.DataContext as DeviceSelectorPageViewModel;
            if (vm != null) {
                vm.RegetAct = () =>  DeviceSelectorPage_Loaded(null, null);
            }
        }

        private void DeviceSelectorPage_Loaded(object sender, RoutedEventArgs e) {
            var xmlDataProvider = this.TryFindResource("Xdp") as XmlDataProvider;
            var vm = this.DataContext as DeviceSelectorPageViewModel;
            if(xmlDataProvider != null && vm != null) {
                xmlDataProvider.Document = vm.DeviceDoc;
            }
        }

    }
}
