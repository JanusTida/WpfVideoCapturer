using CDFCDavPlayer.Models;
using CDFCDavPlayer.ViewModels;
using CDFCUIContracts.Commands;
using MahApps.Metro.Controls;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CDFCDavPlayer.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]
    public partial class Shell : MetroWindow, IPartImportsSatisfiedNotification {
#pragma warning disable 0649 // Imported by MEF
        [Import(AllowRecomposition = false)]
        private IShellViewModel vm;
#pragma warning restore 0649

        public Shell() {
            try
            {
                InitializeComponent();
            }
            catch(Exception ex)
            {

            }
            
            
        }


        public void OnImportsSatisfied() {
            DataContext = vm;
            vm.ActivateRequest += Vm_ActivateRequest;
        }

        private void Vm_ActivateRequest(object sender, EventArgs e)
        {
            this.Activate();
        }

        private void btnCollapse_Click(object sender, RoutedEventArgs e) {
            collaPanel.IsCollapsed = false;
        }

        private void DavPlayerPanel_Click(object sender, EventArgs e) {
            collaPanel.IsCollapsed = true;
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            vm.RaiseOnLoad();
        }

        private void MetroWindow_Drop(object sender, DragEventArgs e)
        {
            try
            {
                vm.RaiseOnDrop(e.Data);
            }
            catch(Exception ex)
            {

            }
            
        }
    }
    
}
