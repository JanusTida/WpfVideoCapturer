using CDFCEntities.DeviceObjects;
using CDFCEntities.Enums;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCEntities.Scanners;
using CDFCLogger.Models;
using CDFCMessageBoxes.MessageBoxes;
using CDFCValidaters.Validaters;
using CDFCValueRanges;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.Models;
using CDFCVideoExactor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace CDFCVideoExactor.Controllers {
    public partial class ScanningController : IScanningController {
        /// <summary>
        /// 使用的对象;
        /// </summary>
        private readonly IObjectDevice iObjectDevice;

        private readonly MainWindowViewModel mainWindowViewModel;

        /// <summary>
        /// 扫描设定;
        /// </summary>
        private readonly ObjectScanSetting objectScanSetting;

        /// <summary>
        /// 扫描窗口对象;
        /// </summary>
        private readonly ScanningInfoDialogWindowViewModel scanningInfoDialogWindowViewModel;

        /// <summary>
        /// 扫描器;
        /// </summary>
        private IObjectScanner scanner;

        /// <summary>
        /// 默认扫描控制器的构造方法;
        /// </summary>
        /// <param name="mainWindowViewModel">主窗体模型，用于控制各个视图</param>
        /// <param name="iObjectDevice">欲扫描的设定</param>
        /// <param name="objectScannSetting">扫描的设定</param>
        public ScanningController(MainWindowViewModel mainWindowViewModel,IObjectDevice iObjectDevice,ObjectScanSetting objectScanSetting,IObjectScanner scanner = null) {
            #region 实现初步验证步骤
            if (mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("ScanningController初始化出错:mainWindowViewModel不可为空");
                throw new NullReferenceException("mainWindowViewModel can't be null!");
            }
            else if (iObjectDevice == null){
                EventLogger.Logger.WriteLine("ScanningController初始化出错:iObjectDevice不可为空");
                throw new NullReferenceException("iObjectDevice can't be null!");
            }
            else if(objectScanSetting == null) {
                EventLogger.Logger.WriteLine("ScanningController初始化出错:objectScanSetting不可为空");
                throw new NullReferenceException("objectScanSetting can't be null");
            }
            #endregion

            this.objectScanSetting = objectScanSetting;
            this.iObjectDevice = iObjectDevice;
            this.mainWindowViewModel = mainWindowViewModel;
            
            if(scanner == null) {
                #region 根据不同的类型生成不同的扫描处理器;
                switch (objectScanSetting.SelectedDeviceTypeEnum) {
                    case DeviceTypeEnum.AnLian:
                        scanner = new AnLianScanner(iObjectDevice);
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
                    case DeviceTypeEnum.Canon:
                        scanner = new CanonScanner(iObjectDevice);
                        break;
                    case DeviceTypeEnum.GoPro:
                        scanner = new GoProScanner(iObjectDevice);
                        break;
                    case DeviceTypeEnum.LingDu:
                        scanner = new LingDuScanner(iObjectDevice);
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
                    default:
                        EventLogger.Logger.WriteLine("ScanningController构造错误:未注册该类型的扫描处理器:" + objectScanSetting.SelectedDeviceTypeEnum);
                        break;
                }
                #endregion
            }
            mainWindowViewModel.Scanner = scanner;
            this.scanner = scanner;
            scanningInfoDialogWindowViewModel = new ScanningInfoDialogWindowViewModel(this);
        }

        /// <summary>
        /// 是否处于扫描状态中;
        /// </summary>
        private bool isScanning = true;
        public bool IsScanning {
            get {
                return isScanning;
            }
            private set {
                isScanning = value;
                scanningInfoDialogWindowViewModel.IsScanning = value;
            }
        }   
    }

    /// <summary>
    /// 与扫描逻辑相关的动作;
    /// </summary>
    public partial class ScanningController {
        /// <summary>
        /// 初始化扫描
        /// </summary>
        /// <returns></returns>
        public bool Init() {
            bool res = true;
            res = scanner.Init(objectScanSetting.ScanMethod,
                    objectScanSetting.IniSector,
                    objectScanSetting.EndSector,
                    objectScanSetting.SectorSize,
                    objectScanSetting.TimePos,
                    objectScanSetting.LbaPos);
            //获得后缀名;
            extensionName = objectScanSetting.ExtensionName;
            if (objectScanSetting.ScanMethod == ScanMethod.Area) {
                try {
                    scanner.SetRegionsize(Convert.ToUInt64(objectScanSetting.RegionSize * 1073741824));
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("ScanningController区域扫描初始化错误:" + ex.Message);
                }
            }
            if(extensionName == "") {
                res = false;
            }
            if(objectScanSetting.CanUserSetClusterSize) {
                scanner.SetClusterSize(objectScanSetting.ClusterSize);
            }
            return res;
        }
        //设备的后缀名;
        private string extensionName;
        /// <summary>
        /// 开始扫描,包括验证设定等;
        /// </summary>
        public void Start() {
            var worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            mainWindowViewModel.VideoItemListViewerPageViewModel.GetIObjectDevice(iObjectDevice);
            
            ThreadPool.QueueUserWorkItem(callBack => UpdateAction());
            worker.RunWorkerAsync();
            //updateThread.Start();

            //部署扫描对话框;
            scanningInfoDialogWindowViewModel.StartDate = DateTime.Now;
            scanningInfoDialogWindowViewModel.TotalSectorCount = objectScanSetting.EndSector;
            scanningInfoDialogWindowViewModel.EndDate = null;
            var dialog = new ScanningInfoDialogWindow(scanningInfoDialogWindowViewModel);
            dialog.ShowDialog();
        }

        private int _itemId = 1;
        
        /// 是否已经取消;
        public bool Canceled { get; set; }
        //是否发生错误;
        private bool error;

        /// <summary>
        /// 扫描完成后的动作;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            bool res;
            if(bool.TryParse(e.Result.ToString(),out res) && !res) {
                error = true;
                CDFCMessageBox.Show("抱歉，发生未知错误!");
            }

            isScanning = false;
        }
       
        /// <summary>
        /// 后台进行扫描动作;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [HandleProcessCorruptedStateExceptions]
        private void Worker_DoWork(object sender, DoWorkEventArgs e) {
            try {
                
                scanner.SearchStart(objectScanSetting.SelectedVersionType.ID);
                e.Result = true;
            }
            catch(AccessViolationException ex) {
                EventLogger.Logger.WriteLine("ScanningController->ScanAsync错误:" + ex.Message);
                e.Result = false;
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("ScanningController->ScanAsync错误:" + ex.Message);
                e.Result = false;
            }
        }
        
        /// <summary>
        /// 终止扫描;
        /// </summary>
        public void Stop() {
            Canceled = true;
            scanner.Stop();
        }
    }

    /// <summary>
    ///通用结构(非MOV)更新界面;
    /// </summary>
    public partial class ScanningController {
        /// <summary>
        /// 前台更新的控制线程;
        /// </summary>
        private void UpdateAction() {
            var second = 0;
            CDFCLogger.Models.LoggerSetting loggerSetting = null;
            try {
                loggerSetting = new CDFCLogger.Models.LoggerSetting {
                    ClusterSize = objectScanSetting.ClusterSize,
                    DeviceTypeEnum = (int)objectScanSetting.SelectedVersionType.DeviceType,
                    DeviceVersionType = (int)objectScanSetting.SelectedVersionType.ID,
                    DriveType = (int)iObjectDevice.DriveType,
                    EndSector = Convert.ToInt64(objectScanSetting.EndSector),
                    EntranceType = (int)mainWindowViewModel.SelectedEntranceType,
                    ExtensionName = objectScanSetting.ExtensionName,
                    IniSector = Convert.ToInt64(objectScanSetting.IniSector),
                    IsMP4Class = CDFCSetting.ScanSetting.IsMP4Class ? 1 : 0,
                    LBAOffset = Convert.ToInt64(objectScanSetting.LbaPos),
                    RegionSize = objectScanSetting.RegionSize,
                    ScanMethod = (int)objectScanSetting.ScanMethod,
                    SectorSize = objectScanSetting.SectorSize,
                    Size = Convert.ToInt64(iObjectDevice.Size),
                    TimeOffset = Convert.ToInt64(objectScanSetting.TimePos),
                    SerialNumber = iObjectDevice.DriveType == CDFCEntities.Enums.DriveType.PhysicalDevice ? ((Device)iObjectDevice).SerialNumber : iObjectDevice.DriveType == CDFCEntities.Enums.DriveType.Disk ? ((Partition)iObjectDevice).Device.SerialNumber : "",
                    Sign = iObjectDevice.DriveType == CDFCEntities.Enums.DriveType.Disk ? ((Partition)iObjectDevice).Sign.ToString() : "",
                    ImgPath = iObjectDevice.DriveType == CDFCEntities.Enums.DriveType.ImgFile ? ((ImgFile)iObjectDevice).Path : "",
                    
                };
                
            }
            catch(Exception ex) {
                EventLogger.CaseLogger.WriteLine("ScanningController->UpdateAction日志设定生成错误:"+ex.Message);
            }

            while (true) {
                //若正在扫描中;则更新视图
                List<DateCategory> curCategories = null;
                if (isScanning) {
                    curCategories = mainWindowViewModel.Scanner.CurCategories;
                    //若非剩余空间扫描，则立即更新视图;
                    //附加:若为大华或者wfs的全盘扫描;
                    if (objectScanSetting.ScanMethod == ScanMethod.Left
                        ||((objectScanSetting.SelectedVersionType.DeviceType == DeviceTypeEnum.DaHua||
                        objectScanSetting.SelectedVersionType.DeviceType == DeviceTypeEnum.WFS)
                        &&objectScanSetting.ScanMethod == ScanMethod.EntireDisk)){    
                        scanningInfoDialogWindowViewModel.FileCount = curCategories.Sum(p => p.Videos.Count);
                        UpdateView(curCategories);
                    }
                    //剩余空间扫描仅更新对话框即可(百分比，已扫扇区);
                    else {
                        curCategories = mainWindowViewModel.Scanner.CurCategories;
                        UpdateView(curCategories);
                    }

                    #region 每二十分钟记录一次日志;
                    if (second % 1200 == 0 && curCategories.Count() != 0) {
                        SaveLog(curCategories, loggerSetting);
                    }
                    #endregion
                }
                else {
                    scanningInfoDialogWindowViewModel.RemainingTime = TimeSpan.Zero;
                    curCategories = scanner.CurCategories;

                    ValueRangeList rangeList = null;
                    if (((objectScanSetting.SelectedVersionType.DeviceType == DeviceTypeEnum.DaHua ||
                        objectScanSetting.SelectedVersionType.DeviceType == DeviceTypeEnum.WFS)
                        && objectScanSetting.ScanMethod == ScanMethod.EntireDisk)) {
                        List<DateCategory> curFileSystemCategories = null;

                        if (objectScanSetting.SelectedVersionType.DeviceType == DeviceTypeEnum.DaHua) {
                            #region 大华全盘处理
                            var dhScanner = scanner as DHScanner;
                            if (dhScanner != null) {
                                curFileSystemCategories = dhScanner.CurFileSystemCategories;
                            }
                            else {
                                EventLogger.Logger.WriteLine("ScanningController->UpdateAction->scanner转换错误。");
                            }
                            #endregion
                        }
                        else {
                            #region wfs全盘的处理;
                            var wfsScanner = scanner as WFSScanner;
                            if (wfsScanner != null) {
                                curFileSystemCategories = wfsScanner.CurFileSystemCategories;
                            }
                            else {
                                EventLogger.Logger.WriteLine("ScanningController->UpdateAction->scanner转换错误。");
                            }
                            #endregion
                        }

                        if (scanningInfoDialogWindowViewModel == null) {
                            EventLogger.Logger.WriteLine("scanningInfoDialogWindoViewModel->UpdateAction错误:参数scanningInfoDialogWindowViewModel为空!");
                        }
                        else {
                            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                scanningInfoDialogWindowViewModel.CurPercentage = 0;
                            });

                            scanningInfoDialogWindowViewModel.TitleWords = "正在进行文件校验";
                            rangeList = new ValueRangeList();

                            scanningInfoDialogWindowViewModel.CurPercentage = 0;

                            if (curFileSystemCategories != null && curCategories.Count != 0) {
                                var fsVideoCount = curFileSystemCategories.Sum(p => p.Videos.Count);
                                var innerIndex = 0;
                                #region 进行文件系统范围表构建;
                                foreach (var p in curFileSystemCategories) {
                                    foreach (var q in p.Videos) {
                                        foreach (var t in q.FileFragments) {
                                            rangeList.Concatenate(t.StartAddress, t.StartAddress + t.Size);
                                        }
                                        try {
                                            scanningInfoDialogWindowViewModel.CurPercentage = Convert.ToByte(innerIndex * 50 / fsVideoCount);
                                        }
                                        catch (Exception ex) {
                                            EventLogger.Logger.WriteLine("ScanningController->UpdateAction文件范围表构建错误:" + ex.Message);
                                        }
                                        innerIndex++;
                                        if (Canceled) {
                                            break;
                                        }
                                    }
                                    if (Canceled) {
                                        break;
                                    }
                                }
                                #endregion
                                
                            }


                            bool? state = null;
                            //是否需要跳出;
                            bool broke = false;
                            //之前碎片的状态;
                            bool? oriState = null;

                            //记录所有文件数量;
                            var videoCount = curCategories.Sum(p => p.Videos.Count);
                            var index = 0;
                            byte curPercentage = 0;
                            //var swCovered = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "CoveredList.txt");
                            //var swDeleted = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "DeletedList.txt");
                            #region 进行文件校验
                            curCategories.ForEach(p => {
                                p.Videos.ForEach(q => {
                                    broke = false;
                                    var firstFrag = q.FileFragments.FirstOrDefault();
                                    oriState = firstFrag != null?rangeList.GetRangeValuePosition(firstFrag.StartAddress,firstFrag.StartAddress+firstFrag.Size):null;
                                    foreach (var frag in q.FileFragments) {
                                        //138018816 217319936
                                        state = rangeList.GetRangeValuePosition(frag.StartAddress, frag.StartAddress + frag.Size);
                                        //若存在碎片为覆盖，则判定整个文件为覆盖;
                                        if (state == null || state != oriState) {
                                            q.Integrity = VideoIntegrity.Covered;
                                            //swCovered.WriteLine($"{ frag.StartAddress}-{frag.StartAddress + frag.Size}");
                                            broke = true;
                                        }
                                        if(broke == true) {
                                            break;
                                        }
                                    }
                                    //若未跳出，则表示文件碎片状态完全一致;
                                    if (!broke) {
                                        
                                        q.Integrity = state == true ? VideoIntegrity.Whole : VideoIntegrity.Deleted;
                                        if(q.Integrity == VideoIntegrity.Deleted) {
                                            var iranL = new ValueRangeList();
                                            q.FileFragments.ForEach(t => {
                                                iranL.Concatenate(t.StartAddress, t.StartAddress + t.Size);

                                            });
                                            //swDeleted.WriteLine($"Deleted:{q.StartAddress}");
                                            iranL.Ranges.ForEach(t => {
                                              //  swDeleted.WriteLine($"{t.IniValue}-{t.EndValue}");
                                            });
                                            //swDeleted.WriteLine();
                                            //swDeleted.WriteLine();
                                        }
                                    }
                                });

                                if(byte.TryParse(((index++) * 50 / videoCount + 50).ToString(),out curPercentage)){
                                    scanningInfoDialogWindowViewModel.CurPercentage = curPercentage ;
                                }
                                else {
                                    EventLogger.Logger.WriteLine("ScanningController->UpdateAction->Can't Convert Percentage To Byte!"+curPercentage);
                                }
                            });

                            #endregion

                            //swCovered.Close();
                            //swDeleted.Close();
                            UpdateView(curCategories);
                        }
                    }
                    else {
                        curCategories = scanner.CurCategories;
                        UpdateView(curCategories);
                    }
                    #region 进行文件保存;（若已载入案件)
                    try {
                        if (curCategories.Count() != 0 && mainWindowViewModel.CaseWriter != null) {
                            mainWindowViewModel.CaseWriter.WriterCategories(
                            curCategories,
                            (loggerSetting.Sign +
                            loggerSetting.SerialNumber +
                            loggerSetting.Sign + loggerSetting.ImgPath.Replace('/', '-').Replace('/', '-') +
                            CDFCSetting.ScanSetting.StartingTime.ToString()).
                            Replace('/', '-').Replace('/', '-').Replace(':', '-'),
                            loggerSetting, rangeList);

                            //由于记录日志将反转结构,故在此复原;
                            curCategories.ForEach(p => p.Videos.ForEach(q => q.FileFragments.Reverse()));
                            curCategories.ForEach(p => p.Videos.Reverse());
                            curCategories.Reverse();
                        }
                    }
                    catch (Exception ex) {
                        EventLogger.CaseLogger.WriteLine("ScanningController->UpdateAction写入案件记录错误:" + ex.Message);
                    }


                    #endregion

                    scanningInfoDialogWindowViewModel.CurPercentage = 100;
                    scanningInfoDialogWindowViewModel.EndDate = DateTime.Now;
                    scanningInfoDialogWindowViewModel.TitleWords = "扫描结束";

                    IniFilterViewModel(curCategories);
                    if (curCategories.Count == 0) {
                        switch (scanner.ErrorType) {
                            case 1: 
                                EventLogger.Logger.WriteLine("ScanningController->UpdateAction->扫描对象出错:簇大小设置失败");
                                mainWindowViewModel.UpdateInvoker(() => {
                                    CDFCMessageBox.Show("文件系统参数获取失败,请在扫描设置界面手动填写簇大小");
                                });
                                break;
                            case 3:
                                EventLogger.Logger.WriteLine("ScanningController->UpdateAction->扫描对象出错:未找到支持的厂商");
                                mainWindowViewModel.UpdateInvoker(() => {
                                    CDFCMessageBox.Show("没有找到支持的MP4信息,请联系厂商增加对此MP4的重组支持");
                                });
                                break;
                        }
                    }
                    if (!Canceled) {
                        //若非文件系统扫描或海康，则将已扫扇区置为最大扇区;
                        if ((objectScanSetting.SelectedDeviceTypeEnum == DeviceTypeEnum.HaiKang ||
                            objectScanSetting.ScanMethod != ScanMethod.FileSystem)
                            && scanningInfoDialogWindowViewModel.CurSectorCount != objectScanSetting.EndSector) {
                            
                            scanningInfoDialogWindowViewModel.CurSectorCount =  objectScanSetting.EndSector;// iObjectDevice.Size /(ulong)(iObjectDevice.SectorSize != 0 ? iObjectDevice.SectorSize : 512);
                        }
                    }
                    break;
                }
                Thread.Sleep(1000);
                second++;
            }

            IsScanning = false;

            //若发生了错误，则关闭文件列表;
            if (error) {
                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                    scanningInfoDialogWindowViewModel.IsEnabled = false;
                });
            }
        }

        /// <summary>
        /// 初始化文件过滤器模型;
        /// </summary>
        /// <param name="curCategories"></param>
        /// <returns></returns>
        public bool IniFilterViewModel(List<DateCategory> curCategories) {
            if (curCategories.Count == 0) {
                return true;
            }
            #region  初始化过滤模型;
            //获得当前最大最小日期;
            try {
                #region 设定过滤有效期限;

                uint minDateNum = 0;
                uint maxDateNum = 0;

                //时间极限取值是否成功;
                bool dateOK = true;

                if(CDFCSetting.ScanSetting.DeviceType == DeviceTypeEnum.HanBang || CDFCSetting.ScanSetting.DeviceType == DeviceTypeEnum.MOV 
                    || CDFCSetting.ScanSetting.DeviceType == DeviceTypeEnum.Canon ||CDFCSetting.ScanSetting.DeviceType == DeviceTypeEnum.JingYi) {
                    var headVideo = curCategories.First().Videos.FirstOrDefault();
                    if (headVideo == null) {
                        dateOK = false;
                    }
                    else {
                        minDateNum = headVideo.StartDate;
                        maxDateNum = headVideo.StartDate;
                        curCategories.ForEach(p => {
                            var innerMinDateNum = p.Videos.Min(q => q.StartDate);
                            var innerMaxDateNum = p.Videos.Max(q => q.StartDate);
                            minDateNum = innerMinDateNum < minDateNum ? innerMinDateNum : minDateNum;
                            maxDateNum = innerMaxDateNum > maxDateNum ? innerMaxDateNum : maxDateNum;
                        });
                    }
                }
                ////暂无时间;
                else if (!CDFCSetting.ScanSetting.VersionType.DateOk) {
                    dateOK = false;
                }
                //由于时间分类尚无时间，故取每个文件的时间极限。
                else {
                    minDateNum = curCategories.Where(p => DateCategoryValidaters.ValidateDateNum(p)).Min(p => p.Date);
                    maxDateNum = curCategories.Where(p => DateCategoryValidaters.ValidateDateNum(p)).Max(p => p.Date);
                }

                //若时间取值极限成功，则更新时间的区间;
                if (dateOK) {
                    mainWindowViewModel.UpdateInvoker.Invoke(() => {
                        mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.SetDateRange(minDateNum, maxDateNum);
                    });
                }

                #endregion
                Action SetChannelNos = () => {
                    List<ChannelNoModel> channelModels = new List<ChannelNoModel>();
                    channelModels.Add(new ChannelNoModel { Info = "全部", ID = -1 });

                    List<uint> channels = new List<uint>();
                    curCategories.ForEach(p => {
                        var innerChannels = p.Videos.Select(q => q.ChannelNO).Distinct();
                        channels.AddRange(innerChannels);
                    });
                    channels = channels.Distinct().OrderBy(p => p).ToList();
                    channels.ForEach(p => {
                        channelModels.Add(new ChannelNoModel { Info = "通道" + p, ID = Convert.ToInt32(p) });
                    });

                    var channelArray = mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.ChannelNums = channelModels.ToArray();
                    mainWindowViewModel.UpdateInvoker.Invoke(() => {
                        mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.SetChannelNums(channelArray);
                    });
                };

                #region 设定通道号(监控);
                if(mainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer) {
                    SetChannelNos();
                }
                else if(mainWindowViewModel.SelectedEntranceType == Enums.EntranceType.CPAndMultiMedia) {
                    if(mainWindowViewModel.Scanner?.DeviceType.GetDeviceCategory() == DeviceCategory.Capturer ? true : false) {
                        SetChannelNos();
                    }
                }
                
                #endregion

                #region 设定起始-终止地址范围,文件的最大大小;
                //最大大小;
                ulong limSize = 0;
                //最大地址;
                ulong maxAddress = 0;
                curCategories.ForEach(p => {
                    p.Videos.ForEach(q => {
                        if (q.StartAddress > maxAddress) {
                            maxAddress = q.StartAddress;
                        }
                        //获取文件的最大大小;
                        if (q.Size > limSize) {
                            limSize = q.Size;
                        }
                    });
                });
                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                    mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.SetAddressRange(maxAddress);
                    mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.SetSizeRange(limSize);
                });
                #endregion

                return true;
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("ScanningController -> 初始化过滤模型出错:" + ex.Message);
                return false;
            }
            #endregion
        }
        private void SaveLog(List<DateCategory> curCategories,LoggerSetting loggerSetting) {
            if (mainWindowViewModel.CaseWriter != null && loggerSetting != null) {
                try {
                    mainWindowViewModel.CaseWriter.WriterCategories(
                        curCategories,
                        (loggerSetting.Sign +
                        loggerSetting.SerialNumber +
                        loggerSetting.Sign + loggerSetting.ImgPath +
                        CDFCSetting.ScanSetting.StartingTime).
                        Replace('/', '-').Replace('/', '-').Replace(':', '-'),
                        loggerSetting);
                    //由于记录日志将反转结构,故在此复原;
                    curCategories.ForEach(p => p.Videos.ForEach(q => q.FileFragments.Reverse()));
                    curCategories.ForEach(p => p.Videos.Reverse());
                    curCategories.Reverse();
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("ScanningController->UpdateAction写入文件错误" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 单次更新前台的动作;
        /// </summary>
        public void UpdateView(List<DateCategory> curCategories) {
            UpdateVideoItemListViewer(curCategories);
            UpdateDialogInfo(curCategories);
        }

        /// <summary>
        /// 更新对话框信息;
        /// </summary>
        /// <param name="categories">欲更新的文件分类列表</param>
        private void UpdateDialogInfo(List<DateCategory> categories) {
            int count = 0;
            ulong fileSize = 0;
            categories?.ForEach(p => {
                p.Videos.ForEach(q => {
                    count++;
                    fileSize += q.Size;
                });
            });

            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                scanningInfoDialogWindowViewModel.FileCount = count;
                scanningInfoDialogWindowViewModel.CurSectorCount = mainWindowViewModel.Scanner.CurrentSector;
                if(scanningInfoDialogWindowViewModel.StartDate != null && IsScanning) {
                    scanningInfoDialogWindowViewModel.ElapsedTime = DateTime.Now - scanningInfoDialogWindowViewModel.StartDate.Value;
                    if((ulong)scanningInfoDialogWindowViewModel.ElapsedTime.TotalSeconds != 0) {
                        var speed = (mainWindowViewModel.Scanner.CurrentSector - objectScanSetting.IniSector)
                        * (ulong)objectScanSetting.SectorSize / (ulong)scanningInfoDialogWindowViewModel.ElapsedTime.TotalSeconds;
                        scanningInfoDialogWindowViewModel.Speed = speed;
                        if(speed > 0) {
                            var remainingSector = ((long)objectScanSetting.EndSector - (long)scanningInfoDialogWindowViewModel.CurSectorCount);
                            if(remainingSector >= 0) {
                                scanningInfoDialogWindowViewModel.RemainingTime = TimeSpan.FromSeconds(remainingSector
                            * objectScanSetting.SectorSize / (long)speed);
                            }
                        }
                    }
                }
                else {
                    scanningInfoDialogWindowViewModel.RemainingTime = TimeSpan.Zero;
                }

                if (iObjectDevice.Size != 0) {
                    scanningInfoDialogWindowViewModel.CurPercentage = (byte)((scanningInfoDialogWindowViewModel.CurSectorCount * 100 /objectScanSetting.EndSector));
                }
                scanningInfoDialogWindowViewModel.TotalFileSize = fileSize;
            });
        }

        private void FakeUpdate() {
            int percentage = 0;
            Thread.Sleep(4000);
            while(percentage ++ < 100) {
                scanningInfoDialogWindowViewModel.CurPercentage = (byte)percentage;
                scanningInfoDialogWindowViewModel.CurSectorCount = scanningInfoDialogWindowViewModel.TotalSectorCount / 100 * (ulong) percentage;
                Thread.Sleep(35);
            }
            //scanningInfoDialogWindowViewModel.CurSectorCount = mainWindowViewModel.Scanner.CurrentSector;
            //scanningInfoDialogWindowViewModel.CurPercentage = (byte)((scanningInfoDialogWindowViewModel.CurSectorCount * (ulong)iObjectDevice.SectorSize * 100 / iObjectDevice.Size));
        }
        /// <summary>
        /// 更新文件列表项;
        /// </summary>
        /// <param name="categories">欲更新的文件分类列表</param>
        private void UpdateVideoItemListViewer(List<DateCategory> categories) {
            var videoItemList = mainWindowViewModel.VideoItemListViewerPageViewModel;
            var curRows = videoItemList.CurRows;
            List<VideoRow> newRows = null;
            lock (curRows) {
                categories.ForEach(p => {
                    p.Videos.ForEach(q => {
                        var theRow = curRows.FirstOrDefault(t => t.Video == q);
                        if (theRow == null) {
                            theRow = new VideoRow(q, _itemId++);
                            (newRows ?? (newRows = new List<VideoRow>())).Add(theRow);
                        }
                        else {
                            if(q.Size == 0) {
                                return;
                            }
                            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                theRow.UpdateState();
                            });
                        }
                    });
                });
            }
            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                lock (curRows) {
                    if (newRows != null) {
                        videoItemList.AddRangeRows(newRows);
                    }
                }
            });
        }


    }

    /// <summary>
    /// MOV结构更新界面;
    /// </summary>
    //public partial class ScanningController {
    //    private void UpdateMovAction() {
    //        while (true) {
    //            var curVideos = scanner.CurVideos;
    //            if (isScanning) {
    //                UpdateMovView(curVideos);
    //            }
    //            else {
    //                UpdateMovView(curVideos);
    //                scanningInfoDialogWindowViewModel.CurPercentage = 100;
    //                scanningInfoDialogWindowViewModel.EndDate = DateTime.Now;
    //                scanningInfoDialogWindowViewModel.TitleWords = "扫描结束";
    //                IniMovFilterViewModel(curVideos);
    //                break;
    //            }
    //            Thread.Sleep(1000);
    //        }
    //    }
    //    private void UpdateMovView(List<Video> videos) {
    //        UpdateMovVideoItemListViewer(videos);
    //        UpdateMovDialogInfo(videos);
    //    }
    //    private void UpdateMovVideoItemListViewer(List<Video> videos) {
    //        var videoItemList = mainWindowViewModel.VideoItemListViewerPageViewModel;
    //        var curRows = videoItemList.CurRows;
    //        List<VideoRow> newRows = null;
    //        lock (curRows) {
    //            videos.ForEach(q => {
    //                var theRow = curRows.FirstOrDefault(t => t.Video == q);
    //                if (theRow == null) {
    //                    theRow = new VideoRow(q, itemID++);
    //                    (newRows ?? (newRows = new List<VideoRow>())).Add(theRow);
    //                }
    //                else {
    //                    mainWindowViewModel.UpdateInvoker.Invoke(() => {
    //                        theRow.UpdateState();
    //                    });
    //                }
    //            });
    //        }
    //        mainWindowViewModel.UpdateInvoker.Invoke(() => {
    //            lock (curRows) {
    //                if (newRows != null) {
    //                    videoItemList.AddRangeRows(newRows);
    //                }
    //            }
    //        });
    //    }
    //    private void UpdateMovDialogInfo(List<Video> videos) {
    //        int count = 0;
    //        ulong fileSize = 0;
    //        videos.ForEach(q => {
    //            count++;
    //            fileSize += q.Size;
    //        });
    //        mainWindowViewModel.UpdateInvoker.Invoke(() => {
    //            scanningInfoDialogWindowViewModel.FileCount = count;
    //            scanningInfoDialogWindowViewModel.CurSectorCount = mainWindowViewModel.Scanner.CurrentSector;
    //            scanningInfoDialogWindowViewModel.CurPercentage = (byte)((mainWindowViewModel.Scanner.CurrentSector * (ulong)iObjectDevice.SectorSize * 100 / iObjectDevice.Size));
    //            scanningInfoDialogWindowViewModel.TotalFileSize = fileSize;
    //        });
    //    }
    //    /// <summary>
    //    ///   初始化过滤模型(MOV)
    //    /// </summary>
    //    /// <param name="videos">文件列表</param>
    //    /// <returns></returns>
    //    private bool IniMovFilterViewModel(List<Video> videos) {
    //        try {
    //            #region 设定起始-终止地址范围,文件的最大大小;
    //            //最大大小;
    //            ulong limSize = 0;
    //            //最大地址;
    //            ulong maxAddress = 0;
    //            videos.ForEach(q => {
    //                if (q.StartAddress > maxAddress) {
    //                    maxAddress = q.StartAddress;
    //                }
    //                //获取文件的最大大小;
    //                if (q.Size > limSize) {
    //                    limSize = q.Size;
    //                }
    //            });
    //            mainWindowViewModel.UpdateInvoker.Invoke(() => {
    //                mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.SetAddressRange(maxAddress);
    //                mainWindowViewModel.VideoItemListViewerPageTopPartialPanelViewModel.SetSizeRange(limSize);
    //            });
    //            #endregion
    //            return true;
    //        }
    //        catch (Exception ex) {
    //            EventLogger.Logger.WriteLine("ScanningController -> 初始化过滤模型出错:" + ex.Message);
    //            return false;
    //        }
    //    }
    //}
}
