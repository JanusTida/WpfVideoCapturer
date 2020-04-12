using System.Windows;
using System.Windows.Controls;

namespace CDFCVideoExactor.Controls {
    /// <summary>
    /// Interaction logic for ObjectHexViewer.xaml
    /// </summary>
    public partial class ObjectHexViewer : UserControl {
        public ObjectHexViewer() {
            InitializeComponent();
            
        }
        /// <summary>
        /// 记录左右选择是否处于变化状态中;
        /// </summary>
        private bool leftChanging = false;
        private bool rightChanging = false;

        /// <summary>
        /// 词块位置转为语句位置;
        /// </summary>
        /// <param name="stokenPlace"></param>
        /// <returns></returns>
        private int StokenPlaceToWordPlace(int stokenPlace) {
            int start = stokenPlace;
            int col = start - start / 97 * 97;
            int wordCol = (col + 1) / 3;
            int row = start / 97;
            return wordCol + row * 50;
        }
        private int WordPlaceToStokenPlace(int wordPlace) {
            int start = wordPlace;
            int col = start - start / 18 * 18;
            int stokenCol = col * 3;
            int row = start / 18;
            return stokenCol + row * 49;
        }
        private void stokenTxb_SelectionChanged(object sender, RoutedEventArgs e) {
            if (!rightChanging) {
                int start = StokenTxb.SelectionStart;
                int length = StokenTxb.SelectionLength;
                int end = start + length;

                int wordStart = StokenPlaceToWordPlace(start);
                int wordEnd = StokenPlaceToWordPlace(end);
                int wordLength = wordEnd - wordStart;

                TxbWords.Focus();
                leftChanging = true;
                TxbWords.SelectionStart = wordStart;
                TxbWords.SelectionLength = wordLength;
                leftChanging = false;
                StokenTxb.Focus();
            }
        }

        private void txbWords_LostFocus(object sender, RoutedEventArgs e) {
            e.Handled = true;
        }

        private void stokenTxb_LostFocus(object sender, RoutedEventArgs e) {
            e.Handled = true;
        }

        private void txbWords_SelectionChanged(object sender, RoutedEventArgs e) {
            if (!leftChanging) {
                int start = TxbWords.SelectionStart;
                int length = TxbWords.SelectionLength;
                int end = start + length;
                int stokenStart = WordPlaceToStokenPlace(start);
                int stokenEnd = WordPlaceToStokenPlace(end);
                int stokenLength = stokenEnd - stokenStart;
                StokenTxb.Focus();
                rightChanging = true;
                StokenTxb.SelectionStart = stokenStart;
                StokenTxb.SelectionLength = stokenLength;
                rightChanging = false;
                TxbWords.Focus();
            }
        }
    }
}
