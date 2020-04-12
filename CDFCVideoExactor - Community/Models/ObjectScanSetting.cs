using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using CDFCVideoExactor.Abstracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CDFCVideoExactor.Enums;
using CDFCLogger.Models;
using CDFCConverters.Enums;
using static CDFCCultures.Managers.ResourceManager;
using CDFCCultures.Managers;
using CDFCSetting;

namespace CDFCVideoExactor.Models {
    /// <summary>
    /// 设定模型;
    /// </summary>
    public partial class ObjectScanSetting : NotificationObject {
        /// <summary>
        /// 设定的构造方法;
        /// </summary>
        /// <param name="iObjectDevice">设定对象</param>
        /// <param name="entranceType">入口类型;</param>
        public ObjectScanSetting(IObjectDevice iObjectDevice, EntranceType entranceType, DeviceTypeEnum deviceType = DeviceTypeEnum.Unknown) {
            if (iObjectDevice == null) {
                EventLogger.Logger.WriteLine("ObjectScanSetting->Constrctor->iObjectDevice为空");
                throw new NullReferenceException("iObjectDevice can't be null");
            }
            IObjectDevice = iObjectDevice;

            this.entranceType = entranceType;
            this.singleType = deviceType;

            #region 自动设定扫描品牌类型;
            switch (entranceType) {
                case EntranceType.Capturer:
                case EntranceType.CPAndMultiMedia:
                    #region 监控视频类型的处理;
                    try {
                        //自动设定监控视频扫描类型;
                        AutoSetCapturerVersionType(iObjectDevice.FileSystemType);
                    }
                    catch (AccessViolationException ex) {
                        EventLogger.Logger.WriteLine("ObjectScanSetting->AutoSetVersionType出错");
                        throw ex;
                    }
                    #endregion
                    break;
                case EntranceType.CapturerSingle:

                    break;
                default:
                    this.SelectedDeviceType =
                        this.DeviceTypes[this.DeviceTypes.Count - 1];
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
            }

            #endregion

            #region 初始化各项参数;
            try {
                endSector = iObjectDevice.Size / (ulong)iObjectDevice.SectorSize;
                maxSector = endSector;
                sectorSize = iObjectDevice.SectorSize;
            }
            //可能并未识别出正确的扇区大小,
            //默认处理;
            catch (DivideByZeroException ex) {
                EventLogger.Logger.WriteLine("ObjectSetting初始化出错:SectorSize为空" + ex.Message);
                endSector = 0;
                sectorSize = 512;
            }

            regionSize = 1;
            iniSector = 0;
            #endregion

        }

        //选定的品牌,针对单独的类型;
        private DeviceTypeEnum singleType;
        //设定对象;
        public IObjectDevice IObjectDevice { get; private set; }
        //入口类型;
        private readonly EntranceType entranceType;
    }

    /// <summary>
    /// 初级设定的状态绑定;
    /// </summary>
    public partial class ObjectScanSetting {
        //区域大小;仅适用与区域扫描;(单位为G)
        private int regionSize = 1;
        public int RegionSize {
            get {
                return regionSize;
            }
            set {
                if (value > 10) {
                    if (value < 20) {
                        regionSize = value % 10;
                    }
                    else {
                        regionSize = 10;
                    }
                }
                else if (value < 1) {
                    regionSize = 1;
                }
                else {
                    regionSize = value;
                }
                NotifyPropertyChanging(nameof(RegionSize));
            }
        }

        /// <summary>
        /// 友情提示;
        /// </summary>
        private string warnWord;
        public string WarnWord {
            get {
                return warnWord;
            }
            set {
                warnWord = value;
                NotifyPropertyChanging(nameof(WarnWord));
            }
        }

        //扫描方法;
        private ScanMethod scanMethod;
        public ScanMethod ScanMethod {
            get {
                return scanMethod;
            }
            set {
                scanMethod = value;
                NotifyPropertyChanging(nameof(ScanMethod));
            }
        }

        #region 品牌型号的类型下拉逻辑;
        /// <summary>
        /// 选定的品牌类型;
        /// </summary>
        private DeviceType selectedDeviceType;
        public DeviceType SelectedDeviceType {
            get {
                if (selectedDeviceType == null && DeviceTypes.Count != 0) {
                    selectedDeviceType = DeviceTypes[0];
                }
                return selectedDeviceType;
            }
            set {
                selectedDeviceType = value;

                if (value == null) {
                    return;
                }
                if (entranceType == EntranceType.Capturer) {
                    DealWithCapturer();
                }
                else if (entranceType == EntranceType.MultiMedia) {
                    DealWithMultiMedia();
                }
                else if (entranceType == EntranceType.CPAndMultiMedia || entranceType == EntranceType.CapturerSingle) {
                    if (selectedDeviceType.DeviceTypeEnum != DeviceTypeEnum.Unknown) {
                        if (selectedDeviceType.DeviceTypeEnum.GetDeviceCategory() == DeviceCategory.Capturer) {
                            DealWithCapturer();
                        }
                        else if (selectedDeviceType.DeviceTypeEnum.GetDeviceCategory() == DeviceCategory.MultiMedia) {
                            DealWithMultiMedia();
                        }
                        else {
                            ScanMethodsEnabled = false;
                        }
                    }
                    else {
                        ScanMethodsEnabled = false;
                    }
                }

                SelectedVersionType = AnyVersionTypes[selectedDeviceType.ID][0];
                NotifyPropertyChanging(nameof(SelectedDeviceType));
                NotifyPropertyChanging(nameof(VersionTypes));
                NotifyPropertyChanging(nameof(ScanEnabled));
            }
        }

        private void DealWithCapturer() {
            #region 监控视频下拉逻辑相关;
            if (SelectedVersionType.DeviceType == DeviceTypeEnum.HaiShiTai ||
                SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.ChuangZe) {
                ScanMethodsEnabled = true;
                IsFSScanEnabled = false;
                IsMP4ScanEnabled = false;
                ScanMethod = ScanMethod.EntireDisk;
            }
            else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.DaHua) {
                ScanMethodsEnabled = true;
                IsMP4ScanEnabled = false;
                IsLeftScanEnabled = false;
                ScanMethod = ScanMethod.EntireDisk;
            }
            else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.Unknown) {
                ScanMethodsEnabled = false;
            }
            else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.AnLian) {
                ScanMethodsEnabled = false;
            }
            else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.HaiSi) {
                ScanMethodsEnabled = false;
            }
            else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.HaiKang
                || SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.WFS) {
                IsAreaScanEnabled = true;
                IsLeftScanEnabled = true;
                IsMP4ScanEnabled = false;
                ScanMethod = ScanMethod.EntireDisk;
            }
            else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.HanBang
                || SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.ZhongWei
                || SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.XingKang
                || SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.H264) {
                IsAreaScanEnabled = false;
                IsLeftScanEnabled = false;
                IsFSScanEnabled = false;
                IsMP4ScanEnabled = false;
                IsEntireScanEnabled = true;
                ScanMethod = ScanMethod.EntireDisk;
            }
            else {
                IsAreaScanEnabled = true;
                IsLeftScanEnabled = true;
                IsMP4ScanEnabled = true;
                IsFSScanEnabled = true;
                ScanMethod = ScanMethod.FileSystem;
            }
            #endregion
        }

        private void DealWithMultiMedia() {
            #region 多媒体下拉逻辑相关;
            if (SelectedDeviceType.DeviceTypeEnum != DeviceTypeEnum.Unknown) {
                IsAreaScanEnabled = false;
                IsFSScanEnabled = false;
                IsLeftScanEnabled = false;
                IsMP4ScanEnabled = false;
                IsEntireScanEnabled = true;
                ScanMethod = ScanMethod.EntireDisk;
            }
            else {
                ScanMethodsEnabled = false;
            }
            #endregion
        }
        public DeviceTypeEnum SelectedDeviceTypeEnum {
            get {
                return SelectedVersionType.DeviceType;
            }
        }
        #endregion


        #region 基本型号的下拉逻辑
        /// <summary>
        /// 选择的基本类型;
        /// </summary>
        private VersionType selectedVersionType;
        public VersionType SelectedVersionType {
            get {
                if (selectedVersionType == null) {
                    selectedVersionType = VersionTypes[0];
                }
                return selectedVersionType;
            }
            set {
                selectedVersionType = value;
                Action dealWithCp = () => {
                    #region 监控类型控制扫描方式可行逻辑;
                    if (selectedVersionType.Info == "FAT32 MP4") {
                        if (selectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.HaiKang) {
                            ScanMethodsEnabled = false;
                        }
                        else if (selectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.H264) {
                            ScanMethodsEnabled = false;
                            IsEntireScanEnabled = true;
                        }
                        else if (selectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.HanBang) {
                            ScanMethodsEnabled = false;
                            IsEntireScanEnabled = true;
                        }
                        else {
                            IsEntireScanEnabled = true;
                        }
                        ScanMethod = ScanMethod.EntireDisk;
                    }
                    else if (selectedVersionType.Info == "JVBK") {
                        if (selectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.ZhongWei) {
                            ScanMethodsEnabled = false;
                            IsEntireScanEnabled = true;
                        }
                    }
                    else if (selectedVersionType.Info == "H264") {
                        if (selectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.H264) {
                            ScanMethodsEnabled = false;
                            //IsLeftScanEnabled = true;
                            //IsAreaScanEnabled = true;
                            //ScanMethod = ScanMethod.Area;
                        }

                    }
                    else if (selectedVersionType.DeviceType == DeviceTypeEnum.HaiKang) {
                        ScanMethodsEnabled = true;
                        IsMP4ScanEnabled = false;
                        IsLeftScanEnabled = false;
                        ScanMethod = ScanMethod.EntireDisk;
                    }
                    else if (selectedVersionType.Info == "RSFS") {
                        ScanMethodsEnabled = false;
                        ScanMethod = ScanMethod.FileSystem;
                    }
                    else if ("大华DHFS" == selectedVersionType.Info || "大华NVR" == selectedVersionType.Info
                    || "DHFS" == selectedVersionType.Info || selectedVersionType.Info == "NVR") {
                        ScanMethodsEnabled = true;
                        IsMP4ScanEnabled = false;
                        IsLeftScanEnabled = false;
                        ScanMethod = ScanMethod.EntireDisk;
                    }
                    else if (selectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.WFS) {
                        ScanMethodsEnabled = true;
                    }
                    else if (SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.HaiShiTai ||
                    SelectedDeviceType.DeviceTypeEnum == DeviceTypeEnum.ChuangZe) {
                        ScanMethodsEnabled = false;
                        IsAreaScanEnabled = true;
                        IsEntireScanEnabled = true;
                        ScanMethod = ScanMethod.EntireDisk;
                    }
                    else if ("HBMS" == SelectedVersionType.Info) {
                        ScanMethodsEnabled = false;
                        IsEntireScanEnabled = true;
                        IsAreaScanEnabled = true;
                        ScanMethod = ScanMethod.EntireDisk;
                    }
                    else if ("SonyPanasonicCanon".Contains(SelectedDeviceType.DeviceTypeEnum.ToString())) {
                        ScanMethodsEnabled = false;
                        IsEntireScanEnabled = true;
                        ScanMethod = ScanMethod.EntireDisk;
                    }
                    else {
                        ScanMethodsEnabled = false;
                        ScanMethod = ScanMethod.FileSystem;
                    }
                    if (selectedVersionType.Info == "FAT32 MP4") {
                        CanUserSetClusterSize = true;
                    }
                    else {
                        IsMP4ScanEnabled = false;
                        CanUserSetClusterSize = false;
                    }
                    #endregion
                    if (selectedVersionType.Info == "FAT32 MP4") {
                        WarnWord = FindResourceString("Mp4WarnWord");
                    }
                    else {
                        WarnWord = null;
                    }
                };
                Action dealWithM = () => {
                    if (SelectedVersionType.DeviceType == DeviceTypeEnum.JingYi) {
                        CanUserSetClusterSize = false;
                    }
                    else {
                        CanUserSetClusterSize = true;
                    }
                };

                if (entranceType == EntranceType.Capturer) {
                    dealWithCp();
                }
                else if (entranceType == EntranceType.MultiMedia) {
                    dealWithM();
                }
                else if (entranceType == EntranceType.CPAndMultiMedia) {
                    if (selectedDeviceType.ID <= CpVersionTypes.Count() - 1) {
                        dealWithCp();
                    }
                    else {
                        dealWithM();
                    }
                }


                NotifyPropertyChanging(nameof(ScanEnabled));
            }
        }
        #endregion

        /// <summary>
        /// 直接控制所有执行扫描方法可行;
        /// </summary>
        private bool scanMethodsEnabled;
        private bool ScanMethodsEnabled {
            get {
                return scanMethodsEnabled;
            }
            set {
                IsAreaScanEnabled = value;
                IsLeftScanEnabled = value;
                IsFSScanEnabled = value;
                IsMP4ScanEnabled = value;
                IsEntireScanEnabled = value;
                scanMethodsEnabled = value;
            }
        }

        /// <summary>
        /// 控制扫描是否可行;
        /// </summary>
        public bool ScanEnabled {
            get {
                switch (scanMethod) {
                    case ScanMethod.EntireDisk:
                        return isEntireScanEnabled;
                    case ScanMethod.FileSystem:
                        return isFSScanEnabled;
                    case ScanMethod.MP4:
                        return isMP4ScanEnabled;
                    case ScanMethod.Area:
                        return isAreaScanEnabled;
                    case ScanMethod.Left:
                        return isLeftScanEnabled;
                    default:
                        return true;
                }
            }
        }

        #region 扫描方法是否可行;
        private bool isFSScanEnabled = true;
        public bool IsFSScanEnabled {
            get {
                return isFSScanEnabled;
            }
            set {
                isFSScanEnabled = value;
                NotifyPropertyChanging(nameof(isFSScanEnabled));
            }
        }
        private bool isEntireScanEnabled = true;
        public bool IsEntireScanEnabled {
            get {
                return isEntireScanEnabled;
            }
            set {
                isEntireScanEnabled = value;
                NotifyPropertyChanging(nameof(IsEntireScanEnabled));
            }
        }
        private bool isMP4ScanEnabled = true;
        public bool IsMP4ScanEnabled {
            get {
                return isMP4ScanEnabled;
            }
            set {
                isMP4ScanEnabled = value;
                NotifyPropertyChanging(nameof(IsMP4ScanEnabled));
            }
        }
        private bool isAreaScanEnabled = true;
        public bool IsAreaScanEnabled {
            get {
                return isAreaScanEnabled;
            }
            set {
                isAreaScanEnabled = value;
                NotifyPropertyChanging(nameof(IsAreaScanEnabled));
            }
        }
        private bool isLeftScanEnabled = true;
        public bool IsLeftScanEnabled {
            get {
                return isLeftScanEnabled;
            }
            set {
                isLeftScanEnabled = value;
                NotifyPropertyChanging(nameof(IsLeftScanEnabled));
            }
        }
        #endregion

    }

    /// <summary>
    /// 高级设定的状态绑定;
    /// </summary>
    public partial class ObjectScanSetting {
        /// <summary>
        /// 用户是否可以手动设置簇大小;
        /// </summary>
        private bool canUserSetClusterSize = false;
        public bool CanUserSetClusterSize {
            get {
                return canUserSetClusterSize;
            }
            set {
                canUserSetClusterSize = value;
                NotifyPropertyChanging(nameof(CanUserSetClusterSize));
            }
        }

        /// <summary>
        /// 初始扫描扇区号；
        /// </summary>
        private ulong iniSector;
        public ulong IniSector {
            get {
                return iniSector;
            }
            set {
                if (value <= endSector) {
                    iniSector = value;
                }
                NotifyPropertyChanging(nameof(IniSector));
            }
        }

        /// <summary>
        /// 终止扫描扇区;
        /// </summary>
        private ulong endSector;
        //最大扇区号;
        private ulong maxSector;
        public ulong EndSector {
            get {
                return endSector;
            }
            set {
                if (value <= maxSector &&
                    (maxSector == 0 || value > iniSector)) {
                    endSector = value;
                }

                NotifyPropertyChanging(nameof(EndSector));
            }
        }

        /// <summary>
        /// 扇区大小设定;
        /// </summary>
        private int sectorSize;
        public int SectorSize {
            get {
                return sectorSize;
            }
            set {
                sectorSize = value;
                NotifyPropertyChanging(nameof(SectorSize));
            }
        }

        /// <summary>
        /// 簇大小设置;
        /// </summary>
        private int clusterSize;
        public int ClusterSize {
            get {
                return clusterSize;
            }
            set {
                clusterSize = value;
                NotifyPropertyChanging(nameof(ClusterSize));
            }
        }

        /// <summary>
        /// 时间偏移量;
        /// </summary>
        private ulong timePos = 0;
        public ulong TimePos {
            get {
                return timePos;
            }
            set {
                timePos = value;
            }
        }

        /// <summary>
        /// lba偏移量;
        /// </summary>
        private ulong lbaPos = 0;
        public ulong LbaPos {
            get {
                return lbaPos;
            }
            set {
                lbaPos = value;
            }

        }
    }

    /// <summary>
    /// 相关固定参数;
    /// </summary>
    public partial class ObjectScanSetting {
        /// <summary>
        /// 所有的品牌类型;
        /// </summary>
        private ObservableCollection<DeviceType> deviceTypes;
        public ObservableCollection<DeviceType> DeviceTypes {
            get {
                if (deviceTypes == null) {
                    switch (entranceType) {
                        case EntranceType.Capturer:
                            #region 监控视频类型的处理;
                            deviceTypes = CpDeviceTypes;
                            break;
                        #endregion

                        case EntranceType.MultiMedia:
                            #region 多媒体类型的处理;
                            switch (multiMediaType) {
                                case MultiMediaType.Camera:
                                    #region 多媒体摄像机的处理
                                    deviceTypes = new ObservableCollection<DeviceType>() {
                                        new DeviceType {ID = 0, DeviceTypeEnum = DeviceTypeEnum.Sony},
                                        new DeviceType {ID = 1, DeviceTypeEnum = DeviceTypeEnum.Panasonic},
                                        new DeviceType {ID = 2, DeviceTypeEnum = DeviceTypeEnum.Canon },
                                        new DeviceType {ID = 3, DeviceTypeEnum = DeviceTypeEnum.Unknown }
                                    };
                                    break;
                                #endregion

                                case MultiMediaType.DrivingRecorder:
                                    #region 行车记录仪的处理；

                                    deviceTypes = new ObservableCollection<DeviceType>() {
                                        new DeviceType {ID = 0, DeviceTypeEnum = DeviceTypeEnum.GoPro },
                                        new DeviceType {ID = 1, DeviceTypeEnum = DeviceTypeEnum.LingDu },
                                        new DeviceType {ID = 2, DeviceTypeEnum = DeviceTypeEnum.ShanLing },
                                        new DeviceType {ID = 3, DeviceTypeEnum = DeviceTypeEnum.UnknownCar },
                                        new DeviceType {ID = 4, DeviceTypeEnum = DeviceTypeEnum.Unknown }
                                    };
                                    break;
                                #endregion

                                case MultiMediaType.ForensicRecorder:
                                    #region 执法记录仪的处理;
                                    deviceTypes = new ObservableCollection<DeviceType>() {
                                    new DeviceType {ID =0,DeviceTypeEnum = DeviceTypeEnum.XingJi },
                                    new DeviceType {ID = 1, DeviceTypeEnum = DeviceTypeEnum.JingYi },
                                    new DeviceType {ID = 2,DeviceTypeEnum = DeviceTypeEnum.Unknown }
                                    };
                                    break;
                                    #endregion
                            }
                            break;
                        #endregion

                        case EntranceType.CPAndMultiMedia:
                            #region 若为监控/多媒体合并
                            deviceTypes = CpAndMDeviceTypes;
                            break;
                        #endregion

                        case EntranceType.CapturerSingle:
                            #region 若为监控单独品牌;
                            var deviceType = CpAndMDeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == singleType);
                            if (deviceType != null) {
                                deviceTypes = new ObservableCollection<DeviceType> {
                                    new DeviceType {ID = 0,DeviceTypeEnum=deviceType.DeviceTypeEnum }
                                };
                                if (deviceType.DeviceTypeEnum.GetDeviceCategory() == DeviceCategory.Capturer) {
                                    DealWithCapturer();
                                }
                                else if (deviceType.DeviceTypeEnum.GetDeviceCategory() == DeviceCategory.MultiMedia) {
                                    DealWithMultiMedia();
                                }
                            }
                            #endregion
                            break;
                    }

                }
                return deviceTypes;
            }
        }

        /// <summary>
        /// 对应品牌的可选项;
        /// </summary>
        //private List<VersionType> versionTypes;
        public List<VersionType> VersionTypes {
            get {
                List<VersionType> versionTypes;
                versionTypes = AnyVersionTypes[SelectedDeviceType.ID].ToList();
                //SelectedVersionType = versionTypes[0];
                NotifyPropertyChanging(nameof(SelectedVersionType));
                return versionTypes;
            }
        }

        private static VersionType[][] CpVersionTypes = new VersionType[][]{
            #region 
            new VersionType[] {
                    new VersionType(DeviceTypeEnum.HaiKang) {ID = 1,Info="HIKVISION" },
                    new VersionType(DeviceTypeEnum.HaiKang) {ID = 2,Info="FAT32 MP4",IsMp4Class=true }
                    },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.AnLian){ID = 1,Info="RSFS" }//安联瑞视;
            },
            new VersionType[]{
                new VersionType (DeviceTypeEnum.DaHua){ID = 1,Info = LanguageHelper.Language == LanguageHelper.DefaultLanguage ? "大华DHFS":"DHFS" },
                //new VersionType {ID = 2,Info= "大华DVR2"},
                //new VersionType (DeviceTypeEnum.DaHua){ID = 3,Info="大华NVR" }
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.HaiSi){ID=1,Info="H264" },//海思;
                new VersionType (DeviceTypeEnum.HaiSi){ID=2,Info="FAT32 MP4" ,IsMp4Class=true}
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.HanBang) {ID=1,Info="HBMS" },//汉邦;
                new VersionType (DeviceTypeEnum.ZhongWei){ID = 2,Info="FAT32 MP4",IsMp4Class=true }
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.WFS){ID=1,Info="WFS0.4" }//WFS;
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.ZhongWei){ID=2,Info="FAT32 MP4",IsMp4Class = true },//中维
                new VersionType (DeviceTypeEnum.JVS){ID=1,Info="JVBK" }
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.XingKang){ID=1,Info="FAT32 MP4" ,IsMp4Class = true}//兴康;
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.H264){ID=1,Info="H264",IsMp4Class = true },//H264
                new VersionType (DeviceTypeEnum.H264){ID=2,Info="FAT32 MP4",IsMp4Class = true }
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.HaiShiTai){ID=1,Info="海视泰" }//海视泰
            },
            new VersionType[] {
                new VersionType (DeviceTypeEnum.RuiShi){ID=1,Info="FAT32 MP4" }//锐视;
            },
            new VersionType[] {
                new VersionType(DeviceTypeEnum.ChuangZe) {ID = 1,Info="创泽" }
            }
#endregion
        };
        private static VersionType[] UnknownVersions = new VersionType[] {
            new VersionType (DeviceTypeEnum.Unknown) {ID=0,Info="未知",IsMp4Class = false }
        };
        private static VersionType[][] CameraVersionTypes = new VersionType[][] {
            #region
            new VersionType[] {
                    new VersionType (DeviceTypeEnum.Sony){ID = 1,Info="MP4",IsMp4Class=true },//索尼
                    new VersionType(DeviceTypeEnum.Sony,false) {ID = 2,Info="MTS" }
                },
                new VersionType[] {
                    new VersionType (DeviceTypeEnum.Panasonic,false){ID = 1,Info="MTS" },//松下
                    new VersionType (DeviceTypeEnum.Panasonic,false){ID = 2,Info="MT2S" },
                    new VersionType (DeviceTypeEnum.Panasonic){ID = 3,Info="MP4" ,IsMp4Class = true}
                },
                new VersionType[] {
                    new VersionType(DeviceTypeEnum.Canon) {ID = 1,Info="MP4" ,IsMp4Class=true},//Canon,
                    new VersionType(DeviceTypeEnum.MOV) {ID = 1,Info="MOV" }//MOV
            }
            #endregion
        };
        private static VersionType[][] DrivingVersionTypes = new VersionType[][] {
            #region
            new VersionType[] {
                                            new VersionType(DeviceTypeEnum.GoPro,false) {ID = 1,Info="MP4" ,IsMp4Class=true }//GoPro
                                        },
                                        new VersionType[] {
                                            new VersionType (DeviceTypeEnum.LingDu,false) {ID = 1,Info = "AVI" }//凌渡;
                                        },
                                        new VersionType[] {
                                            new VersionType(DeviceTypeEnum.ShanLing,false) {ID = 1,Info = "MP4" }//善领;
                                        },
                                        new VersionType[] {
                                            new VersionType(DeviceTypeEnum.UnknownCar) {ID = 1,Info="MP4" }//未知品牌1;
                                        }
            #endregion
        };
        private static VersionType[][] FRecorderVersionTypes = new VersionType[][] {
            #region 
            new VersionType[] {
                    new VersionType(DeviceTypeEnum.JingYi) {ID = 2,Info="MOV" }
                },
                new VersionType[] {
                    //星际与警翼实际调用一个接口;
                    new VersionType(DeviceTypeEnum.JingYi) {ID=1,Info="MP4" }
                },
                //new VersionType[] {
                //    new VersionType(DeviceTypeEnum.GoPro) {ID = 1,Info="MP4" ,IsMp4Class=true}//GoPro
                //},
                new VersionType[] {
                    new VersionType(DeviceTypeEnum.Unknown) {ID = 0,Info="未知" }
                }
            #endregion
        };

        private static ObservableCollection<DeviceType> CpAndMDeviceTypes =>
        #region 
            cpAndMDeviceTypes ??
            (cpAndMDeviceTypes = new ObservableCollection<DeviceType> {
                new DeviceType { ID = 0,  DeviceTypeEnum = DeviceTypeEnum.HaiKang},
                new DeviceType { ID = 1,  DeviceTypeEnum = DeviceTypeEnum.AnLian},
                new DeviceType { ID = 2,  DeviceTypeEnum = DeviceTypeEnum.DaHua},
                //new DeviceType {ID = 3  DeviceTypeEnum = DeviceTypeEnum },
                new DeviceType { ID = 4,  DeviceTypeEnum = DeviceTypeEnum.HanBang},
                new DeviceType { ID = 5,  DeviceTypeEnum = DeviceTypeEnum.WFS},
                new DeviceType { ID = 6,  DeviceTypeEnum = DeviceTypeEnum.ZhongWei},
                //new DeviceType {ID = 7, 
                new DeviceType { ID = 8,  DeviceTypeEnum = DeviceTypeEnum.H264},
                new DeviceType { ID = 9,  DeviceTypeEnum = DeviceTypeEnum.HaiShiTai},
                //new DeviceType {ID = 10,
                new DeviceType { ID = 11, DeviceTypeEnum = DeviceTypeEnum.ChuangZe},
                new DeviceType {ID = 12,  DeviceTypeEnum = DeviceTypeEnum.Sony},
                new DeviceType {ID = 13,  DeviceTypeEnum = DeviceTypeEnum.Panasonic},
                new DeviceType {ID = 14,  DeviceTypeEnum = DeviceTypeEnum.Canon},
                new DeviceType { ID = 15, DeviceTypeEnum = DeviceTypeEnum.GoPro},
                new DeviceType {ID = 16,  DeviceTypeEnum = DeviceTypeEnum.LingDu},
                new DeviceType {ID = 17,  DeviceTypeEnum = DeviceTypeEnum.ShanLing },
                new DeviceType {ID = 18,  DeviceTypeEnum = DeviceTypeEnum.UnknownCar},
                new DeviceType {ID =19,   DeviceTypeEnum = DeviceTypeEnum.XingJi},
                new DeviceType {ID = 20,  DeviceTypeEnum = DeviceTypeEnum.JingYi},
                new DeviceType {ID = 21,  DeviceTypeEnum = DeviceTypeEnum.Unknown }

            });
        #endregion
        private static ObservableCollection<DeviceType> cpAndMDeviceTypes;

        private static ObservableCollection<DeviceType> CpDeviceTypes =>
        #region
            cpDeviceTypes ??
            (cpDeviceTypes = new ObservableCollection<DeviceType> {
                new DeviceType {ID = 0,  DeviceTypeEnum = DeviceTypeEnum.HaiKang},
                new DeviceType {ID = 1,  DeviceTypeEnum = DeviceTypeEnum.AnLian},
                new DeviceType {ID = 2,  DeviceTypeEnum = DeviceTypeEnum.DaHua},
                //new DeviceType {ID = 3,Info="海思监控机" ,DeviceTypeEnum = DeviceTypeEnum},
                new DeviceType {ID = 4,  DeviceTypeEnum = DeviceTypeEnum.HanBang},
                new DeviceType {ID = 5,  DeviceTypeEnum = DeviceTypeEnum.WFS},
                new DeviceType {ID = 6,  DeviceTypeEnum = DeviceTypeEnum.ZhongWei},
                //new DeviceType {ID = 7,Info="兴康监控机" ,DeviceTypeEnum = DeviceTypeEnum},
                new DeviceType {ID = 8,  DeviceTypeEnum = DeviceTypeEnum.H264},
                new DeviceType {ID = 9,  DeviceTypeEnum = DeviceTypeEnum.HaiShiTai},
                //new DeviceType {ID = 10,Info="瑞视监控机" ,DeviceTypeEnum = DeviceTypeEnum},
                new DeviceType {ID = 11, DeviceTypeEnum = DeviceTypeEnum.ChuangZe},
                new DeviceType {ID = 12, DeviceTypeEnum = DeviceTypeEnum.Unknown}
            });
        #endregion
        private static ObservableCollection<DeviceType> cpDeviceTypes;

        /// <summary>
        ///  AnyVersionTypes所有的型号类型;
        /// </summary>
        private VersionType[][] anyVersionTypes;
        private VersionType[][] AnyVersionTypes {
            get {
                if (anyVersionTypes == null) {
                    switch (entranceType) {
                        case EntranceType.Capturer:
                            #region 监控的品牌组合;
                            var anyVersionList = new List<VersionType[]>();
                            anyVersionList.AddRange(CpVersionTypes);
                            anyVersionList.Add(UnknownVersions);
                            anyVersionTypes = anyVersionList.ToArray();
                            break;
                        #endregion
                        case EntranceType.MultiMedia:
                            switch (multiMediaType) {
                                case MultiMediaType.Camera:
                                    #region 多媒体录像机的处理;
                                    var cmanyVersionList = new List<VersionType[]>();
                                    cmanyVersionList.AddRange(CameraVersionTypes);
                                    cmanyVersionList.Add(UnknownVersions);
                                    anyVersionTypes = cmanyVersionList.ToArray();
                                    #endregion
                                    break;
                                case MultiMediaType.DrivingRecorder:
                                    #region 行车记录仪的处理;
                                    var dranyVersionList = new List<VersionType[]>();
                                    dranyVersionList.AddRange(CameraVersionTypes);
                                    dranyVersionList.Add(UnknownVersions);
                                    anyVersionTypes = dranyVersionList.ToArray();
                                    #endregion
                                    break;
                                case MultiMediaType.ForensicRecorder:
                                    #region 执法记录仪的处理;
                                    var franyVersionList = new List<VersionType[]>();
                                    franyVersionList.AddRange(CameraVersionTypes);
                                    franyVersionList.Add(UnknownVersions);
                                    anyVersionTypes = franyVersionList.ToArray();
                                    #endregion
                                    break;
                            }
                            break;
                        case EntranceType.CPAndMultiMedia:
                            #region 监控多媒体合并;
                            var cpAndMVersionList = new List<VersionType[]>();
                            cpAndMVersionList.AddRange(CpVersionTypes);
                            cpAndMVersionList.AddRange(CameraVersionTypes);
                            cpAndMVersionList.AddRange(DrivingVersionTypes);
                            cpAndMVersionList.AddRange(FRecorderVersionTypes);
                            cpAndMVersionList.Add(UnknownVersions);
                            anyVersionTypes = cpAndMVersionList.ToArray();
                            break;

                        #endregion
                        case EntranceType.CapturerSingle:
                            anyVersionTypes = new VersionType[][] {
                                CpVersionTypes.First(p => p.Any(q => q.DeviceType == singleType))
                            };
                            break;
                    }
                }
                return anyVersionTypes;
            }
        }
    }
    /// <summary>
    /// 多媒体选择类型相关;
    /// </summary>
    public partial class ObjectScanSetting {
        private MultiMediaType multiMediaType = MultiMediaType.Camera;
        public MultiMediaType MultiMediaType {
            get {
                return multiMediaType;
            }
            set {
                //若不与前值相等，则刷新;
                if (multiMediaType != value) {
                    deviceTypes.Clear();

                    List<DeviceType> newDeviceTypes = null;
                    switch (value) {
                        case MultiMediaType.Camera:
                            #region 多媒体摄像机的处理
                            newDeviceTypes = new List<DeviceType>() {
                                        new DeviceType {ID = 0,DeviceTypeEnum = DeviceTypeEnum.Sony},
                                        new DeviceType {ID = 1,DeviceTypeEnum = DeviceTypeEnum.Panasonic },
                                        new DeviceType {ID = 2,DeviceTypeEnum = DeviceTypeEnum.Canon },
                                        new DeviceType {ID = 3,DeviceTypeEnum = DeviceTypeEnum.Unknown }
                                    };
                            break;
                        #endregion

                        case MultiMediaType.DrivingRecorder:
                            #region 行车记录仪的处理；
                            newDeviceTypes = new List<DeviceType>() {
                                    new DeviceType {ID = 0, DeviceTypeEnum = DeviceTypeEnum.GoPro},
                                    new DeviceType {ID = 1, DeviceTypeEnum = DeviceTypeEnum.LingDu },
                                    new DeviceType {ID = 2, DeviceTypeEnum = DeviceTypeEnum.ShanLing },
                                    new DeviceType {ID = 3, DeviceTypeEnum = DeviceTypeEnum.UnknownCar },
                                    new DeviceType {ID = 4, DeviceTypeEnum = DeviceTypeEnum.Unknown }
                                    };
                            break;
                        #endregion

                        case MultiMediaType.ForensicRecorder:
                            #region 执法记录仪的处理;
                            newDeviceTypes = new List<DeviceType>() {
                                    new DeviceType {ID =  0, DeviceTypeEnum = DeviceTypeEnum.XingJi},
                                    new DeviceType {ID = 1, DeviceTypeEnum = DeviceTypeEnum.JingYi },
                                    new DeviceType {ID = 2, DeviceTypeEnum = DeviceTypeEnum.Unknown }
                                    };
                            break;
                            #endregion
                    }
                    newDeviceTypes?.ForEach(p => {
                        deviceTypes.Add(p);
                    });
                    multiMediaType = value;
                    anyVersionTypes = null;
                    selectedDeviceType = null;

                    NotifyPropertyChanging(nameof(MultiMediaType));
                    //NotifyPropertyChanging(nameof(DeviceTypes));
                    NotifyPropertyChanging(nameof(SelectedDeviceType));
                    NotifyPropertyChanging(nameof(VersionTypes));
                }
            }
        }
    }

    /// <summary>
    /// 自动识别相关;
    /// </summary>
    public partial class ObjectScanSetting {
        /// <summary>
        /// 设定选中型号类型，用于自动识别;
        /// </summary>
        /// <param name="fsType"></param>
        private void AutoSetCapturerVersionType(int fsType) {
            switch (fsType) {
                case 1:
                    this.SelectedDeviceType =
                        this.DeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == DeviceTypeEnum.WFS);
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
                case 2:
                    this.SelectedDeviceType =
                        this.DeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == DeviceTypeEnum.DaHua);
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
                case 3:
                    this.SelectedDeviceType =
                        this.DeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == DeviceTypeEnum.DaHua);
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
                case 4:
                    this.SelectedDeviceType =
                        this.DeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == DeviceTypeEnum.HaiKang);
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
                case 8:
                    this.SelectedDeviceType =
                        this.DeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == DeviceTypeEnum.HanBang);
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
                case 9:
                    this.SelectedDeviceType =
                        this.DeviceTypes.FirstOrDefault(p => p.DeviceTypeEnum == DeviceTypeEnum.ZhongWei);
                    this.SelectedVersionType =
                        this.VersionTypes[1];
                    break;
                default:
                    this.SelectedDeviceType =
                        this.DeviceTypes[this.DeviceTypes.Count - 1];
                    this.SelectedVersionType =
                        this.VersionTypes[0];
                    break;
            }
        }

        /// <summary>
        /// 获得当前类型的后缀名;
        /// </summary>
        public string ExtensionName {
            get {
                if (entranceType == EntranceType.Capturer) {

                    return selectedVersionType.IsMp4Class ? "MP4" : "dav";
                }
                else if (entranceType == EntranceType.CPAndMultiMedia) {
                    if (SelectedVersionType.DeviceType.GetDeviceCategory() == DeviceCategory.Capturer) {
                        return selectedVersionType.IsMp4Class ? "MP4" : "dav";
                    }
                    else if (SelectedVersionType.DeviceType.GetDeviceCategory() == DeviceCategory.MultiMedia) {
                        return selectedVersionType.Info;
                    }
                    else {
                        return "null";
                    }
                }
                else if (entranceType == EntranceType.CapturerSingle) {
                    return selectedVersionType.IsMp4Class ? "MP4" : "dav";
                }
                else {
                    return selectedVersionType.Info;
                }
            }
        }
    }

    /// <summary>
    /// 日志设定相关；
    /// </summary>
    public partial class ObjectScanSetting {
        public LoggerSetting LoggerSetting {
            get {
                var loggerSetting = new LoggerSetting {
                    DriveType = (int)IObjectDevice.DriveType,
                    ClusterSize = clusterSize,
                    DeviceTypeEnum = selectedDeviceType.ID,
                    DeviceVersionType = selectedVersionType.ID,
                    IniSector = Convert.ToInt64(iniSector),
                    EntranceType = (int)entranceType,
                    ExtensionName = ExtensionName,
                    IsMP4Class = 0,
                    LBAOffset = 0,

                    Id = 0,

                };
                return loggerSetting;
            }
        }
    }

}

