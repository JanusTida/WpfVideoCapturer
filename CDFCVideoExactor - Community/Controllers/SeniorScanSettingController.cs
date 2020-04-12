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
                PrimaryObjectScanSettingPageViewModel settingPage = null;
                settingPage = mainWindowViewModel.CurPageViewModel == mainWindowViewModel.PrimaryObjectScanSettingPageViewModel ?
                    mainWindowViewModel.PrimaryObjectScanSettingPageViewModel : mainWindowViewModel.MultiMediaPrimaryObjectScanSettingPageViewModel;
                seniorObjectScanSettingViewModel = new SeniorObjectScanSettingViewModel(settingPage.ObjectScanSetting);
                SeniorScanSettingWindow window = new SeniorScanSettingWindow(seniorObjectScanSettingViewModel);
                window.ShowDialog();
                return true;
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("SeniorScanSettingController->Start出错:" + ex.Message);
            }
            return false;
        }
    }
}
