using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDFCVideoExactor.ViewModels {
    public partial class Mp4FragsViewerWindowViewModel:ViewModelBase {
        public string Title { get; set; }
        public string SetAsWord { get; set; }

        public ObservableCollection<CellFragment> CellFragments { get; set; } 

        public event EventHandler Closed;
        public event EventHandler FocusRequired;
        public event EventHandler<CellFragment> SelectedCellFragChanged;
        public event EventHandler CloseRequired;

        public void GetFocus() {
            FocusRequired?.Invoke(this, new EventArgs());
        }

        public void Close() {
            CloseRequired?.Invoke(this, new EventArgs());
        }

        private CellFragment selectedFrag;
        public CellFragment SelectedFrag {
            get {
                return selectedFrag;
            }
            set {
                SelectedCellFragChanged?.Invoke(this, value);
                selectedFrag = value;
            }
        }
    }

    public partial class Mp4FragsViewerWindowViewModel {
        /// <summary>
        /// 当窗体已关闭时的命令;
        /// </summary>
        private RelayCommand closingCommand;
        public RelayCommand ClosingCommand {
            get {
                return closingCommand ??
                    (closingCommand = new RelayCommand(
                        () => {
                            Closed?.Invoke(this, new EventArgs());
                        }
                    ));
            }
        }

        public event EventHandler<CellFragment> BorderedFragChanged;
        private RelayCommand setAsMP4BorderCommand;
        public RelayCommand SetAsMP4BorderCommand {
            get {
                return setAsMP4BorderCommand ??
                    (setAsMP4BorderCommand = new RelayCommand(() => {
                        if(SelectedFrag != null) {
                            BorderedFragChanged?.Invoke(this, SelectedFrag);
                        }
                    }));
            }
        }
    }
}
