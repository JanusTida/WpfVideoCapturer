using System.ComponentModel;

namespace CDFCMessageBoxes.Models {
    public partial class ListViewItemModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public partial class ListViewItemModel : INotifyPropertyChanged {
        private string titleWord;
        public string TitleWord {
            get {
                return titleWord;
            }
            set {
                titleWord = value;
                NotifyPropertyChanged(nameof(TitleWord));
            }
        }

        private bool isSelected;
        public bool IsSelected {
            get {
                return isSelected;
            }
            set {
                isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected));
            }
        }
    }
}
