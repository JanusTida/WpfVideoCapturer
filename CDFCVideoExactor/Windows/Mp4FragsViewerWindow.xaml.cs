using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CDFCVideoExactor.Windows {
    /// <summary>
    /// Interaction logic for Mp4FragsViewer.xaml
    /// </summary>
    public partial class Mp4FragsViewerWindow : MetroWindow {
        public Mp4FragsViewerWindow(Mp4FragsViewerWindowViewModel vm) {
            InitializeComponent();
            vm.FocusRequired += (sender, e) => {
                this.Focus();
            };
            vm.CloseRequired += (sender, e) => {
                this.Close();
            };

            this.DataContext = vm;
        }
    }
}
