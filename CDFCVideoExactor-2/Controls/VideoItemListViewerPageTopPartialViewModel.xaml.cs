using System.Windows.Controls;

namespace CDFCVideoExactor.Controls {
    /// <summary>
    /// Interaction logic for VideoItemListViewerTopPagePartialViewModel.xaml
    /// </summary>
    public partial class VideoItemListViewerPageTopPartialViewModel : UserControl {
        public VideoItemListViewerPageTopPartialViewModel() {
            InitializeComponent();
        }

        private void numericTxb_GotFocus(object sender, System.Windows.RoutedEventArgs e) {
            var txb = e.OriginalSource as TextBox;
            if(txb != null) {
                txb.SelectAll();
            }
        }
    }
}
