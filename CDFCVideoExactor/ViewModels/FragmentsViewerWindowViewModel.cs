using CDFCEntities.Interfaces;
using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using WPFHexaEditor.Control.MessageBoxes;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 碎片图表查看器的视图模型;
    /// </summary>
    public partial class FragmentsViewerWindowViewModel:ViewModelBase {
        /// <summary>
        /// 碎片查看器的构造方法;
        /// </summary>
        /// <param name="mainWindowViewModel">主窗体模型</param>
        public FragmentsViewerWindowViewModel(IObjectDevice objectDevice,bool isChannelColVisible) {
            this.isChannelColVisible = isChannelColVisible;
            this.ObjectDevice = objectDevice;
        }
        
        public IObjectDevice ObjectDevice { get; private set; }
        public ObservableCollection<CellFragment> CellFragments { get; set; }

        /// <summary>
        /// 选定的碎片行;
        /// </summary>
        private CellFragment selectedFragment;
        public CellFragment SelectedFragment {
            get {
                return selectedFragment;
            }
            set {
                selectedFragment = value;
                NotifyPropertyChanging(nameof(SelectedFragment));
            }
        }
    }

    
    /// <summary>
    /// 碎片图表查看器的视图状态;
    /// </summary>
    public partial class FragmentsViewerWindowViewModel {
        public event EventHandler<bool> IsLoadingChanged;
        private bool isLoading;
        public bool IsLoading {
            get {
                return isLoading;
            }
            set {
                isLoading = value;
                IsLoadingChanged.Invoke(this, value);
            }
        }
        /// <summary>
        /// 通道号列是否可见;(当选择监控类型时);
        /// </summary>
        private bool isChannelColVisible;
        public bool IsChannelColVisible {
            get {
                return isChannelColVisible;
            }
        }

        #region 控制窗体是否可用，以关闭窗体;
        private bool isEnabled = true;
        public bool IsEnabled {
            get {
                return isEnabled;
            }
            set {
                isEnabled = value;
                NotifyPropertyChanging(nameof(IsEnabled));
            }
        }
        #endregion
        
        public bool IsMP4Class {
            get {
                return CDFCSetting.ScanSetting.IsMP4Class;
            }
        }
    }

    /// <summary>
    /// 碎片图表查看器的视图模型绑定项；
    /// </summary>
    public partial class FragmentsViewerWindowViewModel {
        #region 确定以退出的命令;
        private RelayCommand sureCommand;
        public RelayCommand SureCommand {
            get {
                return sureCommand ??
                    (sureCommand = new RelayCommand(SureExecuted));
            }
        }

        /// <summary>
        /// 确定命令所执行的动作;
        /// </summary>
        private void SureExecuted() {
            IsEnabled = false;
            if(hexBox != null) {
                hexBox.Close();
                hexBox = null;
            }
        }

        private RelayCommand closingCommand;
        public RelayCommand ClosingCommand {
            get {
                return closingCommand??
                    (closingCommand = new RelayCommand(() => {
                    if(hexBox != null) {
                        hexBox.Close();
                        hexBox = null;
                    }
                    }));
            }
        }
        #endregion

        /// <summary>
        /// 恢复某碎片的命令；
        /// </summary>
        private RelayCommand recoverFragmentCommand;
        public RelayCommand RecoverFragmentCommand {
            get {
                return recoverFragmentCommand ??
                    (recoverFragmentCommand = new RelayCommand(
                        () => {
                        if (CDFCSetting.ScanSetting.VersionType.DeviceType != CDFCEntities.Enums.DeviceTypeEnum.DaHua
                        && CDFCSetting.ScanSetting.VersionType.DeviceType != CDFCEntities.Enums.DeviceTypeEnum.WFS) {
                                CDFCMessageBox.Show(FindResourceString("BrandLimited"));
                                return;
                        }
                        if (selectedFragment != null) {
                            var dialog = new VistaSaveFileDialog();
                            dialog.Title = FindResourceString("ChooseOutputDirec");
                            if (dialog.ShowDialog() == true) {
                                var size = selectedFragment.Fragment.Size;
                                var saveFileStream = new FileStream(dialog.FileName, FileMode.Create);
                                var startAddress = selectedFragment.Fragment.StartAddress;
                                var ptrBuffer = Marshal.AllocHGlobal(Convert.ToInt32(size));
                                var ptrSize = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(uint)));
                                if (CDFCStatic.CMethods.CommonMethods.cdfc_common_read(
                                    ObjectDevice.Handle,
                                    (ulong)selectedFragment.Fragment.StartAddress,
                                    ptrBuffer, (ulong)selectedFragment.Fragment.Size,
                                    ptrSize, true)) {
                                    CDFCStatic.CMethods.CommonMethods.cdfc_common_write(saveFileStream.SafeFileHandle.DangerousGetHandle(),
                                        0, ptrBuffer, (ulong)selectedFragment.Fragment.Size, ptrSize, false);
                                }
                                Marshal.FreeHGlobal(ptrBuffer);
                                Marshal.FreeHGlobal(ptrSize);
                                saveFileStream.Close();

                                if (CDFCMessageBox.Show(FindResourceString("ConfirmToBrowseFrag"),
                                    FindResourceString("SavingCompleted"),
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                    try {
                                        var fileName = dialog.FileName;
                                        var path = fileName.Substring(0,fileName.LastIndexOf("\\"));
                                          Process.Start("explorer", path);
                                        }
                                        catch (Exception ex) {
                                            EventLogger.Logger.WriteLine("FragmentViewerWindowViewModel->打开文件夹错误:" + ex.Message);
                                            CDFCMessageBox.Show(FindResourceString("FailedToOpenFolder"));
                                        }
                                    }
                                }
                            }

                        },
                        () => selectedFragment != null
                    ));
            }
        }

        private static ObjectDeviceHexMessageBox hexBox;
        private RelayCommand showInHexViewerCommand;
        public RelayCommand ShowInHexViewerCommand {
            get {
                return showInHexViewerCommand ??
                    (showInHexViewerCommand = new RelayCommand(
                        () => {
                            IsLoading = true;
                            if(hexBox == null) {
                                hexBox = new ObjectDeviceHexMessageBox(ObjectDevice);
                                hexBox.Show();
                            }
                            hexBox.GetFocus();
                            hexBox.SetPosition((long)SelectedFragment.Fragment.StartAddress,(long) selectedFragment.Fragment.Size);

                            IsLoading = false;
                        },
                        () => SelectedFragment != null
                    ));
            }
        }
    }
}
