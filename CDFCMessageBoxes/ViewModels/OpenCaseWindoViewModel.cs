using CDFCMessageBoxes.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CDFCMessageBoxes.ViewModels {
    public class OpenCaseWindoViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public OpenCaseWindoViewModel(IEnumerable<ListViewItemModel> items) {
            if(items != null && items.Count() != 0) {
                foreach (var p in items) {
                    Items.Add(p);
                }
            }
            else {
                
            }
        }
        public ObservableCollection<ListViewItemModel> Items { get; set; } = new ObservableCollection<ListViewItemModel>();

        private ListViewItemModel selectedItem;
        public ListViewItemModel SelectedItem {
            get {
                return selectedItem;
            }
            set {
                selectedItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedItem)));
            }
        }

        private bool isEnabled = true;
        public bool IsEnabled {
            get {
                return isEnabled;
            }
            set {
                isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        private RelayCommand confirmCommand;
        public RelayCommand ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand = new RelayCommand(
                        () => {
                            if (selectedItem != null) {
                                IsEnabled = false;
                            }
                        },
                        () => SelectedItem != null
                        )
                    );
            }
        }
    }
    
}
