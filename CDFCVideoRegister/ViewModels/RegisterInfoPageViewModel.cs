using CDFCEntities.DeviceObjects;
using CDFCMessageBoxes.MessageBoxes;
using CDFCVideoRegister.Abstracts;
using CDFCVideoRegister.Commands;
using CDFCVideoRegister.Models;
using System;
using System.Threading;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoRegister.ViewModels {
    /// <summary>
    /// 注册窗体的状态信息;
    /// </summary>
    public partial class RegisterInfoPageViewModel:ViewModelBase {
        private NotifyingRegisterInfo registerInfo;
        public NotifyingRegisterInfo RegisterInfo {
            get {
                return registerInfo ??
                    (registerInfo = new NotifyingRegisterInfo());
            }
        }

        public RegisterInfoPageViewModel(MainWindowViewModel mainWindowViewModel) {
            if(mainWindowViewModel == null) {
                EventLogger.RegisterLogger.WriteLine("RegisterInfoViewModel初始化错误:参数不得为空!");
            }
            this.mainWindowViewModel = mainWindowViewModel;
        }

        /// <summary>
        /// 主窗体模型；
        /// </summary>
        private MainWindowViewModel mainWindowViewModel;

        /// <summary>
        /// 当前是否处于等待状态;
        /// </summary>
        private bool isLoading = false;
        public bool IsLoading {
            get {
                return isLoading;
            }
            set {
                isLoading = value;
                NotifyPropertyChanging(nameof(IsLoading));
            }
        }
    }
    /// <summary>
    /// 注册窗体的命令绑定;
    /// </summary>
    public partial class RegisterInfoPageViewModel : ViewModelBase {
        /// <summary>
        /// 确认注册的命令;
        /// </summary>
        private RelayCommand confirmCommand;
        public RelayCommand ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand = new RelayCommand(
                        () => {
                            //EventLogger.RegisterLogger.WriteLine("1st");
                            if (CheckInput()) {
                                //EventLogger.RegisterLogger.WriteLine("2nd");
                                try {
                                    IsLoading = true;
                                    int res = -1000;
                                   // EventLogger.RegisterLogger.WriteLine("3rd");
                                    ThreadPool.QueueUserWorkItem(callBack => {
                                        try { 
                                            Register register = new Register(new Models.RegisterInfo {
                                                Name = RegisterInfo.Name,
                                                Company = RegisterInfo.Company,
                                                Email = RegisterInfo.Email,
                                                Phone = RegisterInfo.Phone,
                                                HardId = ComInfo.LocalHardID,
                                                SoftName = "BlackHole"
                                            });
                                            //EventLogger.RegisterLogger.WriteLine("4th");
                                            res = register.Validate();
                                            //若res == 0,则调用成功。
                                            if (res == 0) {
                                               // EventLogger.RegisterLogger.WriteLine("5th");
                                                mainWindowViewModel.RegisterFinishedPageViewModel = new RegisterFinishedPageViewModel(mainWindowViewModel, register.RegisterInfo);
                                                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                                    mainWindowViewModel.CurPageViewModel = mainWindowViewModel.RegisterFinishedPageViewModel;
                                                });
                                            }
                                            else {
                                                EventLogger.RegisterLogger.WriteLine("RegisterInfoPageViewModel->ConfirmCommand错误:错误码:" + res);
                                                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                                    CDFCMessageBox.Show("抱歉,注册未知错误:" + res);
                                                });
                                            }
                                        }
                                        catch(PlatformNotSupportedException ex) {
                                            EventLogger.RegisterLogger.WriteLine("RegisterInfoPageViewModel->ConfirmCommand错误:不支持的平台:" + ex.Message);
                                            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                                CDFCMessageBox.Show("抱歉，获取本机信息错误!");
                                            });
                                        }
                                        catch(Exception ex) {
                                            EventLogger.RegisterLogger.WriteLine("6th:" + ex.Message);
                                            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                                CDFCMessageBox.Show(FindResourceString("UnknownErrorEncountered"));
                                            });
                                        }
                                        IsLoading = false;
                                    });
                                    
                                    
                                }
                                catch (Exception ex) {
                                    EventLogger.RegisterLogger.WriteLine("RegisterInfoViewModel->ConfirCommand未知错误:" + ex.Message);
                                }

                            }
                        }
                        )
                    );
            }
        }
        private bool CheckInput() {
            return !string.IsNullOrEmpty(RegisterInfo.Company) &&
                               !string.IsNullOrEmpty(RegisterInfo.Name) &&
                               !string.IsNullOrEmpty(RegisterInfo.Email) &&
                               !string.IsNullOrEmpty(RegisterInfo.Phone);
        }
    }

}
