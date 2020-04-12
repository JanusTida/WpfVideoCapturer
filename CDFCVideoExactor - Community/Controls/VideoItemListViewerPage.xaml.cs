using CDFCVideoExactor.ViewModels;
using System.Windows.Controls;
using System.Linq;
using System.Threading;

namespace CDFCVideoExactor.Controls {
    /// <summary>
    /// Interaction logic for VideoItemtViewerPage.xaml
    /// </summary>
    public partial class VideoItemListViewerPage : UserControl {
        public VideoItemListViewerPage() {
            InitializeComponent();
        }

        private VideoItemListViewerPageViewModel vm;
        private VideoItemListViewerPageViewModel VM {
            get {
                return vm ??
                    (vm = this.DataContext as VideoItemListViewerPageViewModel);
            }
        }

        private void dgVolCur_Sorting(object sender, DataGridSortingEventArgs e) {
            var emptyRows = VM.CurRows.Where(p => p.IsEmpty).ToList();

            ThreadPool.QueueUserWorkItem(callBack => {
                var selectedRows = VM.CurRows.Where(p => p.IsSelected).ToList();
                foreach (var p in selectedRows) {
                    p.IsSelected = false;
                }
            });
            foreach (var p in emptyRows) {
                VM.CurRows.Remove(p);
            }

            if (VM != null) {
                var vm = VM;
                switch (e.Column.SortMemberPath) {
                    case "ChannelNO":
                        //vm.EmptyRows.ForEach(p => p.ChannelNO = -1);
                        break;
                }
            }
        }
    }
}
