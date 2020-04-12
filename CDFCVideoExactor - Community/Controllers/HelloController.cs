using CDFCEntities.DeviceObjects;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.ViewModels;
using CDFCVideoRegister.Models;
using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using WPFCustomMessageBox;
using CDFCMessageBoxes.MessageBoxes;

namespace CDFCVideoExactor.Controllers {
    public class HelloController : IHelloController {
        private HelloWindowViewModel helloWindowViewModel;
        private static string keyFileName = "key.cdfccer";
        public HelloController(HelloWindowViewModel helloWindowViewModel) {
            this.helloWindowViewModel = helloWindowViewModel;
        }
        public void Start() {
            MainWindow mainWindow = null;

            ThreadPool.QueueUserWorkItem(callBack => {
                Thread.Sleep(3000);
                //若存在证书文件，进行验证;
                helloWindowViewModel.UpdateInvoker.Invoke(() => {
                    if (true) {
                        mainWindow = new MainWindow();
                        mainWindow.Show();
                    };
                });
                helloWindowViewModel.IsEnabled = false;
                //if (File.Exists(keyFileName)) {
                //    helloWindowViewModel.UpdateInvoker.Invoke(() => {
                //        if (ValidateCertification()) {
                //            mainWindow = new MainWindow();
                //            mainWindow.Show();
                //        };
                //    });
                //}
                //else {
                //    //检查对话框选择的状态,决定是否关闭对话框。
                //    bool passed = false;

                //    while (!passed) {
                //        #region 查找证书文件;
                //        helloWindowViewModel.UpdateInvoker.Invoke(() => {
                //            var res = CustomMessageBox.ShowYesNoCancel("天宇宁达-注册", "未找到证书文件,进行注册吗?", "进行注册", "我已有证书文件", "取消", MessageBoxImage.Warning);
                //            if (res ==MessageBoxResult.Yes) {
                //                RegisterExecute();
                //                passed = true;
                //            }
                //            else if (res == MessageBoxResult.No) {
                //                var dialog = new VistaOpenFileDialog();
                //                dialog.Filter = "(天宇宁达-证书文件)|*.cdfccer";
                //                dialog.Multiselect = false;

                //                dialog.FileName = "D://key.cdfccer";
                //                if (dialog.ShowDialog() == true) {
                //                    if (dialog.FileName.Length < keyFileName.Length) {
                //                        CDFCMessageBox.Show("文件名错误!请选择正确的文件名:" + keyFileName);
                //                    }
                //                    else {
                //                        var fullFileName = dialog.FileName;
                //                        var fileName = fullFileName.Substring(fullFileName.Length - 11, 11);
                //                        if (fileName == keyFileName) {
                //                            File.Copy(fullFileName, AppDomain.CurrentDomain.BaseDirectory + keyFileName, true);
                //                            if (ValidateCertification()) {
                //                                passed = true;
                //                                helloWindowViewModel.UpdateInvoker.Invoke(() => {
                //                                    mainWindow = new MainWindow();
                //                                    mainWindow.Show();
                //                                });
                //                            }
                //                        }
                //                        else {
                //                            CDFCMessageBox.Show("文件名错误!请选择正确的文件名:" + keyFileName);
                //                        }
                //                    }
                //                }

                //            }
                //            else {
                //                passed = true;

                //            }
                //        });
                //        #endregion
                //    }
                //}
                //

            });
        }

        /// <summary>
        /// 对证书进行验证;
        /// </summary>
        /// <returns></returns>
        private bool ValidateCertification() {
            bool validated = false;
            try {
                #region 若存在证书，则进行相关验证
                string code = File.ReadAllText(keyFileName);
                var fieldList = AES.Decrypt(code, "cdfcstorage'code").Split(new char[] { '&', '&' }).ToList();
                fieldList.RemoveAll(p => string.IsNullOrEmpty(p));
                if (fieldList.Count != 7) {
                    throw new ArgumentOutOfRangeException("字段数量错误:" + fieldList.Count);
                }
                RegisterInfo info = new RegisterInfo {
                    Name = fieldList[0],
                    Phone = fieldList[1],
                    Email = fieldList[2],
                    SoftName = fieldList[3],
                    HardId = fieldList[4],
                    Company = fieldList[5]
                };
                if (info.HardId != ComInfo.LocalHardID) {
                    throw new CredentialException("证书硬件ID与本地信息不符");
                }

                try {
                    #region 进行服务器认证;
                    CDFCRegisteryValidater validater = new CDFCRegisteryValidater(info);
                    var res = validater.CheckExpired();
                    if (res == -1) {
                        validated = true;
                        EventLogger.RegisterLogger.WriteLine("证书验证错误!" + res);
                        CDFCMessageBox.Show("证书验证错误，请核实。");
                    }
                    else if (res == -2) {
                        if (CDFCMessageBox.Show("您的试用期已终止，重新注册？", "试用终止", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                            
                            RegisterExecute();
                        }
                    }
                    else if (res != 0) {
                        CDFCMessageBox.Show("证书未知错误:" + res);
                    }
                    else {
                        validated = true;
                    }
                    #endregion
                }
                catch (WebException ex) {
                    #region 若网络不可达，则进行本地验证；
                    try {
                        EventLogger.RegisterLogger.WriteLine("网络不可达" + ex.Message + "进行本地验证...");
                        var dt = StampToDateTime(fieldList[6]);
                        if (dt > DateTime.Now) {
                            validated = true;
                        }
                        else {
                            if (CDFCMessageBox.Show("您的试用期已终止，重新注册？", "试用终止", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                RegisterExecute();
                            }
                        }
                    }
                    catch (Exception innerEx) {
                        EventLogger.RegisterLogger.WriteLine("时间验证错误:" + innerEx.Message + "字段：" + fieldList[6]);
                        CDFCMessageBox.Show("时间验证错误:请核实!");
                    }
                    #endregion
                }
                catch (Exception ex) {
                    EventLogger.RegisterLogger.WriteLine("证书未知错误：" + ex.Message);
                    CDFCMessageBox.Show("服务端错误:请移步至www.cflab.net下载最新版本的试用版!");
                }
                #endregion
            }
            catch (ArgumentOutOfRangeException ex) {
                EventLogger.RegisterLogger.WriteLine("证书文件字段数量错误：" + ex.Message);
                CDFCMessageBox.Show("证书文件字段数量错误:请核实!");
            }
            catch (CredentialException ex) {
                EventLogger.RegisterLogger.WriteLine("硬件ID认证错误:" + ex.Message);
                CDFCMessageBox.Show("证书硬件ID与当前设备不符:请核实!");
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("证书文件错误：" + ex.Message);
                CDFCMessageBox.Show($"证书文件错误:请核实{ex.Message}");
            }

            return validated;
        }

        private DateTime StampToDateTime(string timeStamp) {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(lTime);

            return dateTimeStart.Add(toNow);
        }

        // DateTime时间格式转换为Unix时间戳格式
        private int DateTimeToStamp(System.DateTime time) {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        //执行注册过程;
        private static void RegisterExecute() {
            string registerPath = AppDomain.CurrentDomain.BaseDirectory + "CDFCVideoRegister.exe";
            if (File.Exists(registerPath)) {
                Process.Start(registerPath);
            }
            else {
                CDFCMessageBox.Show("注册程序损坏，请移步至http://www.cflab.net下载最新版本!");
            }
        }
    }
}
