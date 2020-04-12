using CDFCLogger.Models;
using CDFCMessageBoxes.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using CDFCCultures.Managers;
using static CDFCCultures.Managers.ManagerLocator;
using System;

namespace CDFCMessageBoxes.MessageBoxes {
    /// <summary>
    /// Interaction logic for CreateCaseMessageBox.xaml
    /// </summary>
    public partial class CreateCaseMessageBox : MetroWindow {
        private CreateCaseWindowViewModel vm;
        public CreateCaseMessageBox(CreateCaseWindowViewModel viewModel) {
            vm = viewModel;
            this.DataContext = viewModel;
            InitializeComponent();
            this.Resources.MergedDictionaries.LoadLanguage("CDFCMessageBoxes");
        }
       
        private void MetroWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool res = false;
            if (bool.TryParse(e.NewValue.ToString(), out res)){
                if (!res) {
                    this.Close();
                }
            }
        }

        public static LoggerCase Show() {
            var vm = new CreateCaseWindowViewModel();
            CreateCaseMessageBox msgBox = new CreateCaseMessageBox(vm);
            var res = msgBox.ShowDialog();
            var loggerCase = vm.LoggerCase;
            if(res == true&& loggerCase.Name != null && loggerCase.CreateTime != null) {
                return loggerCase;
            }
            return null;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (vm.IsEnabled) {
                this.DialogResult = false;
            }
            else {
                this.DialogResult = true;
            }
        }
    }
}
