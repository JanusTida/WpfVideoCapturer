using CDFCEntities.DeviceObjects;
using CDFCUIContracts.Abstracts;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CDFCMessageBoxes;
using CDFCMessageBoxes.MessageBoxes;
using CDFCValidaters.Exceptions;
using System.Security.Cryptography;
using System.Diagnostics;
using EventLogger;
using CDFCValidaters.Validaters;
using static CDFCCultures.Managers.ManagerLocator;
using CDFCVideoExactor.Commands;

namespace CDFCVideoExactor.ViewModels.AboutInfo {
    public partial class RegisterOptionsViewModel:BindableBaseTemp {
        public RegisterOptionsViewModel() {
            if (RegisterInfoHelper.RegisterState == RegisterStatus.OK) {
                SerialNumber = RegisterInfo.LocalRegisterInfo?.SerialNumber;
                PhoneNumString = RegisterInfo.LocalRegisterInfo?.PhoneNumberString;
                Email = RegisterInfo.LocalRegisterInfo?.Email;
            }
            if (!File.Exists("Hard.dll")) {
                StringBuilder sb = new StringBuilder();
                var rand = new Random();
                for (int i = 0; i < 20; i++) {
                    var rn = rand.Next(0, 35);
                    if(rn <= 9) {
                        sb.Append(rn.ToString());
                    }
                    else {
                        sb.Append((char)('A' + (rn - 10)));
                    }
                }
                var sw = new StreamWriter("Hard.dll");
                sw.WriteLine(sb.ToString());
                sw.Close();
                HardID = sb.ToString();
            }
            else {
                HardID = File.ReadAllText("Hard.dll");
            }
        }
        
        public string RegisterWord {
            get {
                return RegisterInfoHelper.RegisterState == RegisterStatus.OK ? 
                    FindResourceString("Registered"):FindResourceString("NotRegistered");
            }
        }

        public bool IsReadOnly => RegisterInfoHelper.RegisterState == RegisterStatus.OK;

        private string phoneNumString ;
        public string PhoneNumString {
            get {
                return phoneNumString;
            }
            set {
                if(value != null && value.Length < 14) {
                    SetProperty(ref phoneNumString, value);
                }
            }
        }
        
        private string serialNumber;
        public string SerialNumber {
            get {
                return serialNumber;
            }
            set {
                SetProperty(ref serialNumber, value);
            }
        }

        private string email;
        public string Email {
            get {
                return email;
            }
            set {
                SetProperty(ref email, value);
            }
        }
        public string HardID { get; set; }

        private bool isLoading;
        public bool IsLoading {
            get {
                return isLoading;
            }
            set {
                SetProperty(ref isLoading, value);
            }
        }
    }
    public partial class RegisterOptionsViewModel {
        private RelayCommand submitCommand;
        public RelayCommand SubmitCommand =>
            submitCommand??(submitCommand = new RelayCommand(() => {
                if (CheckInput()) {
                    IsLoading = true;
                    ThreadPool.QueueUserWorkItem(callBack => {
                        Action<string> notifyErrorAct = msgText => {
                            Application.Current.Dispatcher.Invoke(() => {
                                CDFCMessageBox.Show(msgText, FindResourceString("RegisterError"), MessageBoxButton.OK);
                            });
                        };

                        StringBuilder sb = new StringBuilder();
                        RegisterInfo info = new RegisterInfo {
                            PhoneNumberString = PhoneNumString,
                            HardID = ComInfo.LocalHardID,
                            Email = Email,
                            SerialNumber = SerialNumber
                        };

                        //本地校验模块号是否满足当前模块;
                        //
                        if (info.FeatureID != 4096 && info.FeatureID != ConfigState.RequiredFeature) {
                            notifyErrorAct(FindResourceString("SerialFeatureNotMatched"));
                        }
                        else {
                            #region
                            var resBytes = RegisterInfoHelper.SubRegisterInfo(info);
                            if (resBytes != null) {
                                try {
                                    var endo = RegisterInfo.GetEndoByFileBytes(resBytes);
                                    if (endo == null) {
                                        notifyErrorAct(FindResourceString("CheckForRegisterCode"));
                                    }
                                    else {
                                        var rString = AES.Decrypt(endo, "edocrefsnartcfdc");
                                        var rInfo = RegisterInfo.GetRegisterInfoByOri(rString);
                                        if (rInfo == null) {
                                            notifyErrorAct(FindResourceString("SeeLogToCheckForRegisterError"));
                                        }
                                        else {
                                            if (rInfo.Email == Email &&
                                            rInfo.HardID == ComInfo.LocalHardID
                                                && rInfo.PhoneNumberString == PhoneNumString
                                                && rInfo.SerialNumber == SerialNumber) {
                                                File.WriteAllBytes("key.cdfccer", resBytes);
                                                Application.Current.Dispatcher.Invoke(() => {
                                                    if (CDFCMessageBox.Show(FindResourceString("ConfirmToRestartForUpdate"),
                                                        FindResourceString("RegisterSucceed"),
                                                        MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                                        System.Windows.Forms.Application.Restart();
                                                        Environment.Exit(0);
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                                catch (SnNotMatchedException ex) {
                                    RegisterLogger.WriteLine($"{nameof(RegisterOptionsViewModel)}->{nameof(SubmitCommand)}:{ex.Message}");
                                    notifyErrorAct(FindResourceString("InValidSn"));
                                }
                                catch (Exception ex) {
                                    RegisterLogger.WriteLine($"{nameof(RegisterOptionsViewModel)}->{nameof(SubmitCommand)}:{ex.Message}");
                                    notifyErrorAct(FindResourceString("InValidSn"));
                                }
                            }
                            else {
                                RegisterLogger.WriteLine($"{nameof(RegisterOptionsViewModel)}->{nameof(SubmitCommand)}:The res string can't be null!");
                                Application.Current.Dispatcher.Invoke(() => {
                                    CDFCMessageBox.Show(FindResourceString("ServerError"),
                                        FindResourceString("NullRetVal"),
                                        MessageBoxButton.OK);
                                });
                            }
                            #endregion
                        }

                        IsLoading = false;
                    });
                }
            }, () => !(RegisterInfoHelper.RegisterState == RegisterStatus.OK)));
        
        private bool CheckInput() {
            if (string.IsNullOrEmpty(PhoneNumString)) {
                CDFCMessageBox.Show(FindResourceString("InputValidPhone"));
                return false;
            }
            else if (string.IsNullOrEmpty(Email)) {
                CDFCMessageBox.Show(FindResourceString("InputValidEmail"));
                return false;
            }
            else if (string.IsNullOrEmpty(SerialNumber)) {
                CDFCMessageBox.Show(FindResourceString("PleaseInputSn"),
                        FindResourceString("InputError"));
                return false;
            }
            else {
                return true;
            }
        }

        private string BuildStreamBySnAndHardID(string sn, string hardID) {
            if (sn == null)
                throw new ArgumentNullException(nameof(sn));
            if (hardID == null)
                throw new ArgumentNullException(nameof(hardID));

            var provider = new MD5CryptoServiceProvider();

            var snHashStrings = provider.ComputeHash(Encoding.Unicode.GetBytes(sn)).Select(p => p.ToString("X2"));
            var sbHash = new StringBuilder();
            foreach (var item in snHashStrings) {
                sbHash.Append(item);
            }
            var snLength = string.Format("{0:D4}", sn.Length);
            var hardIdLength = string.Format("{0:D4}", hardID.Length);


            return AES.Encrypt($"{sbHash.ToString()}{snLength}{sn}{hardIdLength}{hardID}", "cdfcstorage'code");

        }
    }
}
