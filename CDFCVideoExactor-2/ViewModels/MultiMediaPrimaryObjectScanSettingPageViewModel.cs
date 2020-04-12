using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Controllers;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.Models;
using System;
using System.ComponentModel;
using System.Windows;

namespace CDFCVideoExactor.ViewModels {
    public class MultiMediaPrimaryObjectScanSettingPageViewModel:PrimaryObjectScanSettingPageViewModel {
        public MultiMediaPrimaryObjectScanSettingPageViewModel(MainWindowViewModel mainWindowViewModel):base(mainWindowViewModel) {
            if(mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("PrimaryObjectScanSettingPageViewModel ->构造方法出错:mainWindowViewModel为空");
            }
        }
    }


    
}
