﻿using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Interfaces;
using System;
using System.Windows;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCVideoExactor.ViewModels {
    public partial class ScanningInfoDialogWindowViewModel:ViewModelBase {
        /// <summary>
        /// 扫描控制器;
        /// </summary>
        private readonly IScanningController iScanningController;
        /// <summary>
        /// 扫描对话框的构造方法;
        /// </summary>
        /// <param name="iScanningController">扫描控制器接口</param>
        public ScanningInfoDialogWindowViewModel(IScanningController iScanningController) : base(0) {
            if(iScanningController == null) {
                EventLogger.Logger.WriteLine("ScanningInfoDialogWindowViewModel初始化出错:iScnningController不可为空");
                throw new NullReferenceException("iScanningController出错");
            }
            this.iScanningController = iScanningController;
        }
    }

    /// <summary>
    /// 扫描对话框的状态;
    /// </summary>
    public partial class ScanningInfoDialogWindowViewModel {
        //窗体顶部的词，表示当前状态;
        private string titleWords = FindResourceString("Scanning");
        public string TitleWords {
            get {
                return titleWords;
            }
            set {
                titleWords = value;
                NotifyPropertyChanging(nameof(TitleWords));
            }
        }

        /// <summary>
        /// 窗体是否可用？用于关闭窗体;
        /// </summary>
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
        /// 是否处于扫描状态;
        /// </summary>
        public bool IsScanning {
            get {
                return iScanningController.IsScanning;
            }
            set {
                NotifyPropertyChanging(nameof(IsScanning));
            }
        }

        /// <summary>
        /// 当前完成的百分比;
        /// </summary>
        private byte curPercentage;
        public byte CurPercentage {
            get {
                return curPercentage;
            }
            set {
                curPercentage = value;
                NotifyPropertyChanging(nameof(CurPercentage));
            }
        }
        
        /// <summary>
        /// 已经扫描到文件总数;
        /// </summary>
        private int fileCount;
        public int FileCount {
            get {
                return fileCount;
            }
            set {
                fileCount = value;
                NotifyPropertyChanging(nameof(FileCount));
            }
        }

        /// <summary>
        /// 当前已经扫描的扇区数;
        /// </summary>
        private long curSectorCount;
        public long CurSectorCount {
            get {
                return curSectorCount;
            }
            set {
                curSectorCount = value;
                NotifyPropertyChanging(nameof(CurSectorCount));
                NotifyPropertyChanging(nameof(SectorState));
            }
        }

        /// <summary>
        /// 当前扇区扫描进度
        /// </summary>
        public string SectorState {
            get {
                return curSectorCount + "/" + TotalSectorCount;
            }
        }

        /// <summary>
        /// 结束时间;
        /// </summary>
        private DateTime? endDate;
        public DateTime? EndDate {
            get {
                return endDate;
            }
            set {
                endDate = value;
                NotifyPropertyChanging(nameof(EndDate));
            }
        }

        //已用时间;
        private TimeSpan elapsedTime = TimeSpan.Zero;
        public TimeSpan ElapsedTime {
            get {
                return elapsedTime;
            }
            set {
                elapsedTime = value;
                NotifyPropertyChanging(nameof(ElapsedTime));
            }
        }

        /// <summary>
        /// 总扇区数目;
        /// </summary>
        public long TotalSectorCount { get; set; }

        //扫描速度;
        private long speed;
        public long Speed {
            get {
                return speed;
            }
            set {
                speed = value;
                NotifyPropertyChanging(nameof(Speed));
            }
        }

        //预计剩余时间;
        private TimeSpan remainingTime = TimeSpan.Zero;
        public TimeSpan RemainingTime {
            get {
                return remainingTime;
            }
            set {
                remainingTime = value;
                NotifyPropertyChanging(nameof(RemainingTime));
            }
        }
        
        private long totalFileSize;
        public long TotalFileSize {
            get {
                return totalFileSize;
            }
            set {
                totalFileSize = value;
                NotifyPropertyChanging(nameof(TotalFileSize));
            }
        }
        /// <summary>
        /// 扫描开始时间;
        /// </summary>
        public DateTime? StartDate { get; set; }
    }
    
    /// <summary>
    /// 扫描对话模型的命令绑定项;
    /// </summary>
    public partial class ScanningInfoDialogWindowViewModel {
        #region 终止扫描的命令;
        private RelayCommand stopScanCommand;
        public RelayCommand StopScanCommand {
            get {
                return stopScanCommand ??
                    (stopScanCommand = new RelayCommand(StopScanExecute));
            }
        }
        /// <summary>
        /// 终止扫描所执行的动作;
        /// </summary>
        private void StopScanExecute() {
            if(CDFCMessageBox.Show(FindResourceString("ConfirmToStopScan"),  MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                return;
            }
            try { 
                iScanningController.Stop();
            }
            catch (Exception ex){
                EventLogger.Logger.WriteLine("扫描终止出错:" + ex.Message);
            }
        }
        #endregion

        #region 确定的命令;用于关闭当前窗体;

        private RelayCommand confirmCommand;
        public RelayCommand ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand = new RelayCommand(ConfirmExecuted));
            }
        }

        private void ConfirmExecuted() {
            if (iScanningController.IsScanning) {
                EventLogger.Logger.WriteLine("ScanningInfoDialogWindowViewModel出错:ConfirmExecuted,对象仍在扫描中");
            }
            IsEnabled = false;
        }
        #endregion

    }

}
