using CDFCConverters.Converters;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using System;
using System.Linq;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 文件列表顶部所需的视图;(过滤选项)
    /// </summary>
    public partial class VideoItemListViewerPageTopPartialPanelViewModel:ViewModelBase {
        private MainWindowViewModel mainWindowViewModel;
        //文件列表顶部视图模型构造方法;
        public VideoItemListViewerPageTopPartialPanelViewModel(MainWindowViewModel mainWindowViewModel) {
            if(mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("VideoItemListViewerPageTopParitialViewModel初始化出现错误:参数mainWindowViewModel不得为空!");
                throw new NullReferenceException("mainWindowViewModel can't be null!");
            }
            this.mainWindowViewModel = mainWindowViewModel;
        }
        
        //释放接口;
        public void Exit() {
            iniDate = null;
            endDate = null;
            minDate = null;
            maxDate = null;
            channelNums = null;
            channelNum = null;
            startAddress = 0;
            endAddress = 0;
            minSize = 0;
            maxSize = 0;
            
        }
    }

    /// <summary>
    /// 文件列表顶部所需视图模型的状态;
    /// </summary>
    public partial class VideoItemListViewerPageTopPartialPanelViewModel {
        //初始时间(筛选);
        private DateTime? iniDate;
        public DateTime? IniDate {
            get {
                return iniDate;
            }
            set {
                if (value.HasValue && IsDateFilterEnabled) {
                    value = value.Value.AddSeconds(1);
                    //检查起始时间是否比终止时间大;
                    if (endDate.HasValue&&value > endDate) {
                        iniDate = endDate.Value.AddDays(-1).AddSeconds(2);
                    }
                    //检查起始时间是否超出范围;
                    else if (value < minDate) {
                        iniDate = minDate;
                    }
                    else {
                        iniDate = value;
                    }
                    FilterItems();
                }
                NotifyPropertyChanging(nameof(IniDate));
            }
        }

        //终止时间(筛选);
        private DateTime? endDate;
        public DateTime? EndDate {
            get {
                return endDate;
            }
            set {
                if (value.HasValue && IsDateFilterEnabled ) {
                    value = value.Value.AddHours(24).AddSeconds(-1);
                    if (iniDate.HasValue && value < iniDate) {
                        endDate = iniDate.Value.AddDays(1).AddSeconds(-2);
                    }
                    else if (value > maxDate) {
                        endDate = maxDate;
                    }
                    else {
                        endDate = value;
                    }
                    FilterItems();
                }
                NotifyPropertyChanging(nameof(EndDate));
            }
        }
        
        //时间的有效范围;
        private DateTime? minDate;
        private DateTime? maxDate;

        //起始地址;(以GB为单位)
        private ulong startAddress;
        public ulong StartAddress {
            get {
                return startAddress;
            }
            set {
                if(value < 0) {
                    startAddress = 0;
                }
                else if (value > endAddress) {
                    startAddress = endAddress;
                }
                else { 
                    startAddress = value;
                }
//                FilterItems();
                NotifyPropertyChanging(nameof(StartAddress));
            }
        }

        //终止地址;(以GB为单位)
        private ulong endAddress;
        public ulong EndAddress {
            get {
                return endAddress;
            }
            set {
                if(value > maxAddress) {
                    endAddress = maxAddress;
                }
                else if(value < startAddress) {
                    endAddress = startAddress;
                }
                else {
                    endAddress = value;
                }
               // FilterItems();
                NotifyPropertyChanging(nameof(EndAddress));
            }
        }

        //最大地址;(以GB为单位)
        private ulong maxAddress;

        //最小大小;
        private ulong minSize;
        public ulong MinSize {
            get {
                return minSize;
            }
            set {
                if(value > maxSize) {
                    minSize = maxSize;
                }
                else {
                    minSize = value;
                }
                NotifyPropertyChanging(nameof(MinSize));
            }
        }

        //最大大小;
        private ulong maxSize;
        public ulong MaxSize {
            get {
                return maxSize;
            }
            set {
                if(value > limSize) {
                    maxSize = limSize;
                }
                else if(value < minSize) {
                    maxSize = minSize;
                }
                else {
                    maxSize = value;
                }
                //FilterItems();
                NotifyPropertyChanging(nameof(MaxSize));
            }
        }

        //最大大小限制;
        private ulong limSize;

        //通道号下拉参数;
        private ChannelNoModel[] channelNums;
        public ChannelNoModel[] ChannelNums {
            get {
                return channelNums;
            }
            set {
                channelNums = value;
                NotifyPropertyChanging(nameof(ChannelNums));
            }
        }

        //当前选定的通道;
        private ChannelNoModel channelNum;
        public ChannelNoModel ChannelNum {
            get {
                return channelNum;
            }
            set {
                if (IsChannelFilterEnabled) {
                    channelNum = value;
                    FilterItems();
                }
                NotifyPropertyChanging(nameof(ChannelNum));
            }
        }

        //时间是否可筛选情况;
        private bool IsDateFilterEnabled {
            get {
                return CDFCSetting.ScanSetting.VersionType.DateOk;
            }
        }

        //通道号筛选是否可用;
        public bool IsChannelFilterEnabled {
            get {
                if(mainWindowViewModel == null) {
                    EventLogger.Logger.WriteLine("VideoItemListViewerTopPartialViewModel->IsChannelFileterEnabled错误:主窗体模型为空!");
                    return false;
                }
                else {
                    return mainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer;
                }
            }
        }

        //地址筛选是否可用;
        private bool IsAddressFilterEnabled {
            get {
                return CDFCSetting.ScanSetting.DeviceType == CDFCEntities.Enums.DeviceTypeEnum.Canon;
            }
        }
    }

    /// <summary>
    /// 文件列表顶部所需视图模型的命令绑定项;
    /// </summary>
    public partial class VideoItemListViewerPageTopPartialPanelViewModel {
        /// <summary>
        /// 设定时间的有效范围;
        /// </summary>
        /// <param name="minDateNum">最小的时间数字</param>
        /// <param name="maxDateNum">最大的时间数字</param>
        public void SetDateRange(uint minDateNum, uint maxDateNum) {
            minDate = DateNumToDateStringConverter.ConvertToNullableDate(minDateNum);
            maxDate = DateNumToDateStringConverter.ConvertToNullableDate(maxDateNum);

            //将时间范围设置到整天临界值;
            minDate = minDate.Value.AddSeconds(-(minDate.Value.Hour * 3600 + minDate.Value.Minute * 60 + minDate.Value.Second - 1));
            maxDate = maxDate.Value.AddDays(1);
            maxDate = maxDate.Value.AddSeconds(-(maxDate.Value.Hour * 3600 + maxDate.Value.Minute * 60 + maxDate.Value.Second + 1));

            iniDate = minDate;
            endDate = maxDate;
            
            NotifyPropertyChanging(nameof(IniDate));
            NotifyPropertyChanging(nameof(EndDate));
        }

        /// <summary>
        /// 设定地址的范围;
        /// </summary>
        /// <param name="maxAddress"></param>
        public void SetAddressRange(ulong maxAddress) {
            try { 
                this.maxAddress = maxAddress / 1073741824 + 1;
                this.startAddress = 0;
                this.endAddress = this.maxAddress;

                NotifyPropertyChanging(nameof(EndAddress));
                NotifyPropertyChanging(nameof(StartAddress));
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("VideoitemListTopPanel->setAddressRange错误:" + ex.Message);
            }
        }
        /// <summary>
        /// 设定大小的范围;
        /// </summary>
        /// <param name="limSize"></param>
        public void SetSizeRange(ulong limSize) {
            try { 
                this.limSize = limSize / 1048576 + 1;
                this.maxSize = this.limSize;
                this.minSize = 0;

                NotifyPropertyChanging(nameof(MinSize));
                NotifyPropertyChanging(nameof(MaxSize));
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("VideoitemListTopPanel->SetSizeRange错误:" + ex.Message);
            }
        }

        /// <summary>
        /// 设定通道号设定组;
        /// </summary>
        /// <param name="channels">通道组</param>
        public void SetChannelNums(ChannelNoModel[] channels) {
            mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.ChannelNums = channels;
            channelNum = channels[0];
            NotifyPropertyChanging(nameof(ChannelNum));
        }
        
        #region 确定筛选的命令;
        private RelayCommand confirmFilteringCommand;
        public RelayCommand ConfirmFilteringCommand {
            get {
                return confirmFilteringCommand ??
                    (confirmFilteringCommand = new RelayCommand(FilterItems));
            }
        }
        #endregion

        //过滤当前可见项;
        private void FilterItems() {
            #region 过滤文件大小;
            ulong ulongMinSize = minSize * 1048576;
            ulong ulongMaxSize = maxSize * 1048576;

            var filteredRows = mainWindowViewModel.VideoItemListViewerPageViewModel.ActualRows.Where(p => p.Size >= ulongMinSize && p.Size <= ulongMaxSize);
            #endregion

            //若不是MOV，则进行过滤;
            if(CDFCSetting.ScanSetting.DeviceType != CDFCEntities.Enums.DeviceTypeEnum.Canon) {
                #region 过滤时间
                
                if(endDate != null && iniDate != null) {
                    filteredRows = filteredRows.Where(p => {
                        var date = DateNumToDateStringConverter.ConvertToNullableDate(p.StartDate);
                        if (date.HasValue) {
                            if (date >= iniDate && date <= endDate) {
                                return true;
                            }
                        }
                        else {
                            return true;
                        }
                        return false;
                    });
                }
                #endregion

                #region 过滤通道号;
                if(channelNum != null) { 
                    filteredRows = filteredRows.Where(p => channelNum.ID == -1 || p.ChannelNO == channelNum.ID);
                }
                #endregion

                #region 过滤起始地址
            ulong ulongStartAddress = startAddress * 1073741824;
            ulong ulongEndAddress = endAddress * 1073741824;

            filteredRows = filteredRows.Where(p =>p.StartAddress   >=  ulongStartAddress
            && p.StartAddress <= ulongEndAddress);
                #endregion
            }

            var rows = filteredRows.ToList();

            mainWindowViewModel.VideoItemListViewerPageViewModel.CurRows.Clear();
            int itemId = 0;
            rows.ForEach(p => { 
                mainWindowViewModel.VideoItemListViewerPageViewModel.AddRow(p, itemId++);
            });
        }
    }
}
