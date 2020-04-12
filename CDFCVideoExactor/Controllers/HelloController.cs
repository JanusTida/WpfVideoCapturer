using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.ViewModels;
using CDFCMessageBoxes.MessageBoxes;
using System.Threading;
using Aladdin.HASP;
using System;
using System.Xml.Linq;
using System.Linq;
using CDFCVideoExactor.Windows;
using CDFCVideoExactor.Helpers;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;
using System.Diagnostics;
using EventLogger;

namespace CDFCVideoExactor.Controllers {
    public class HelloController : IHelloController {
        private HelloWindowViewModel helloWindowViewModel;
        public HelloController(HelloWindowViewModel helloWindowViewModel) {
            this.helloWindowViewModel = helloWindowViewModel;
        }
        private MainWindow mainWindow = null;
        public void Start() {
            Application.Current.Dispatcher.Invoke(() => {
                mainWindow = new MainWindow();
            });

            ThreadPool.QueueUserWorkItem(callBack => {
                //if (ConfigState.IsDependencyInstalled) {
                //    Application.Current.Dispatcher.Invoke(() => {
                //        if (CDFCMessageBox.Show($"{FindResourceString("UnIntalledDependencies")}:Visual C++ 2010,{FindResourceString("ConfirmToInstall")}", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                //            try {
                //                Process.Start("vcredist_x86.exe");
                //                return;
                //            }
                //            catch (Exception ex) {
                //                CDFCMessageBox.Show(ex.Message);
                //                Logger.WriteLine($"{nameof(HelloController)}->{nameof(callBack)}:{ex.Message}");
                //            }
                //            finally {
                //                Environment.Exit(0);
                //            }
                //        }
                //    });
                //}
                Thread.Sleep(3000);
                
                if (ConfigState.EnWay == Enums.EncryptWay.Dongle) {
                    Application.Current.Dispatcher.Invoke(() => {
#if DEBUG
                        if (CheckDongle()) {
#else
                        if(CheckDongle()){
#endif
                            helloWindowViewModel.IsEnabled = false;
                            helloWindowViewModel.UpdateInvoker.Invoke(() => {
                                mainWindow.Show();
                            });
                        }
                        else {
                            helloWindowViewModel.IsEnabled = false;
                            mainWindow.VM.IsEnabled = false;
                        }
                    });
                }
                else {
                    Action<string, string> notifyAct = (title, word) => {
                       
                        Application.Current.Dispatcher.Invoke(() => {
                            CDFCMessageBox.Show(title, word);
                        });
                    };
                    var rState = RegisterInfoHelper.RegisterState;
                    switch (rState) {
                        case RegisterStatus.NotMatchedFeatureID:
                            notifyAct(FindResourceString("SerialFeatureNotMatched"), FindResourceString("InvalidSN"));
                            break;
                        case RegisterStatus.NotMatchedHardID:
                            notifyAct(FindResourceString("HardIdNotMatched"), FindResourceString("InvalidSN"));
                            break;
                        case RegisterStatus.IDExpired:
                            notifyAct(FindResourceString("SnExpired"),FindResourceString("Tip"));
                            break;
                    }

                    helloWindowViewModel.IsEnabled = false;
                    helloWindowViewModel.UpdateInvoker.Invoke(() => {
                        mainWindow.Show();
                    });
                }
                
            });


        }



#region 检查Dongle是否满足条件;
        private bool CheckDongle() {
            if (mainWindow == null) {
                EventLogger.Logger.WriteLine("HelloController->CheckDongle错误:主窗体为空!");
                throw new NullReferenceException("mainWindow");
            }
            else {
                string entranceString = null;
                
                switch (mainWindow.VM.SelectedEntranceType) {
                    case Enums.EntranceType.Capturer:
                    case Enums.EntranceType.CPAndMultiMedia:
                        entranceString = "25346";
                        break;
                    case Enums.EntranceType.MultiMedia:
                        entranceString = "25324";
                        break;
                    default:
                        EventLogger.Logger.WriteLine("HelloController->CheckDongle错误:Feature不可用。");
                        return true;
                }

                var scope = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?> <haspscope/> ";
                string vendorCode = "U6Y92pZ9VdC9OIn+Nne+v44l/1WBEnwWQO6QdLnSV4yQjfyVe4jyCf9KAjQDBBGlE4s0bWxYoh/tIhT3Tr0t9IVvOnfftRSkMsZaUmJmeqMkM6j0yagdnedRqF07uaK4Kw1LUBqY638AeHTU0U+JOQLSrZh2CHusHERuWil1pl/Eb/Z4h8XfcbHHTsQ/6hNz+zZEISPT0R4eOOt/mliCn+CyarXtiB1A7b87y26c8bYbenaL0VM2XyYTYRUKGbgWhwyE5FOKjL4jQIAl654UbmLqBqNEt6L+sDEkDYZka00+zqI882Tfs6dFAoah3F3S4PdFrlb2YVLSw2Pej253stWwISYiasrs1LXJdq+hoRlu3+zXpyAMMhxxFuSIya4v0U1kuMu5gfIHV+F25LzOtA8YTsOkPCQmyqgB9n63ncFRk9D04zTC2Dpa7ndI9kyKVECptXdSzV6B6JtkSoKYpBFGOuvy6Lou7LoQe199rchzya7haakLjrR0iZhRw0jtfalEu9CsQ7ywyApXCYCyzjz7lcUBOEGSc7LAZajaVQXEvAix44k9t3UJABvfXDugvEvkW4msw+5L7g9ryat+qaZZ/wWI3Lo13e8IN4e3ktKGmBKfZ+UtPD+nS1UqkodnO9Z4c7hds+s5xWRHK7Zz89+CWfzhoe45TtOsYLEyRqK/GVSL/HqxgKfP4Sh50fsCiBtu7ztjjtoxIpabWMgbtnkq33hBNjdX5sjh4ZT5lyKNC0j7oZLhly/20IgDJdyW8Y2jjLG1WB7e22DtLHUANK+j9QuCn1F8G5VmDQo3tJt7aAVrq8yDRwvgZzOi8Fkjf9fqFyrOT64SsFVHgFbgaVGXiFeu2XSNAIDqr5O+6rPI+bZsvM2NW/PSz6KWmBQNFRCtR3wfomJEd/Rjlc8sCEul+AoJtcNO/Qit3EYDgeE=";
                string info = null;
#region 登录一次,确保start_time存在;
                HaspFeature hf = HaspFeature.FromFeature(int.Parse(entranceString));
                Hasp hs = new Hasp(hf);
                var loginStatus = hs.Login(vendorCode);
                var logOutStatus = hs.Logout();
                
                if(loginStatus == HaspStatus.ContainerNotFound || loginStatus == HaspStatus.TooOldLM) {
                    var checkExpired = NetReactorHelper.CheckExpired();
                    if(checkExpired == false) {
                        //CDFCMessageBox.Show(FindResourceString("TimeModified"));
                        NetReactorHelper.KillSelf();
                        return false;
                    }
                    //CDFCMessageBox.Show(FindResourceString("DongleNotFound"));
                    return true;
                }
                else if(loginStatus != HaspStatus.StatusOk) {
                    EventLogger.Logger.WriteLine("HelloController->登录加密狗错误,Status:"+loginStatus);
                }
#endregion

#region 获得模块信息，并检查;
                var featureFormat = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                                                          "<haspformat root=\"hasp_info\">" +
                                                            " <feature>" +
                                                            "  <attribute name=\"id\" />" +
                                                            "  <element name=\"license\" />" +
                                                            " </feature>" +
                                                            "</haspformat>";
                var status = Hasp.GetInfo(scope, featureFormat, vendorCode, ref info);
                if (status == HaspStatus.StatusOk) {
                    XDocument featureDoc = XDocument.Parse(info);
                    var infoElem = featureDoc.Root;
                    var featureElems = infoElem.Elements().Where(p => p.Attribute(XName.Get("id")).Value == entranceString);
                    if (featureElems.Count() != 1) {
                        if (featureElems.Count() == 0) {
                            EventLogger.Logger.WriteLine("SerialFeatureNotFound:" + entranceString);
                            CDFCMessageBox.Show(FindResourceString("DongleSnNotFound"));
                        }
                        else {
                            EventLogger.Logger.WriteLine("序列号不唯一!");
                            featureElems.Select(p => p.Value).ToList().
                                ForEach(p => EventLogger.Logger.WriteLine(p));
                        }
                        return false;
                    }
                    else {
                        var featureElem = featureElems.First();
                        var licenseElem = featureElem.Element(XName.Get("license"));
                        //当前发布日期戳;
                        int nowNum = 1481711190;

                        if (licenseElem != null) {
                            var licenseTypeElem = licenseElem.Element(XName.Get("license_type"));
#region 若为永久许可,则通过验证;
                            if (licenseTypeElem.Value == "perpetual") {
                                return true;
                            }
#endregion

#region 若为限制时间段类型许可;
                            else if (licenseTypeElem.Value == "trial") {
                                var totalTimeElem = licenseElem.Element(XName.Get("total_time"));
                                var startTimeElem = licenseElem.Element(XName.Get("time_start"));
                                if (totalTimeElem != null && startTimeElem != null) {
                                    int totalTimeNum = Convert.ToInt32(totalTimeElem.Value);
                                    int startTimeNum = 0;
                                    if (Int32.TryParse(startTimeElem.Value, out startTimeNum)) {
                                        int expirateTimeNum = totalTimeNum + startTimeNum;
                                        if (expirateTimeNum < nowNum) {
                                            EventLogger.Logger.WriteLine("密钥使用期限不足:" + expirateTimeNum + "\t" + nowNum);
                                            CDFCMessageBox.Show(FindResourceString("DongleExpiredForVersion"));
                                            return false;
                                        }
                                        else {
                                            return true;
                                        }
                                    }
                                    else {
                                        return true;
                                    }
                                }
                                else {
                                    return false;
                                }
                            }
#endregion

#region 若为延时使用许可;
                            else if(licenseTypeElem.Value == "expiration") {
                                var expDateElem = licenseElem.Element(XName.Get("exp_date"));
                                int expDateNum = 0;
                                if(Int32.TryParse(expDateElem.Value,out expDateNum)){
                                    if(expDateNum < nowNum) {
                                        EventLogger.Logger.WriteLine("密钥使用期限不足:" + expDateNum + "\t" + nowNum);
                                        CDFCMessageBox.Show(FindResourceString("DongleExpiredForVersion"));
                                        return false;
                                    }
                                    else {
                                        return true;
                                    }
                                }
                                else {
                                    EventLogger.Logger.WriteLine("HelloController->CheckDongle错误:延时日期转换错误:"+expDateElem.Value);
                                    CDFCMessageBox.Show(FindResourceString("DongleDateConvertError"));
                                    return false;
                                }
                            }
#endregion
                            
                        }
                        else {
                            EventLogger.Logger.WriteLine("HelloController->CheckDongle为找到相关许可元素!");
                            return false;
                        }
                    }
#endregion
                }
                return false;
            }
#endregion


        }
    }
}
