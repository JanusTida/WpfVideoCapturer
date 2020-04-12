using CDFCVideoExactorUpdater.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CDFCVideoExactorUpdater {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Shell : MetroWindow {
        private ShellViewModel vm;
        public Shell() {
            InitializeComponent();
            vm = new ShellViewModel();
            vm.CloseRequired += (sender, e) => {
                this.Close();
            };
            this.DataContext = vm;
        }
    }
}
