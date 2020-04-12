using CDFCCultures.Managers;
using CDFCMessageBoxes.ViewModels;
using MahApps.Metro.Controls;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCMessageBoxes.MessageBoxes {
    /// <summary>
    /// Interaction logic for CDFCMessageBox.xaml
    /// </summary>
    public partial class CDFCMessageBox : MetroWindow {
        private CDFCMessageBoxViewModel vm;
        private CDFCMessageBox(CDFCMessageBoxViewModel vm) {
            this.vm = vm;
            this.DataContext = vm;
            InitializeComponent();
        }
        
        private void MetroWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            bool res = false;
            if (bool.TryParse(e.NewValue.ToString(), out res)) {
                if (!res) {
                    this.Close();
                }
            }
        }
        public static MessageBoxResult Show(string msgText) {
            return Show(msgText, FindResourceString("Tip"), MessageBoxButton.OK);
        }
        public static MessageBoxResult Show(string msgText,MessageBoxButton button) {
            return Show(msgText, FindResourceString("Tip"), button);
        }
        
        public static MessageBoxResult Show(string msgText,string caption,MessageBoxButton button = MessageBoxButton.OK) {
            var vm = new CDFCMessageBoxViewModel(button,msgText,caption);
            var msg = new CDFCMessageBox(vm);
            var res = msg.ShowDialog();
            switch (vm.DialogResult) {
                case null:
                    if(button == MessageBoxButton.YesNoCancel)
                        return MessageBoxResult.Cancel;
                    return MessageBoxResult.None;
                case false:
                    return MessageBoxResult.No;
                case true:
                    if(button == MessageBoxButton.OK) 
                        return MessageBoxResult.OK;
                    return MessageBoxResult.Yes;
                default:
                    return MessageBoxResult.None;
            }
        }
    }
}
