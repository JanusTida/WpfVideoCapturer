using System.Windows.Controls;

namespace CDFCVideoExactor.Controls {
    /// <summary>
    /// Interaction logic for PrimaryObjectScanSettingPage.xaml
    /// </summary>
    public partial class PrimaryObjectScanSettingPage : UserControl {
        public PrimaryObjectScanSettingPage() {
            InitializeComponent();
        }

        private void numericTextBox_GetFocus(object sender, System.Windows.RoutedEventArgs e) {
            var numTxb = sender as TextBox;
            if(numTxb != null) {
                numTxb.SelectAll();
            }
        }
    }
}
