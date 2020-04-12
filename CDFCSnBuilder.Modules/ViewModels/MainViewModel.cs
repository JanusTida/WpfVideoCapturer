using CDFCSnBuilder.Modules.Models;
using CDFCUIContracts.Commands;
using Ookii.Dialogs.Wpf;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace CDFCSnBuilder.Modules.ViewModels {
    public class MainViewModel:BindableBase {
        public MainViewModel() {
            LoadDeviceTypes();
            BuiltReports = new ObservableCollection<string>();
        }
        private void LoadDeviceTypes() {
            this.FeatureModels = new ObservableCollection<DeviceFeatureModel>();

            try {
                var xDoc = XDocument.Load("DeviceTypes.xml");
                var rootElem = xDoc.Root;
                var deviceElems = rootElem.Elements().Where(p => p.Name == "DeviceType");
                foreach (var item in deviceElems) {
                    var idElem = item.Element(XName.Get("ID"));
                    var nameElem = item.Element(XName.Get("Name"));
                    if(idElem != null && nameElem != null) {
                        var deviceFeature = new DeviceFeatureModel {
                            FeatureID = int.Parse(idElem.Value),
                            Name = nameElem.Value
                        };
                        deviceFeature.IsCheckedChanged += (sender, e) => {
                            CommandManager.InvalidateRequerySuggested();
                        };
                        this.FeatureModels.Add(deviceFeature);
                    }
                }
            }
            catch {

            }
            
        }
        public ObservableCollection<DeviceFeatureModel> FeatureModels { get; set; }

        private string buildCountString = "1";
        public string BuildCountString {
            get {
                return buildCountString;
            }
            set {
                var buildCount = 0;
                if(int.TryParse(value,out buildCount)) {
                    if(0 < buildCount && buildCount <= 100) {
                        buildCountString = buildCount.ToString();
                    }
                }
                else {
                    RaisePropertyChanged();
                }

            }
        }

        public ObservableCollection<string> BuiltReports { get; set; }
        private List<BuiltSnModel> builtItems = new List<BuiltSnModel>();

        //private string limitedTimeSpanString = "0";
        //public string LimitedTimeSpanString {
        //    get {
        //        return limitedTimeSpanString;
        //    }
        //    set {
        //        SetProperty(ref limitedTimeSpanString, value);
        //    }
        //}
        private bool _dateLimited;
        public bool DateLimited {
            get {
                return _dateLimited;
            }
            set {
                SetProperty(ref _dateLimited, value);
            }
        }

        private string _limDate;
        public string LimDate {
            get {
                return _limDate;
            }
            set {
                SetProperty(ref _limDate, value);
            }
        }


        private RelayCommand buildCommand;
        public RelayCommand BuildCommand =>
            buildCommand ??
            (buildCommand = new RelayCommand(() => {
                try {
                    IsLoading = true;
                    uint limDt = 0;
                    if (DateLimited) {
                        limDt = uint.Parse(LimDate);
                    }
                    builtItems.Clear();
                    BuiltReports.Add("生成开始...");
                    ThreadPool.QueueUserWorkItem(callBack => {
                        for (int i = 0; i < int.Parse(BuildCountString); i++) {
                            var builtModel = new BuiltSnModel {
                                DateTime = string.Format("{0:D4}{1:D2}{2:D2}{3:D2}{4:D2}{5:D2}",
                                DateTime.Now.Millisecond,
                                DateTime.Now.Second,
                                DateTime.Now.Minute,
                                DateTime.Now.Hour,
                                DateTime.Now.Day,
                                DateTime.Now.Month,
                                DateTime.Now.Year),
                                FeatureID = $"{FeatureModels.First(p => p.IsChecked).FeatureID:D4}",
                                LimTS = TimeSpan.FromDays(limDt)
                            };

                            var decryptedString = builtModel.DateTime + builtModel.FeatureID+
                            $"{(DateLimited ? 1:0):D4}{(int)(builtModel.LimTS?.TotalSeconds ?? 0):D10}";
                            var sss = decryptedString.Length;
                            var base1 = AES.Encrypt(decryptedString, "cdfcstorage'code");
                            var base2 = AES.Encrypt(base1, "cdfcstorage'code"); 
                            builtModel.Sn = base2;
                            var originLength = decryptedString.Length;
                            var snLength = decryptedString.Length;
                            builtItems.Add(builtModel);
                            Application.Current.Dispatcher.Invoke(() => {
                                BuiltReports.Add($"功能ID:{FeatureModels.First(p => p.IsChecked).FeatureID}({FeatureModels.First(p => p.IsChecked).Name}) 生成时间:{builtModel.DateTime}sn号:{builtModel.Sn}");
                            });
                            Thread.Sleep(10);
                        };
                        Application.Current.Dispatcher.Invoke(() => {
                            BuiltReports.Add("生成结束");
                            
                        });
                    });
                }
                catch {

                }
                finally {
                    IsLoading = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            },() => 
                FeatureModels.FirstOrDefault(p => p.IsChecked) != null));

        private RelayCommand saveAsCommand;
        public RelayCommand SaveAsCommand =>
            saveAsCommand = new RelayCommand(() => {
                var dialog = new VistaSaveFileDialog();
                dialog.Filter = "(文本文件)|*.txt";
                if(dialog.ShowDialog() == true) {
                    IsLoading = true;
                    ThreadPool.QueueUserWorkItem(callBack => {
                        try {
                            var fileName = dialog.FileName.EndsWith(".txt") ? dialog.FileName : dialog.FileName + ".txt";
                            var fs = File.Create(fileName);
                            var sw = new StreamWriter(fs);
                            sw.WriteLine($"功能ID:{builtItems.First().FeatureID}");
                            builtItems.ForEach(p => {
                                sw.WriteLine(p.Sn);
                            });
                            sw.Close();
                            Application.Current.Dispatcher.Invoke(() => {
                                if(MessageBox.Show("是否查看文件?", "生成成功", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                    Process.Start("explorer", fileName);
                                }
                            });
                        }
                        catch(Exception ex) {
                            Application.Current.Dispatcher.Invoke(() => {
                                MessageBox.Show($"生成错误:{ex.Message}");
                            });
                        }
                        finally {
                            IsLoading = false;
                        }
                    });
                }
            },() => builtItems.FirstOrDefault() != null);
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
    public static class AES {
        public static string Encrypt(string toEncrypt, string keyCode) {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(keyCode);
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string toDecrypt, string keyCode) {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes(keyCode);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

    }
}
