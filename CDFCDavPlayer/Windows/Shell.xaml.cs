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

namespace CDFCDavPlayer.Windows {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export]
    public partial class Shell : MetroWindow, IPartImportsSatisfiedNotification {
#pragma warning disable 0649 // Imported by MEF
        [Import(AllowRecomposition =false)]
        private IShellViewModel vm;

        [Import(AllowRecomposition = false)]
        private IModuleManager moduleManager;
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
        }
        
        private void btnCollapse_Click(object sender, RoutedEventArgs e) {
            collaPanel.IsCollapsed = false;
        }

        private void DavPlayerPanel_Click(object sender, EventArgs e) {
            collaPanel.IsCollapsed = true;
        }

       
    }
    
}
