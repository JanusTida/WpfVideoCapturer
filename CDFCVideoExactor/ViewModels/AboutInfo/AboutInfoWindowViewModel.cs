using CDFCEntities.Enums;
using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Abstracts;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Enums;
using CDFCVideoExactor.Helpers;
using EventLogger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.ViewModels.AboutInfo {
    public partial class AboutInfoWindowViewModel:BindableBaseTemp {
        public string IconSource {
            get {
                if (ConfigState.EtrType == EntranceType.CapturerSingle) {
                    if (ConfigState.SingleType != DeviceTypeEnum.Unknown) {
                        return $"/CDFCVideoExactor;component/Images/Icons/BlackHole_{ConfigState.SingleType}.png";
                    }
                }
                return $"/CDFCVideoExactor;component/Images/Icons/BlackIcon.ico";
            }
        }
        public string FeatureName {
            get {
                return ConfigState.SoftName;
            }
        }
        private RegisterOptionsViewModel registerOptionsViewModel;
        public RegisterOptionsViewModel RegisterOptionsViewModel =>
            registerOptionsViewModel ?? (registerOptionsViewModel = new RegisterOptionsViewModel());

        private FeedBackViewModel feedBackViewModel;
        public FeedBackViewModel FeedBackViewModel =>
            feedBackViewModel ??
            (feedBackViewModel = new FeedBackViewModel());

    }
    public partial class AboutInfoWindowViewModel  {
        private bool isCheckingNewVersion;
        public bool IsCheckingNewVersion {
            get {
                return isCheckingNewVersion;
            }
            set {
                SetProperty(ref isCheckingNewVersion, value);
            }
        }

        private RelayCommand checkForNewVersionCommand;
        public RelayCommand CheckForNewVersionCommand =>
            checkForNewVersionCommand ??
            (checkForNewVersionCommand = new RelayCommand(() => {
                IsCheckingNewVersion = true;
                ThreadPool.QueueUserWorkItem(callBack => {
                    try {
                        CDFCVideoExactorUpdater.Helpers.VersionHelper.VersionBranch = ConfigState.SingleType.ToString();
                        var hasNewVersion = CDFCVideoExactorUpdater.Helpers.VersionHelper.HasNewVersion;
                        var latestVersion = CDFCVideoExactorUpdater.Helpers.VersionHelper.LatestVersion;

                        //var items = CDFCVideoExactorUpdater.Helpers.VersionHelper.ItemsNeed;

                        if (!closed &&( hasNewVersion && latestVersion != ConfigState.VersionString )) {
                            UpdateReport = $"{FindResourceString("NewVersionFound")}{FindResourceString("Comma")}{latestVersion}";

                            Application.Current.Dispatcher.Invoke(() => {
                                if (CDFCMessageBox.Show($"{FindResourceString("ConfirmToUpdate")}",
                                    FindResourceString("Tip"),
                                    MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                    if(ConfigState.EtrType == EntranceType.CPAndMultiMedia) {
                                        if(ConfigState.EnWay == EncryptWay.Dongle) {
                                            Process.Start("CDFCVideoExactorUpdater.exe",$"{nameof(EntranceType.CPAndMultiMedia)} All Dongle");
                                        }
                                        else {

                                        }
                                    }
                                    else {

                                    }
                                    Process.Start("CDFCVideoExactorUpdater.exe", ConfigState.SingleType.ToString());
                                    Environment.Exit(0);
                                }

                            });
                        }
                        else {
                            UpdateReport = FindResourceString("VersionUpToDate");
                            IsUpToDate = true;
                        }
                    }
                    catch(Exception ex) {
                        Logger.WriteLine($"{nameof(AboutInfoWindowViewModel)}->{nameof(CheckForNewVersionCommand)}:{ex.Message}");
                        Application.Current.Dispatcher.Invoke(() => {
                            CDFCMessageBox.Show($"{ex.Message}");
                        });
                    }
                    finally {
                        IsCheckingNewVersion = false;
                    }
                    
                    
                });
            }));

        private string updateReport;
        public string UpdateReport {
            get {
                return updateReport;
            }
            set {
                SetProperty(ref updateReport, value);
            }
        }

        private bool isUpToDate;
        public bool IsUpToDate {
            get {
                return isUpToDate;
            }
            set {
                SetProperty(ref isUpToDate, value);
            }
        }

        private bool closed;
        private DelegateCommand<CancelEventArgs> closingCommand;
        public DelegateCommand<CancelEventArgs> ClosingCommand =>
            closingCommand ??
            (closingCommand = new DelegateCommand<CancelEventArgs>(e => {
                if (IsCheckingNewVersion) {
                    if(CDFCMessageBox.Show(FindResourceString("ConfirmToStopCheck"),
                        MessageBoxButton.YesNo)  == MessageBoxResult.No) {
                        e.Cancel = true;
                    }
                }
                closed = true;
            }));
    }
 
}
