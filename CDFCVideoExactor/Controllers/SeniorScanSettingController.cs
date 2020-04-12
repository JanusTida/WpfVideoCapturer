using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.ViewModels;
using System;

namespace CDFCVideoExactor.Controllers {
    //控制高级设定的控制器;
    public class SeniorScanSettingController : ISeniorScanSettingController {
        private MainWindowViewModel mainWindowViewModel;
        private SeniorObjectScanSettingViewModel seniorObjectScanSettingViewModel;
        public SeniorScanSettingController(MainWindowViewModel mainWindowViewModel) {
            if(mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("SeniorScanSettingController构造出错:参数mainWindowViewModel不得为空!");
                throw new NullReferenceException("mainWindowViewModel can't be null!");
            }
            this.mainWindowViewModel = mainWindowViewModel;
        }
        public bool Start() {
            try {
                var settingPage = mainWindowViewModel.CurPageViewModel as PrimaryObjectScanSettingPageViewModel;
                if(settingPage != null) {
                    seniorObjectScanSettingViewModel = new SeniorObjectScanSettingViewModel(settingPage.ObjectScanSetting);
                    var window = new SeniorScanSettingWindow(seniorObjectScanSettingViewModel);
                    window.ShowDialog();
                    return true;
                }
                else {
                    return false;
                }
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("SeniorScanSettingController->Start出错:" + ex.Message);
            }
            return false;
        }
    }
}
