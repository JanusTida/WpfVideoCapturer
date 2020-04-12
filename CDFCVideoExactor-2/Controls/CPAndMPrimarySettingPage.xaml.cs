﻿using System;
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

namespace CDFCVideoExactor.Controls {
    /// <summary>
    /// Interaction logic for CPAndMPrimarySettingPage.xaml
    /// </summary>
    public partial class CPAndMPrimarySettingPage : UserControl {
        public CPAndMPrimarySettingPage() {
            InitializeComponent();
        }
        private void numericTextBox_GetFocus(object sender, System.Windows.RoutedEventArgs e) {
            var numTxb = sender as TextBox;
            if (numTxb != null) {
                numTxb.SelectAll();
            }
        }
    }
}
