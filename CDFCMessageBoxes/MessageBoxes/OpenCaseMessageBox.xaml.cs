using CDFCMessageBoxes.Models;
using CDFCMessageBoxes.ViewModels;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Windows;

namespace CDFCMessageBoxes.MessageBoxes {
    /// <summary>
    /// Interaction logic for OpenCaseMessageBox.xaml
    /// </summary>
    public partial class OpenCaseMessageBox : MetroWindow {
        public OpenCaseWindoViewModel vm { get; private set; }
        public OpenCaseMessageBox(List<ListViewItemModel> items) {
            vm = new OpenCaseWindoViewModel(items);
            this.DataContext = vm;
            InitializeComponent();
        }

        private void MetroWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool res;
            if(bool.TryParse(e.NewValue.ToString(),out res)){
                if (!res) {
                    this.Close();
                }
            }
        }

        public static ListViewItemModel Show(List<ListViewItemModel> items) {
            OpenCaseMessageBox msgBox = new OpenCaseMessageBox(items);
            if(msgBox.ShowDialog() == true) {
                return msgBox.vm.SelectedItem;
            }
            else {
                return null;
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!vm.IsEnabled) {
                this.DialogResult = true;
            }
        }
    }
    
}
