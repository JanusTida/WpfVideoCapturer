using CDFCEntities.DeviceObjects;
using CDFCLogger;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using CDFCVideoExactor.Models;
using CDFCVideoExactor.Abstracts;
using System.Windows.Input;
using CDFCEntities.Enums;
using System.Threading;
using System.Windows.Threading;
using System.Linq;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.Controllers;
using CDFCVideoExactor.Enums;
using CDFCEntities.Interfaces;
using CDFCMessageBoxes.MessageBoxes;
using CDFCEntities.Scanners;
using CDFCMessageBoxes.Models;
using CDFCLogger.Models;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.MessageBoxes;
using CDFCVideoExactor.Helpers;
using static CDFCCultures.Managers.ManagerLocator;
using CDFCSetting;
using CDFCVideoExactorUpdater.Helpers;
using EventLogger;
using System.Diagnostics;
using CDFCVideoExactor.Commands;

namespace CDFCVideoExactor.ViewModels {
    public partial class MainWindowViewModel : ViewModelBase {

        public string IconSource {
            get {
                if(ConfigState.EtrType == EntranceType.CapturerSingle) {
                    if(ConfigState.SingleType != DeviceTypeEnum.Unknown) {
                        return $"../Images/Icons/BlackHole_{ConfigState.SingleType}.png";
                    }
                }
                return "../Images/Icons/BlackIcon.ico";
            }
        }

        //已经过的页面的模型;
        private List<ViewModelBase> pastedPageViewModels;

        //是否可以保存日志;
        public bool CanSaveLog { get; set; }
        //设备信息;
        public ComObject ComObject { get; private set; }

        //主窗体UI调用器;
        public Action<Action> UpdateInvoker { get; private set; }

        //当前所使用的扫描器;
        public IScanner Scanner { get; set; }

        /// <summary>
        /// 窗体的构造方法;
        /// </summary>
        /// <param name="dispatcher">UI线程调度器，安全地使子线程访问UI</param>
        public MainWindowViewModel(Dispatcher dispatcher) {
            #region 读取设备信息;
            try {
                LoadInfo();
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("MainWindowViewModel构造方法出错!");
                throw ex;
            }
            #endregion

            UpdateInvoker = dispatcher.Invoke;
        }

        /// <summary>
        /// 加载设备基本信息;
        /// </summary>
        private void LoadInfo() {
            try {
                var comObject = ComObject.LocalObject;
                if (comObject.Devices.Count == 0) {
                    EventLogger.Logger.WriteLine("未获取设备基本信息");
                    throw new Exception("未获取到设备信息");
                }
                ComObject = comObject;
                EventLogger.Logger.WriteLine("加载设备基本信息结束");
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("MainWindowViewModel" + ex.Source + ":" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// 重新加载信息;
        /// </summary>
        public void ReloadInfo() {
            if (ComObject != null) {
                try {
                    //退出对象;
                    ComObject.Exit();
                    //重载本地对象;
                    ComObject = ComObject.LocalObject;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("MainWindowViewModel->RelaodInfo对象退出错误。" + ex.Message);
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// 主窗体分部视图的模型;
    /// </summary>
    public partial class MainWindowViewModel {
        public ViewModelBase FirstPageModel {
            get {
                if(ConfigState.EtrType == EntranceType.CPAndMultiMedia) {
                    return HomePageViewModel;
                }
                else {
                    return VideoObjectSelectorPageViewModel;
                }
            }
        }
         
        /// <summary>
        /// 当前主页面的模型;
        /// </summary>
        private ViewModelBase curPageViewModel;
        public ViewModelBase CurPageViewModel {
            get {
                //若当前页为空，则设置其为首页;
                if (curPageViewModel == null) {
                    curPageViewModel = FirstPageModel;
                    //VideoItemListViewerPageViewModel;
                    //HomePageViewModel; //
                }
                //若页面轨迹为空,则创建一个页面记录列表;
                if (pastedPageViewModels == null) {
                    pastedPageViewModels = new List<ViewModelBase>();
                    pastedPageViewModels.Add(HomePageViewModel);
                }
                return curPageViewModel;
            }
            set {
                //若当前轨迹页面为空,则初始化之;
                if (pastedPageViewModels == null) {
                    pastedPageViewModels = new List<ViewModelBase>();
                    pastedPageViewModels.Add(HomePageViewModel);
                }
                else if (pastedPageViewModels.Count == 0) {
                    pastedPageViewModels.Add(HomePageViewModel);
                }

                if(value == curPageViewModel) {
                    return;
                }

                //获得上一级的轨迹页面;
                var lastPageViewModel = pastedPageViewModels.LastOrDefault();
                if (lastPageViewModel != null) {
                    //若目标页面等级大于上一个页面，则将当前页面加入轨迹页面列表中;
                    if (lastPageViewModel.PageLevel < value.PageLevel) {
                        pastedPageViewModels.Add(curPageViewModel);
                    }
                    //否则可能为回退，首页操作;
                    else {
                        pastedPageViewModels.Remove(lastPageViewModel);
                    }
                }
                
                curPageViewModel = value;

                if (curPageViewModel == homePageViewModel) {
                    CurTopPagePartialPanelViewModel = HomePageTopPartialPanelViewModel;
                }
                else if (curPageViewModel == videoObjectSelectorPageViewModel) {
                    CurTopPagePartialPanelViewModel = VideoObjectSelectorPageTopPartialPanelViewModel;
                }
                else if (curPageViewModel == deviceSelectorPageViewModel) {
                    CurTopPagePartialPanelViewModel = DeviceSelectorTopPartialPanelViewModel;
                }
                else if (curPageViewModel == videoItemListViewerPageViewModel) {
                    CurTopPagePartialPanelViewModel = VideoItemListViewerPageTopPartialPanelViewModel;
                }
                else if(curPageViewModel == PrimaryObjectScanSettingPageViewModel) {
                    CurTopPagePartialPanelViewModel = PrimaryObjectScanSettingTopPartialPanelViewModel;
                }
                else if(curPageViewModel == MultiMediaPrimaryObjectScanSettingPageViewModel) {
                    CurTopPagePartialPanelViewModel = PrimaryObjectScanSettingTopPartialPanelViewModel;
                }
                else if(curPageViewModel == cpAndMPrimarySettingViewModel) {
                    CurTopPagePartialPanelViewModel = PrimaryObjectScanSettingTopPartialPanelViewModel;
                }
                else {
                    CurTopPagePartialPanelViewModel = BlankPageViewModel;
                }

                NotifyPropertyChanging(nameof(CurPageLevel));
                NotifyPropertyChanging(nameof(CurPageViewModel));
                NotifyPropertyChanging(nameof(IsHomePage));
                NotifyPropertyChanging(nameof(IsRegisterNeed));
                NotifyPropertyChanging(nameof(IsAboutNeed));
            }
        }

        

        public byte CurPageLevel {
            get {
                var curPage = CurPageViewModel;
                return curPage != null ? curPage.PageLevel :(byte)1;
            }
        }

        //首页的视图模型;
        private HomePageViewModel homePageViewModel;
        public HomePageViewModel HomePageViewModel {
            get {
                return homePageViewModel ??
                    (homePageViewModel = new HomePageViewModel(this));
            }
            private set {
                homePageViewModel = value;
            }
        }


        //视频监控初始页模型;
        private VideoObjectSelectorPageViewModel videoObjectSelectorPageViewModel;
        public VideoObjectSelectorPageViewModel VideoObjectSelectorPageViewModel {
            get {
                return videoObjectSelectorPageViewModel ??
                    (videoObjectSelectorPageViewModel = new VideoObjectSelectorPageViewModel(this));
            }
        }

        //空白页
        private BlankPageViewModel blankPageViewModel;
        public BlankPageViewModel BlankPageViewModel {
            get {
                return blankPageViewModel ??
                    (blankPageViewModel = new BlankPageViewModel());
            }
        }

        //磁盘，分区选择器模型;
        public DeviceSelectorPageViewModel deviceSelectorPageViewModel;
        public DeviceSelectorPageViewModel DeviceSelectorPageViewModel {
            get {
                return deviceSelectorPageViewModel ??
                    (deviceSelectorPageViewModel = new DeviceSelectorPageViewModel( this));
            }
        }

        //对象初级设定页面模型;
        private PrimaryObjectScanSettingPageViewModel primaryObjectScanSettingPageViewModel;
        public PrimaryObjectScanSettingPageViewModel PrimaryObjectScanSettingPageViewModel {
            get {
                return primaryObjectScanSettingPageViewModel ??
                   (primaryObjectScanSettingPageViewModel = new PrimaryObjectScanSettingPageViewModel(this));
            }
        }

        //对象初级设定页面模型;(多媒体)
        private MultiMediaPrimaryObjectScanSettingPageViewModel multiMediaPrimaryObjectScanSettingPageViewModel;
        public MultiMediaPrimaryObjectScanSettingPageViewModel MultiMediaPrimaryObjectScanSettingPageViewModel {
            get {
                return multiMediaPrimaryObjectScanSettingPageViewModel ??
                    (multiMediaPrimaryObjectScanSettingPageViewModel = new MultiMediaPrimaryObjectScanSettingPageViewModel(this));
            }
        }

        private CPAndMPrimarySettingViewModel cpAndMPrimarySettingViewModel;
        public CPAndMPrimarySettingViewModel CPAndMPrimarySettingViewModel {
            get {
                return cpAndMPrimarySettingViewModel ??
                    (cpAndMPrimarySettingViewModel = new CPAndMPrimarySettingViewModel(this));
            }
        }

        //文件显示页的页面模型;
        private VideoItemListViewerPageViewModel videoItemListViewerPageViewModel;
        public VideoItemListViewerPageViewModel VideoItemListViewerPageViewModel {
            get {
                return videoItemListViewerPageViewModel ??
                    (videoItemListViewerPageViewModel = new VideoItemListViewerPageViewModel(this));
            }
        }

    }

    /// <summary>
    /// 主窗体附加分部视图模型;
    /// </summary>
    public partial class MainWindowViewModel {
        /// <summary>
        /// 当前顶部页的模型;
        /// </summary>
        private ViewModelBase curTopPartialPanelViewModel;
        public ViewModelBase CurTopPagePartialPanelViewModel {
            get {
                return curTopPartialPanelViewModel ??
                    (curTopPartialPanelViewModel = HomePageTopPartialPanelViewModel);
                    //);
                //VideoItemListViewerPageTopPartialPanelViewModel););
            }
            private set {
                curTopPartialPanelViewModel = value;
                NotifyPropertyChanging(nameof(CurTopPagePartialPanelViewModel));
                NotifyPropertyChanging(nameof(IsHomePage));
                NotifyPropertyChanging(nameof(IsNotHomePage));
                //NotifyPropertyChanging(nameof(CurPageViewModelLevel));
            }

        }
        
        /// <summary>
        /// 当前页面级别;
        /// </summary>
        //public byte CurPageViewModelLevel {
        //    get {
        //        return CurPageViewModel.PageLevel;
        //    }
        //}

        //首页所呈现的顶部视图模型；
        private HomePageTopPartialPanelViewModel homePageTopPartialPanelViewModel;
        public HomePageTopPartialPanelViewModel HomePageTopPartialPanelViewModel {
            get {
                return homePageTopPartialPanelViewModel ??
                    (homePageTopPartialPanelViewModel = new HomePageTopPartialPanelViewModel());
            }
        }

        //对象选择页所呈现的顶部视图模型;
        private VideoObjectSelectorPageTopPartialPanelViewModel videoObjectSelectorPageTopPartialPanelViewModel;
        public VideoObjectSelectorPageTopPartialPanelViewModel VideoObjectSelectorPageTopPartialPanelViewModel {
            get {
                return videoObjectSelectorPageTopPartialPanelViewModel ??
                    (videoObjectSelectorPageTopPartialPanelViewModel = new VideoObjectSelectorPageTopPartialPanelViewModel());
            }
        }

        //磁盘选择页所呈现的顶部视图模型;
        private DeviceSelectorTopPartialPanelViewModel deviceSelectorTopPartialPanelViewModel;
        public DeviceSelectorTopPartialPanelViewModel DeviceSelectorTopPartialPanelViewModel {
            get {
                return deviceSelectorTopPartialPanelViewModel ??
                    (deviceSelectorTopPartialPanelViewModel = new DeviceSelectorTopPartialPanelViewModel(this));
            }
        }

        //文件结果列表的顶部视图模型;
        private VideoItemListViewerPageTopPartialPanelViewModel videoItemListViewerPageTopPartialPanelViewModel;
        public VideoItemListViewerPageTopPartialPanelViewModel VideoItemListViewerPageTopPartialPanelViewModel {
            get {
                return videoItemListViewerPageTopPartialPanelViewModel ??
                    (videoItemListViewerPageTopPartialPanelViewModel = new VideoItemListViewerPageTopPartialPanelViewModel(this));
            }
        }

        private PrimaryObjectScanSettingTopPartialPanelViewModel primaryObjectScanSettingTopPartialPanelViewModel;
        public PrimaryObjectScanSettingTopPartialPanelViewModel PrimaryObjectScanSettingTopPartialPanelViewModel {
            get {
                return primaryObjectScanSettingTopPartialPanelViewModel ??
                     (primaryObjectScanSettingTopPartialPanelViewModel = new PrimaryObjectScanSettingTopPartialPanelViewModel(this));
            }
        }
    }

    /// <summary>
    /// 主窗体的视图状态;
    /// </summary>
    public partial class MainWindowViewModel {
        /// <summary>
        ///是否正在加载;
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

        //private const string capturerSystemDes = "黑洞监控视频取证分析系统 4.1.0.0";
        //private const string multimediaSystemDes = "黑洞多媒体视频取证分析系统 4.1.0.0";
        //private const string cpAndMSystemDes = "黑洞视频取证分析系统 4.1.0.0";

        /// <summary>
        /// 标题栏文字;
        /// </summary>
        private string titleWord;
        public string TitleWord {
            get {
                Func<string> entranceString = () => {
                    return ConfigState.SoftName;
                };
                return $"{(!string.IsNullOrEmpty(titleWord)?titleWord+"-":(string.Empty))}{entranceString()}";
            }
            set {
                titleWord = value;
                NotifyPropertyChanging(nameof(TitleWord));
            }
        }
        
        /// <summary>
        /// 窗体的转换状态;
        /// </summary>
        private Models.WindowState windowState;
        public Models.WindowState WindowState {
            get {
                return windowState;
            }
            set {
                windowState = value;
                NotifyPropertyChanging(nameof(WindowState));
            }
        }

        //窗体是否可用，用于窗体的关闭状态;
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

        /// <summary>
        /// 当前选定的入口类型;
        /// </summary>
        public EntranceType SelectedEntranceType { get; set; }
        
        public DeviceTypeEnum SingleType { get; set; }

        public bool IsHomePage => CurPageViewModel == FirstPageModel;

        public bool IsNotHomePage => !IsHomePage;

        public CaseWriter CaseWriter { get; private set; }

        public bool IsRegisterNeed => IsHomePage && ( ConfigState.EnWay == EncryptWay.SoftKey);
        public bool IsAboutNeed => IsHomePage && (ConfigState.EnWay == EncryptWay.Dongle);
    }

    /// <summary>
    /// 主窗体的命令绑定项;(与业务逻辑相关)
    /// </summary>
    public partial class MainWindowViewModel {
        #region 返回至上一页
        /// <summary>
        /// 主窗体回退页面的命令动作;
        /// </summary>
        private RelayCommand backSpacePageCommand;
        public RelayCommand BackSpacePageCommand {
            get {
                if (backSpacePageCommand == null) {
                    backSpacePageCommand = new RelayCommand(
                        () => {
                            if (!ConfirmClear) {
                                return;
                            }
                            var lastPageViewModel = pastedPageViewModels.LastOrDefault();
                            if (lastPageViewModel != null) {
                                CurPageViewModel = lastPageViewModel;
                                pastedPageViewModels.Remove(lastPageViewModel);
                            }
                        },
                        () => 
                        CurPageViewModel != FirstPageModel);
                }
                return backSpacePageCommand;
            }
        }

        #endregion

        #region 回退至首页
        /// <summary>
        /// 回退至首页的命令;
        /// </summary>
        private RelayCommand goHomePageCommand;
        public RelayCommand GoHomePageCommand {
            get {
                if (goHomePageCommand == null) {
                    goHomePageCommand = new RelayCommand(GoHomePageExecuted, () => CurPageViewModel != FirstPageModel);
                }
                return goHomePageCommand;
            }
        }
        /// <summary>
        /// 回退至首页的方法;
        /// </summary>
        private void GoHomePageExecuted() {
            if (!ConfirmClear) {
                return;
            }
            pastedPageViewModels.Clear();
            pastedPageViewModels.Add(VideoObjectSelectorPageViewModel);
            //pastedPageViewModels.Add(HomePageViewModel);
            //CurPageViewModel = HomePageViewModel;
            CurPageViewModel = FirstPageModel;
        }

        #endregion 回退至首页

        //检查当前页是否为文件列表页，并询问确定退出;
        private bool ConfirmClear {
            get {
                if (CurPageViewModel == VideoItemListViewerPageViewModel && VideoItemListViewerPageViewModel.ActualRows.Count != 0) {
                    if (CDFCMessageBox.Show(FindResourceString("ConfirmToCoverResult"), MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                        VideoItemListViewerPageViewModel.Exit();
                        videoItemListViewerPageTopPartialPanelViewModel.Exit();
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                return true;
            }
        }

        #region 加载镜像文件;
        /// <summary>
        /// 加载镜像文件的命令;
        /// </summary>
        private RelayCommand loadImgFileCommand;
        public RelayCommand LoadImgFileCommand {
            get {
                return loadImgFileCommand ??
                    (loadImgFileCommand = new RelayCommand(LoadImgFileExecuted, LoadImgFileCanExecute));
            }
        }

        private bool LoadImgFileCanExecute() {
            return CurPageViewModel == HomePageViewModel && !IsLoading;
        }
        public void LoadImgFileExecuted() {
            Logger.WriteLine($"{nameof(MainWindowViewModel)}:{nameof(LoadImgFileExecuted)}");
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = "(支持的镜像文件)|*.img;*.dd;*.raw|(所有文件)|*.*";
            dialog.Title = "打开镜像文件";
            dialog.Multiselect = false;
            dialog.ValidateNames = true;
            dialog.ReadOnlyChecked = false;
            
            Logger.WriteLine($"{nameof(MainWindowViewModel)}:{nameof(LoadImgFileExecuted)}2");
            
            if (dialog.ShowDialog() == true && dialog.CheckFileExists) {
                Logger.WriteLine($"{nameof(MainWindowViewModel)}:{nameof(LoadImgFileExecuted)}3");
                //检查是否已经打开过此文件;
                var existingFile = ComObject.ImgFiles.FirstOrDefault(p => p.Path == dialog.FileName);
                #region 判定目标页;
                PrimaryObjectScanSettingPageViewModel targetPageViewModel = null;
                switch (SelectedEntranceType) {
                    case EntranceType.MultiMedia:
                        targetPageViewModel = MultiMediaPrimaryObjectScanSettingPageViewModel;
                        break;
                    case EntranceType.CPAndMultiMedia:
                        targetPageViewModel = CPAndMPrimarySettingViewModel;
                        break;
                    default:
                        targetPageViewModel = PrimaryObjectScanSettingPageViewModel;
                        break;
                }
                #endregion

                if (existingFile != null) {
                    //重置设定对象;
                    var objectScanSetting = new ObjectScanSetting(existingFile, SelectedEntranceType,SingleType);
                    targetPageViewModel.ObjectScanSetting = objectScanSetting;
                    CurPageViewModel = targetPageViewModel;
                }   
                else {
                    this.IsLoading = true;
                    var errMsg = string.Empty;
                    //部署后台工作器;
                    BackgroundWorker worker = new BackgroundWorker();
                    //后台加载
                    worker.DoWork += (sender, e) => {
                        //获得镜像文件对象;
                        try {
                            ImgFile imgFile = ImgFile.GetImgFile(dialog.FileName);
                            ComObject.ImgFiles.Add(imgFile);
                            //重置设定对象;
                            var objectScanSetting = new ObjectScanSetting(imgFile, SelectedEntranceType, SingleType);
                            targetPageViewModel.ObjectScanSetting = objectScanSetting;
                            e.Result = true;
                        }
                        catch(Exception ex) {
                            errMsg = ex.Message;
                            e.Result = false;
                        }
                    };

                    //加载完毕后跳转页面;
                    worker.RunWorkerCompleted += (sender, e) => {
                        if ((bool)e.Result) {
                            CurPageViewModel = targetPageViewModel;
                        }
                        else {
                            CDFCMessageBox.Show($"{FindResourceString("ErrorWhileLoadingImg")}:{errMsg}");
                        }

                        IsLoading = false;
                    };

                    worker.RunWorkerAsync();
                }
            }
            Logger.WriteLine($"{nameof(MainWindowViewModel)}:{nameof(LoadImgFileExecuted)}4");
        }
        #endregion

        #region 高级扫描设定；
        private RelayCommand seniorObjectSettingCommand;
        public RelayCommand SeniorObjectSettingCommand {
            get {
                return seniorObjectSettingCommand ??
                    (seniorObjectSettingCommand = new RelayCommand(SeniorObjectScanSettingExecuted, SeniorObjectScanSettingCanExecute));
            }
        }
        //高级设定是否可以执行;
        private bool SeniorObjectScanSettingCanExecute() {
            return CurPageViewModel == PrimaryObjectScanSettingPageViewModel;
        }
        private void SeniorObjectScanSettingExecuted() {
            try {
                ISeniorScanSettingController controller = new SeniorScanSettingController(this);
                controller.Start();
            }
            catch (Exception ex) {
                CDFCMessageBox.Show(FindResourceString("UnknownErrorEncountered") + ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// 创建案件的命令;s
        /// </summary>
        private RelayCommand createCaseCommand;
        public RelayCommand CreateCaseCommand {
            get {
                return createCaseCommand ??
                    (createCaseCommand =
                        new RelayCommand(
                            () => {
                                var loggerCase = CreateCaseMessageBox.Show();
                                if(loggerCase != null) {
                                    try { 
                                        CaseWriter writer = new CaseWriter(loggerCase);
                                        CaseWriter.WriteRecentcase(loggerCase);
                                        this.CaseWriter = writer;
                                        this.TitleWord = loggerCase.Name;
                                        string form = FindResourceString("CaseCreateSucceedFormat");
                                        if(form != null) {
                                            CDFCMessageBox.Show(string.Format(form,loggerCase.Name));
                                        }
                                        else {
                                            CDFCMessageBox.Show("Failed To Get Format!");
                                        }
                                        

                                    }
                                    catch(Exception ex) {
                                        EventLogger.CaseLogger.WriteLine("MainWindoViewModel->CreateCaseCommand错误:" + ex.Message);
                                        var form = FindResourceString("CaseCreateFailedFormat");
                                        if(form != null) {
                                            try {
                                                CDFCMessageBox.Show(string.Format(form,loggerCase.Name) + ex.Message + FindResourceString("ContactVendor"));
                                            }
                                            catch(Exception iex) {
                                                CDFCMessageBox.Show($"Failed To Parse Format:{iex.Message}");
                                            }
                                        }
                                        
                                    }
                                }
                            },
                            () => {
                                return CurPageViewModel != null?
                                PageLevel == 0:false;
                            }
                        )
                    );
            }
        }

        /// <summary>
        /// 打开案件的命令;
        /// </summary>
        private RelayCommand openCaseCommand;
        public RelayCommand OpenCaseCommand {
            get {
                return openCaseCommand ??
                   (openCaseCommand = new RelayCommand(
                       () => {
                           VistaOpenFileDialog dialog = new VistaOpenFileDialog();
                           dialog.Filter = $"{FindResourceString("CaseFile")}(*.bhproj)|*.bhproj";
                           dialog.Multiselect = false;

                           if (dialog.ShowDialog() == true) {
                               IsLoading = true;
                               var index = dialog.FileName.LastIndexOf("\\");
                               var path = dialog.FileName.Substring(0, index);
                               CaseReader reader = new CaseReader(dialog.FileName);
                               

                               ThreadPool.QueueUserWorkItem(callBack => {
                                   OpenCase(reader.Case, path);
                                   IsLoading = false;
                               });
                               
                           }
                       }
                    ));
            }
        }

        private void OpenCase(LoggerCase pastCase,string path) {
            try {
                var reader = new CaseReader(path + "/" + "case.bhproj");
                var records = reader.Records;
                if (records == null || records.Count() == 0) {
                    UpdateInvoker.Invoke(() => {
                        IsLoading = false;
                        if (CDFCMessageBox.Show(FindResourceString("ConfirmToLoadCaseWhileNull"), 
                            FindResourceString("RecordNotFound"),
                            MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                            TitleWord = pastCase.Name;
                            var caseWriter = new CaseWriter(pastCase);
                            this.CaseWriter = caseWriter;
                        }
                    });
                }
                else {
                    UpdateInvoker.Invoke(() => {
                        var caseWriter = new CaseWriter(pastCase);
                        this.CaseWriter = caseWriter;
                        this.TitleWord = pastCase.Name;
                        IsLoading = false;
                        if (CDFCMessageBox.Show(FindResourceString("ConfirmToLoadLog"),
                            FindResourceString("LogFound"), MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                            var recordsItems = records.Select(p => new ListViewItemModel { TitleWord = p.Path });
                            var selectedItem = OpenCaseMessageBox.Show(recordsItems.ToList());
                            if (selectedItem == null) {
                                return;
                            }
                            
                            var record = records.FirstOrDefault(p => p.Path == selectedItem.TitleWord);
                            if (record == null) {
                                EventLogger.CaseLogger.WriteLine("MainWindowViewModel->读取案件错误:案件为空!");
                                CDFCMessageBox.Show(FindResourceString("UnknownErrorWhileLoadCase"));
                                return;
                            }
                            var recordItem = path + "/" + record.Path;

                            var setting = reader.GetSetting(record.Path);

                            if (setting == null) {
                                CDFCMessageBox.Show(FindResourceString("FailedToGetRelaventSetting"));
                                return;
                            }

                           

                            IObjectDevice iObjectDevice = null;

                            switch ((DriveType)setting.DriveType) {
                                case DriveType.PhysicalDevice:
                                    iObjectDevice = ComObject.Devices.FirstOrDefault(
                                        p => p.SerialNumber == setting.SerialNumber
                                        );
                                    if(iObjectDevice == null) {
                                        CDFCMessageBox.Show(FindResourceString("ObjectNotFound"));
                                        return;
                                    }
                                    break;
                                case DriveType.Disk:
                                    var device = ComObject.Devices.FirstOrDefault(p => 
                                        p.SerialNumber == setting.SerialNumber);
                                    if(device == null) {
                                        CDFCMessageBox.Show(FindResourceString("ObjectNotFound"));
                                        return;
                                    }
                                    if(device != null) {
                                        iObjectDevice = device.Partitions.FirstOrDefault(p => p.Sign.ToString() == setting.Sign.ToString());
                                        if (iObjectDevice == null) {
                                            CDFCMessageBox.Show(FindResourceString("ObjectNotFound"));
                                            return;
                                        }
                                    }
                                    break;
                                case DriveType.ImgFile:
                                    try {
                                        var imgPath = setting.ImgPath;
                                        iObjectDevice = ImgFile.GetImgFile(imgPath);
                                    }
                                    catch(Exception ex) {
                                        EventLogger.CaseLogger.WriteLine("MainWindowViewModel->OpenCase:" + ex.Message);
                                        CDFCMessageBox.Show(FindResourceString("FailedToOpenImageFile"));
                                        return;
                                    }
                                    break;
                            }

                            var objectSetting = new ObjectScanSetting(iObjectDevice, EntranceType.Capturer,SingleType);
                            IScanner scanner = null;

                            try {
                                ScanSetting.DeviceType = (DeviceTypeEnum)setting.DeviceTypeEnum;
                                ScanSetting.ExtensionName = setting.ExtensionName;
                                ScanSetting.IsMP4Class = setting.IsMP4Class == 1;
                                ScanSetting.ScanMethod = (ScanMethod)setting.ScanMethod;
                                ScanSetting.SectorSize = setting.SectorSize;
                                ScanSetting.StartingTime = DateTime.Now.ToString().Replace('\\', '-').Replace(':', '-');
                                ScanSetting.VersionType = new VersionType((DeviceTypeEnum)setting.DeviceTypeEnum) {
                                    ID = setting.DeviceVersionType
                                };
                                ScanSetting.DeviceTypeInfo = new DeviceType { ID = 2, DeviceTypeEnum = (DeviceTypeEnum)setting.DeviceTypeEnum };


                                #region 根据不同的类型生成不同的扫描处理器;
                                switch ((DeviceTypeEnum)setting.DeviceTypeEnum) {
                                    #region 监控处理;
                                    case DeviceTypeEnum.AnLian:
                                        scanner = new AnLianScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.Canon:
                                        scanner = new CanonScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.ChuangZe:
                                        scanner = new ChuangZeScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.DaHua:
                                        scanner = new DHScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.H264:
                                    case DeviceTypeEnum.ZhongWei:
                                    case DeviceTypeEnum.XingKang:
                                        scanner = new MP4Scanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.HaiKang:
                                        scanner = new HKScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.HaiShiTai:
                                        scanner = new HSTScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.HaiSi:
                                        scanner = new HaiSiScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.HanBang:
                                        scanner = new HanBangScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.RuiShi:
                                        scanner = new RuiShiScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.YinTan:
                                        scanner = new YinTanScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.QiDun:
                                        scanner = new QiDunScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.XiaoMi:
                                        scanner = new XiaoMiScanner(iObjectDevice);
                                        break;
                                    #endregion
                                    case DeviceTypeEnum.Panasonic:
                                        scanner = new PanasonicScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.Sony:
                                        scanner = new SonyScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.WFS:
                                        scanner = new WFSScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.MOV:
                                        scanner = new MOVScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.JingYi:
                                        scanner = new JingYiScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.ShanLing:
                                        scanner = new ShanLingScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.UnknownCar:
                                        scanner = new UnknownCarScanner(iObjectDevice);
                                        break;
                                    case DeviceTypeEnum.WJCL:
                                        scanner = new WJCLScanner(iObjectDevice);
                                        break;
                                    

                                    default:
                                        EventLogger.Logger.WriteLine("MainWindowViewModel->OpenCase->Scanner构造错误:未注册该类型的扫描处理器:" + (DeviceTypeEnum)setting.DeviceTypeEnum);
                                        break;
                                }
                                #endregion


                            }
                            catch (Exception ex) {
                                EventLogger.CaseLogger.WriteLine("MainWindowViewModel->读取设定错误:" + ex.Message);
                                CDFCMessageBox.Show(FindResourceString("FailedToReadRecord") + ex.Message);
                            }

                            if (scanner != null) {
                                List<IntPtr> unManagedPtrs;
                                var fileList = reader.GetFileList(record.Path, iObjectDevice, out unManagedPtrs);
                                
                                scanner.LoadFileList(fileList, unManagedPtrs);
                                var categories = scanner.CurCategories;
                                CurPageViewModel = VideoItemListViewerPageViewModel;
                                IsLoading = true;
                                ThreadPool.QueueUserWorkItem(innerCallBack => {
                                    var rangeList = reader.RangeList(record.Path, iObjectDevice);
                                    var videoItemList = VideoItemListViewerPageViewModel;
                                    if(rangeList != null) {
                                        #region 进行文件校验
                                        bool? state = null;
                                        //是否需要跳出;
                                        bool broke = false;
                                        //之前碎片的状态;
                                        bool? oriState = null;

                                        //记录所有文件数量;
                                        var videoCount = categories.Sum(p => p.Videos.Count);
                                        
                                        categories.ForEach(p => {
                                            p.Videos.ForEach(q => {
                                                broke = false;
                                                var firstFrag = q.FileFragments.FirstOrDefault();
                                                oriState = firstFrag != null ? rangeList.GetRangeValuePosition(firstFrag.StartAddress, firstFrag.StartAddress + firstFrag.Size) : null;
                                                foreach (var frag in q.FileFragments) {
                                                    //138018816 217319936
                                                    state = rangeList.GetRangeValuePosition(frag.StartAddress, frag.StartAddress + frag.Size);
                                                    //若存在碎片为覆盖，则判定整个文件为覆盖;
                                                    if (state == null || state != oriState) {
                                                        q.Integrity = VideoIntegrity.Covered;
                                                        broke = true;
                                                    }
                                                    if (broke == true) {
                                                        break;
                                                    }
                                                }
                                                //若未跳出，则表示文件碎片状态完全一致;
                                                if (!broke) {
                                                    q.Integrity = state == true ? VideoIntegrity.Whole : VideoIntegrity.Deleted;
                                                }
                                            });
                                        });

                                        #endregion
                                    }
                                    if (videoItemList != null) {
                                        videoItemList.GetIObjectDevice(iObjectDevice);
                                        ScanningController scanCtr = new ScanningController(this, iObjectDevice, objectSetting,scanner);
                                        UpdateInvoker.Invoke(() => {
                                            try { 
                                                scanCtr.UpdateView(scanner.CurCategories);
                                                scanCtr.IniFilterViewModel(scanner.CurCategories);

                                            }
                                            catch(Exception ex) {
                                                EventLogger.CaseLogger.WriteLine("MainWindowViewModel->打开案件" + pastCase.Name + "错误:"+ex.Message);
                                            }
                                        });
                                    }
                                });
                                
                            }
                        }
                    });
                }
            }
            catch (Exception ex) {
                EventLogger.CaseLogger.WriteLine("MainWindowViewModel打开案件错误:" + ex.Message);
                UpdateInvoker.Invoke(() => {
                    IsLoading = false;
                    CDFCMessageBox.Show(FindResourceString("FailedToOpenRecord"));
                });
            }
        }
        /// <summary>
        /// 历史案件;
        /// </summary>
        private RelayCommand seeRecentCaseCommand;
        public RelayCommand SeeRecentCaseCommand {
            get {
                return seeRecentCaseCommand ??
                    (seeRecentCaseCommand = new RelayCommand(
                        () => {
                            var cases = CaseReader.RecentCases;
                            if(cases != null) {
                                if(cases.Count != 0) {
                                    var caseItems = cases.Select(p => new ListViewItemModel { TitleWord = p.Name }).ToList();
                                    var selectedItem = OpenCaseMessageBox.Show(caseItems);
                                    if(selectedItem == null) {
                                        return;
                                    }
                                    var selectedCase = cases.FirstOrDefault(p => p.Name == selectedItem.TitleWord);
                                    if(selectedCase == null) {
                                        CDFCMessageBox.Show(FindResourceString("FailedToOpenRecentFile"));
                                    }

                                    CaseReader reader = null;
                                    try {
                                        reader   = new CaseReader(selectedCase.Path + "/" + selectedCase.Name + "/" + "case.bhproj");
                                    }
                                    catch(Exception ex) {
                                        EventLogger.CaseLogger.WriteLine($"{nameof(MainWindowViewModel)}->{nameof(SeeRecentCaseCommand)}:" + ex.Message);
                                        CDFCMessageBox.Show(FindResourceString("FailedToOpenRecentRecord"));
                                        IsLoading = false;
                                        return;
                                    }
                                    IsLoading = true;
                                    ThreadPool.QueueUserWorkItem(callBack => {
                                        try { 
                                            OpenCase(reader.Case, reader.Case.Path+"/"+reader.Case.Name);
                                        }
                                        catch(Exception ex) {
                                            EventLogger.CaseLogger.WriteLine("MainWindowViewModel->OpenCase错误:" + ex.Message+ex.StackTrace);
                                            this.UpdateInvoker.Invoke(() => {
                                                CDFCMessageBox.Show(FindResourceString("NoRecordToCheck"));
                                            });
                                        }
                                        IsLoading = false;
                                    });

                                }
                                else {
                                    CDFCMessageBox.Show(FindResourceString("ErrorWhenCheckingRecent"));
                                }
                            }
                            else {
                                CDFCMessageBox.Show(FindResourceString("FailedToAquireRencentRecord"));
                            }
                        }
                        )
                    );
            }
        }

        private RelayCommand _loadedCommand;
        public RelayCommand LoadedCommand =>
            _loadedCommand ?? (_loadedCommand =
                new RelayCommand(
                    () => {
                        ThreadPool.QueueUserWorkItem(cb => {
                            try {
                                if (ConfigState.EnWay == EncryptWay.SoftKey && VersionHelper.LatestVersion != ConfigState.VersionString) {
                                    System.Windows.Application.Current.Dispatcher.Invoke(() => {
                                        if (CDFCMessageBox.Show($"{FindResourceString("ConfirmToUpdate")}",
                                            FindResourceString("Tip"),
                                            MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                            Process.Start("CDFCVideoExactorUpdater.exe", ConfigState.SingleType.ToString());
                                            Environment.Exit(0);
                                        }
                                    });
                                }
                                else {
                                    CDFCMessageBox.Show($"{FindResourceString("VersionUpToDate")}");
                                }
                            }
                            catch(Exception ex) {
                                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                                    Logger.WriteLine($"{nameof(MainWindowViewModel)}->{nameof(LoadedCommand)}:{ex.Message}");
                                    CDFCMessageBox.Show($"{FindResourceString("ErrorCheckingUpdate")}:{ex.Message}");
                                });
                            }
                        });
                        
                    }
                ));
        #region 退出命令;
        private RelayCommand exitCommand;
        public RelayCommand ExitCommand {
            get {
                return exitCommand ??
                    (exitCommand = new RelayCommand(() => {
                        if (CurPageViewModel == VideoItemListViewerPageViewModel) {
                            if (CDFCMessageBox.Show(FindResourceString("ConfitmToUnSaveResult"), MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                ComObject.Exit();
                                VideoItemListViewerPageViewModel.Exit();
                                IsEnabled = false;
                            }
                            else {
                                IsEnabled = true;
                            }
                        }
                        else {
                            IsEnabled = false;
                        }

                    }));
            }
        }

        #endregion

        #region 窗体关闭时的处理
        private DelegateCommand<CancelEventArgs> closingCommand;
        public DelegateCommand<CancelEventArgs> ClosingCommand {
            get {
                return closingCommand ??
                    (closingCommand = new DelegateCommand<CancelEventArgs>(e => {
                        MessageBoxResult result = MessageBoxResult.Yes;
                        if (CurPageViewModel == VideoItemListViewerPageViewModel) {
                            result = CDFCMessageBox.Show(FindResourceString("ConfitmToUnSaveResult"), MessageBoxButton.YesNo);
                        }
                        if (result == MessageBoxResult.Yes) {
                            try {
                                ComObject.Exit();
                                VideoItemListViewerPageViewModel.Exit();
                                Scanner?.Exit();
                            }
                            catch (Exception ex) {
                                EventLogger.Logger.WriteLine("MainWindowViewModel->ClosingCommandExecuted错误:" + ex.Message);
                            }
                        }
                        else {
                            IsEnabled = true;
                            e.Cancel = true;
                        }
                    }));
            }
        }
        #endregion
        

        #region 设备刷新;
        /// <summary>
        /// 设备是否能够进行刷新;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnRefreshCanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e) {
            e.CanExecute = WindowState == CDFCVideoExactor.Models.WindowState.Home;
            e.Handled = true;
        }
        /// <summary>
        /// 设备刷新命令执行;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnRefreshExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {
            IsLoading = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += LoadingLoggerWorker_DoWork;

            worker.RunWorkerCompleted += (innerWorker, arg) => {
                IsLoading = false;
            };
            worker.RunWorkerAsync();
        }
        #endregion

        

        #region 退出窗体;
        /// <summary>
        /// 退出时的动作;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MiExitCommand_Executed(object sender, ExecutedRoutedEventArgs e) {

            e.Handled = true;
        }
        #endregion



        #region 文件命令;
        /// <summary>
        /// 文件命令的菜单绑定;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MiFileExecuted(object sender, RoutedEventArgs e) {
            e.Handled = true;
        }

        #endregion

        #region 加载日志;
        /// <summary>
        /// 是否可以执行加载日志文件的动作;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MiLoadLoggerCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {

            e.Handled = true;
        }
        /// <summary>
        /// 加载日志的命令动作;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MiLoadLoggerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Filter = "(日志文件)|*.db";
            dialog.Title = "选择一个日志";
            dialog.Multiselect = false;

            e.Handled = true;
        }

        /// <summary>
        /// 日志与设备不匹配时所调用的方法;
        /// </summary>
        public void ReportNotMatchedError() {
            IsLoading = false;
            CDFCMessageBox.Show(FindResourceString("UnmatchedObjectAndLog"));
        }
        //选定的日志读取器;
        LoggerReader loggerReader;
        /// <summary>
        /// 加载日志的后台工作器;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoadingLoggerWorker_DoWork(object sender, DoWorkEventArgs e) {
            var deviceType = (DeviceTypeEnum)loggerReader.LoggerSetting.DeviceTypeEnum;
            var scanMethod = (ScanMethod)loggerReader.LoggerSetting.ScanMethod;
            CDFCSetting.ScanSetting.DeviceType = deviceType;
            CDFCSetting.ScanSetting.ScanMethod = scanMethod;
            CDFCSetting.ScanSetting.IsMP4Class = loggerReader.LoggerSetting.IsMP4Class == 1 ? true : false;
            //关闭计算对象;
            ComObject.Exit();
            //初始化对象;
            //重载计算对象;
            try {
                LoadInfo();
            }
            catch (Exception ex) {
                CDFCMessageBox.Show(FindResourceString("FailedToRefreshDevice") + ex.Message);
                EventLogger.Logger.WriteLine("MainWindowViewModel->Logger");
            }
        }
        /// <summary>
        /// 日志加载完成后将要进行的操作;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoadingLoggerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {

            IsLoading = false;
        }
        #endregion
    }

    /// <summary>
    /// 关于,注册窗体相关;
    /// </summary>
    public partial class MainWindowViewModel {
        //关于窗体命令;
        private RelayCommand aboutInfoCommand;
        public RelayCommand AboutInfoCommand =>
            aboutInfoCommand ?? (aboutInfoCommand = new RelayCommand(() => {
                AboutInfoMessageBox.Show();
            }));

        private RelayCommand aboutCommand;
        public RelayCommand AboutCommand =>
            aboutCommand ?? (aboutCommand = new RelayCommand(() => {
                AboutMessageBox.Show();
            }));
    }
}
