using CDFCEntities.CScanMethods;
using CDFCEntities.Enums;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCEntities.Recoverers;
using CDFCEntities.Structs;
using CDFC.Util.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace CDFCEntities.Abstracts
{
    /// <summary>
    /// 默认的对象设备扫描器;
    /// </summary>
    public abstract partial class DefaultObjectScanner : IScanner
    {
        protected IScanMethods iObjectScanMethods;

        public DefaultObjectScanner(DeviceTypeEnum deviceType, IObjectDevice iObjectDevice)
        {
            switch (deviceType)
            {
                #region 监控处理
                case DeviceTypeEnum.DaHua:
                    break;
                #region 海康的处理
                case DeviceTypeEnum.HaiKang:
                    iObjectScanMethods = new HKScanMethods();
                    break;
                #endregion

                #region 中维,兴康的处理;
                case DeviceTypeEnum.ZhongWei:
                case DeviceTypeEnum.XingKang:
                case DeviceTypeEnum.H264:
                    iObjectScanMethods = new MP4ScanMethods();
                    break;
                #endregion;

                #region 汉邦的处理;
                case DeviceTypeEnum.HanBang:
                    iObjectScanMethods = new HanBangScanMethods();
                    break;
                #endregion

                #region 海思的处理;
                case DeviceTypeEnum.HaiSi:
                    iObjectScanMethods = new HaiSiScanMethods();
                    break;
                #endregion

                #region 海视泰的处理;
                case DeviceTypeEnum.HaiShiTai:
                    iObjectScanMethods = new HSTScanMethods();
                    break;
                #endregion

                #region 创泽的处理;
                case DeviceTypeEnum.ChuangZe:
                    iObjectScanMethods = new ChuangZeScanMethods();
                    break;
                #endregion

                #region 瑞视的处理
                case DeviceTypeEnum.RuiShi:
                    iObjectScanMethods = new RuiShiScanMethods();
                    break;
                #endregion

                #region 安联的处理
                case DeviceTypeEnum.AnLian:
                    iObjectScanMethods = new AnLianScanMethods();
                    break;
                #endregion

                #region WFS的处理
                case DeviceTypeEnum.WFS:
                    iObjectScanMethods = new WFSScanMethods();
                    break;
                #endregion

                #region JVS(中维处理)
                case DeviceTypeEnum.JVS:
                    iObjectScanMethods = JVSScanMethods.StaticInstance;
                    break;
                #endregion

                #region 鹰潭
                case DeviceTypeEnum.YinTan:
                    iObjectScanMethods = YinTanScanMethods.StaticInstance;
                    break;
                #endregion

                #region 奇盾
                case DeviceTypeEnum.QiDun:
                    iObjectScanMethods = QiDunScanMethods.StaticInstance;
                    break;
                #endregion

                #region 小米
                case DeviceTypeEnum.XiaoMi:
                    iObjectScanMethods = XiaoMiScanMethods.StaticInstance;
                    break;
                #endregion

                #endregion

                #region 多媒体处理
                #region MOV的处理
                case DeviceTypeEnum.MOV:
                    iObjectScanMethods = new MOVScanMethods();
                    break;
                #endregion

                #region  佳能的处理;
                case DeviceTypeEnum.Canon:
                    iObjectScanMethods = new CanonScanMethods();
                    break;
                #endregion

                #region 松下的处理;
                case DeviceTypeEnum.Panasonic:
                    iObjectScanMethods = new PanasonicScanMethods();
                    break;
                #endregion

                #region 索尼的处理;
                case DeviceTypeEnum.Sony:

                    iObjectScanMethods = new SonyScanMethods();
                    break;
                #endregion

                #region GoPro的处理;
                case DeviceTypeEnum.GoPro:
                    iObjectScanMethods = new GoProScanMethods();
                    break;
                #endregion

                #region 凌渡的处理;
                case DeviceTypeEnum.LingDu:
                    iObjectScanMethods = new LingDuScanMethods();
                    break;
                #endregion

                #region 警翼的处理;
                case DeviceTypeEnum.JingYi:
                    iObjectScanMethods = new JingYiScanMethods();
                    break;
                #endregion

                #region 善领的处理;
                case DeviceTypeEnum.ShanLing:
                    iObjectScanMethods = new ShanLingScanMethods();
                    break;
                #endregion

                #region 未知行车记录仪的处理;
                case DeviceTypeEnum.UnknownCar:
                    iObjectScanMethods = new UnknownCarScanMethods();
                    break;
                #endregion

                #region 未佳长凌
                case DeviceTypeEnum.WJCL:
                    iObjectScanMethods = new WJCLScanMethods();
                    break;
                #endregion

                #endregion

                
                default:
                    throw new NotImplementedException("当前设备不支持");
            }
            this.DeviceType = deviceType;
            this.IObjectDevice = iObjectDevice;
        }

        public DeviceTypeEnum DeviceType { get; private set; }
        private ScanMethod scanMethod;

        public IObjectDevice IObjectDevice { get; private set; }

        private int versionType;
        private int secSize;
        protected int? typeIndex;

        /// <summary>
        /// 获取默认的恢复器;
        /// </summary>
        private IRecoverer defaultRecoverer;
        public IRecoverer DefaultRecoverer {
            get {
                if (defaultRecoverer == null)
                {
                    switch (DeviceType)
                    {
                        #region 根据不同型号生成不同的恢复器;
                        case DeviceTypeEnum.AnLian:
                            defaultRecoverer = new AnlianRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.QiDun:
                            defaultRecoverer = new QiDunRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.Canon:
                            defaultRecoverer = new CanonRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.ChuangZe:
                            defaultRecoverer = new ChuangZeRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.DaHua:
                            
                            defaultRecoverer = new DHRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.H264:
                        case DeviceTypeEnum.ZhongWei:
                        case DeviceTypeEnum.XingKang:
                            defaultRecoverer = new MP4Recoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.HaiKang:
                            defaultRecoverer = new HKRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.HaiShiTai:
                            defaultRecoverer = new HSTRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.HaiSi:
                            defaultRecoverer = new HaiSiRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.HanBang:
                            defaultRecoverer = new HanBangRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.JVS:
                            defaultRecoverer = new JVSRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.Panasonic:
                            defaultRecoverer = new PanasonicRecoverer(scanMethod, versionType, IObjectDevice);
                            break;
                        case DeviceTypeEnum.RuiShi:
                            defaultRecoverer = new RuiShiRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.Sony:
                            defaultRecoverer = new SonyRecoverer(scanMethod, versionType, IObjectDevice);
                            break;
                        case DeviceTypeEnum.WFS:
                            defaultRecoverer = new WFSRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.MOV:
                            defaultRecoverer = new MOVRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.GoPro:
                            defaultRecoverer = new GoProRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.LingDu:
                            defaultRecoverer = new LingDuRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.JingYi:
                            defaultRecoverer = new JingYiRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.ShanLing:
                            defaultRecoverer = new ShanLingRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.UnknownCar:
                            defaultRecoverer = new UnknownCarRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.WJCL:
                            defaultRecoverer = new WJCLRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.YinTan:
                            defaultRecoverer = new YinTanRecoverer(scanMethod, IObjectDevice);
                            break;
                        case DeviceTypeEnum.XiaoMi:
                            defaultRecoverer = new XiaoMiRecoverer(scanMethod, IObjectDevice);
                            break;
                        default:
                            EventLogger.Logger.WriteLine("DefaultObjectScanner->DefaultRecoverer传入了未处理的设备类型！");
                            break;
                            #endregion
                    }
                }
                return defaultRecoverer;
            }
        }

        /// <summary>
        /// 本地初始化参数;
        /// </summary>
        /// <param name="scanMethod"></param>
        /// <param name="nStartSec"></param>
        /// <param name="nEndSec"></param>
        /// <param name="nSecSize"></param>
        /// <param name="nTimePos"></param>
        /// <param name="nLBAPos"></param>
        protected void LocalInit(ScanMethod scanMethod, long nStartSec, long nEndSec, int nSecSize, long nTimePos, long nLBAPos,int typeIndex) {
            if (iObjectScanMethods.InitFunc == null) {
                var ex = new NotImplementedException("未注册初始化接口:" + DeviceType.ToString());
                EventLogger.Logger.WriteLine(ex.Message);
                throw ex;
            }
            this.scanMethod = scanMethod;
            this.secSize = nSecSize;
            this.typeIndex = typeIndex;
        }

        /// <summary>
        /// Native初始化;
        /// </summary>
        /// <param name="scanMethod"></param>
        /// <param name="nStartSec"></param>
        /// <param name="nEndSec"></param>
        /// <param name="nSecSize"></param>
        /// <param name="nTimePos"></param>
        /// <param name="nLBAPos"></param>
        /// <returns></returns>
        protected bool NativeInit(ScanMethod scanMethod, long nStartSec, long nEndSec, int nSecSize, long nTimePos, long nLBAPos) {
            try {
                return iObjectScanMethods.InitFunc((ulong)nStartSec, (ulong)nEndSec, nSecSize, (ulong)nTimePos, (ulong)nLBAPos, IObjectDevice.Handle);
            }
            catch (AccessViolationException ex) {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->初始化底层发生错误:" + ex.Message + "\n" + ex.Source + "\n" + DeviceType.ToString());
                return false;
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->初始化发生未知错误:" + ex.Message + "\n" + ex.Source + "\n" + DeviceType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 默认开始扫描的接口;
        /// </summary>
        /// <param name="scanMethod"></param>
        /// <param name="nStartSec"></param>
        /// <param name="nEndSec"></param>
        /// <param name="nSecSize"></param>
        /// <param name="nTimePos"></param>
        /// <param name="nLBAPos"></param>
        /// <param name="hDisk"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public virtual bool Init(ScanMethod scanMethod, long nStartSec, long nEndSec, int nSecSize, long nTimePos, long nLBAPos,int typeIndex)
        {
            LocalInit(scanMethod, nStartSec, nEndSec, nSecSize, nTimePos, nLBAPos,typeIndex);
            return NativeInit(scanMethod, nStartSec, nEndSec, nSecSize, nTimePos, nLBAPos);
        }
        
        /// <summary>
        /// 开始扫描的接口;
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="type"></param>
        [HandleProcessCorruptedStateExceptions]
        public void SearchStart()
        {
            if(typeIndex == null) {
                throw new InvalidOperationException("The typeIndex not been set.");
            }

            try
            {
                State = SearchState.Searching;
                if (DeviceType == DeviceTypeEnum.Panasonic && (typeIndex == 2 || typeIndex == 1))
                {
                    #region 对应PanaSonic->MP4需扫描前设置2048;
                    if (iObjectScanMethods is PanasonicScanMethods panaSonicScanMethods) {
                        if (secSize == 512) {
                            panaSonicScanMethods.Set2048Act(512, 2);
                        }
                        else {
                            panaSonicScanMethods.Set2048Act(2048, 4);
                        }
                    }
                    else {
                        EventLogger.Logger.WriteLine("DefaultObjectScanner->SearchStart错误:扫描方法转化错误(Panasonic)");
                    }
                    #endregion
                }
                else if (DeviceType == DeviceTypeEnum.Sony && typeIndex == 1)
                {
                    #region 对应Sony->MP4需设置2048;
                    var sonyScanMethods = iObjectScanMethods as SonyScanMethods;
                    if (sonyScanMethods != null)
                    {
                        if (secSize == 512)
                        {
                            sonyScanMethods.Set2048Act(512, 2);
                        }
                        else
                        {
                            sonyScanMethods.Set2048Act(2048, 4);
                        }
                    }
                    else
                    {
                        EventLogger.Logger.WriteLine("DefaultObjectScanner->SearchStart错误:扫描方法转化错误(sony)");
                    }
                    #endregion
                }
                switch (scanMethod)
                {
                    case ScanMethod.FileSystem:
                        ErrorType = iObjectScanMethods.SearchStartFFunc(IObjectDevice.Handle, typeIndex.Value);
                        break;
                    case ScanMethod.Left:
                        ErrorType = iObjectScanMethods.SearchStartFreeFunc(IObjectDevice.Handle, typeIndex.Value);
                        break;
                    default:
                        ErrorType = iObjectScanMethods.SearchStartFunc(IObjectDevice.Handle, typeIndex.Value);
                        break;
                }
                versionType = typeIndex.Value;
            }
            catch (AccessViolationException ex)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner -> SearchStart底层错误:" + ex.Message + DeviceType.ToString() + ":" + scanMethod.ToString());
            }
            catch (Exception ex)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->SearchStart未知错误:" + ex.Message + DeviceType.ToString() + ":" + scanMethod.ToString());
            }
            finally
            {
                State = SearchState.Conquiring;
            }

        }

        /// <summary>
        /// 开始扫描(文件系统)的接口;
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="type"></param>
        [HandleProcessCorruptedStateExceptions]
        private void SearchStartF(int type)
        {
            try
            {

            }
            catch (AccessViolationException ex)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner -> SearchStartF底层出错:" + ex.Message + DeviceType.ToString());
            }
            catch (Exception ex)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->SearchStartF未知出错:" + ex.Message + DeviceType.ToString());
            }
        }

        /// <summary>
        /// 开始扫描的接口(剩余空间);
        /// </summary>
        /// <param name="type"></param>
        [HandleProcessCorruptedStateExceptions]
        private void SearchStartFree(int type)
        {
            try
            {
                ErrorType = iObjectScanMethods.SearchStartFreeFunc(IObjectDevice.Handle, type);
            }
            catch (AccessViolationException ex)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner -> SearchStartFree底层出错:" + ex.Message + DeviceType.ToString());
            }
            catch (Exception ex)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->SearchStartFree未知出错:" + ex.Message + DeviceType.ToString());
            }
        }

        /// <summary>
        /// 获得当前扇区号;
        /// </summary>
        public long CurrentSector {
            [HandleProcessCorruptedStateExceptions]
            get {
                try
                {
                    var sec = iObjectScanMethods.CurrentSector;
                    return iObjectScanMethods.CurrentSector;
                }
                catch (AccessViolationException ex)
                {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->获得当前扇区底层错误:" + ex.Message);
                    throw ex;
                }
                catch (Exception ex)
                {
                    EventLogger.Logger.WriteLine("未知当前扇区错误:" + ex.Message);
                    return 0;
                }
            }

        }

        public bool Searching {
            get {
                return State == SearchState.Searching;
            }
        }

        private List<DateCategory> curCategories;
        /// <summary>
        /// 获得当前文件列表;
        /// </summary>
        /// <param name="iObjectDevice">所属的设备对象</param>
        public List<DateCategory> CurCategories {
            [HandleProcessCorruptedStateExceptions]
            get {
                if (curCategories == null)
                {
                    curCategories = new List<DateCategory>();
                }
                if (State == SearchState.Finished)
                {
                    return curCategories;
                }
                //判断是否已从日志加载;
                if (loadedFromLogger)
                {
                    return curCategories;
                }

                lock (curCategories)
                {
                    IntPtr categoryPtr = IntPtr.Zero;
                    IntPtr categoryNode;
                    try
                    {
                        categoryPtr = iObjectScanMethods.FileList;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.Logger.WriteLine("DefaultObject->CurDateCategories出错:" + ex.Message);
                        return curCategories;
                    }
                    categoryNode = categoryPtr;

                    //轮询文件分类列表;
                    while (categoryNode != IntPtr.Zero)
                    {
                        DateCategoryStruct categoryStruct;
                        try
                        {
                            categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
                        }
                        catch (AccessViolationException ex)
                        {
                            EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->categoryNode.GetStructure出错:" + ex.Message);
                            break;
                        }
                        //Check the category is existing in the list;
                        var category = curCategories.FirstOrDefault(p => p.Date == categoryStruct.Date);
                        //if it is not,try to create it and add it into the list;
                        if (category == null)
                        {
                            category = IObjectDevice.CreateDatecategory(categoryStruct, scanMethod, DeviceType);
                            if (DeviceType == DeviceTypeEnum.HaiKang)
                            {
                                category.Videos.ForEach(p => p.Size *= 256);
                            }
                            if (category != null)
                            {
                                curCategories.Add(category);
                            }
                        }
                        //If not just make sure every video in the list is unique;
                        else
                        {
                            var videoPtr = categoryStruct.File;
                            var videoNode = videoPtr;
                            while (videoNode != IntPtr.Zero)
                            {
                                VideoStruct videoStruct;

                                #region 尝试获取文件结构体，若失败，则退出;
                                try
                                {
                                    videoStruct = videoNode.GetStructure<VideoStruct>();
                                }
                                catch (AccessViolationException ex)
                                {
                                    EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->videoNode.GetStructure出错!" + ex.Message + ex.Source);
                                    break;
                                }

                                //验证文件;
                                //若文件通道号过大或文件大小过小,或文件起始范围过大（非文件系统扫描）,则继续轮询;
                                videoStruct.ChannelNO += DeviceType == DeviceTypeEnum.DaHua ? (uint)1 : 0;
                                if (videoStruct.Size < 512)
                                {//videoStruct.ChannelNO >= 200
                                    //||(scanMethod == ScanMethod.FileSystem&&videoStruct.StartAddress >= IObjectDevice.Size)
                                    //videoStruct.ChannelNO > 200 || videoStruct.Size <= 1048576
                                    videoNode = videoStruct.Next;
                                    continue;
                                }
                                else
                                {

                                }
                                #endregion

                                var video = category.CreateVideo(videoNode, videoStruct);
                                //对海康品牌的特别处理:将大小乘以256;
                                if (DeviceType == DeviceTypeEnum.HaiKang)
                                {
                                    video.Size *= 256;
                                }

                                //查询非托管环境下是否存在已有指针,以确定唯一项;
                                Video videoEntity = category.Videos.FirstOrDefault(p => p.VideoPtr == videoNode);

                                #region 若没有，添加新的项;
                                if (videoEntity == null)
                                {
                                    category.Videos.Add(video);
                                    video.DateCategory = category;
                                }
                                #endregion

                                #region 若存在，则刷新值;
                                else
                                {
                                    videoEntity.FileFragments = video.FileFragments;
                                    videoEntity.Size = video.Size;
                                    videoEntity.EndDate = video.EndDate;
                                }
                                #endregion

                                videoNode = videoStruct.Next;
                            }
                        }
                        categoryNode = categoryStruct.Next;
                    }
                    if (State == SearchState.Conquiring)
                    {
                        State = SearchState.Finished;
                    }
                    return curCategories;
                }
            }
        }

        /// <summary>
        /// 时间转换的接口;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public DateTime DateConvert(uint dateNum)
        {
            DateTime? dt = null;

            int byteSize = Marshal.SizeOf(typeof(byte));
            IntPtr dateNumsPtr = Marshal.AllocHGlobal(6 * byteSize);
            this.iObjectScanMethods.DateConvertFunc?.Invoke(dateNum, dateNumsPtr);
            var dateNums = new short[6];
            for (int index = 0; index < 6; index++)
            {
                dateNums[index] = Marshal.ReadByte(dateNumsPtr + index * byteSize);
            }
            Marshal.FreeHGlobal(dateNumsPtr);
            try
            {
                dt = new DateTime(dateNums[0] + 2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                return dt.Value;
            }
            //若时间构造失败;
            catch (AccessViolationException ex)
            {
                EventLogger.Logger.WriteLine("非托管转换时间错误!" + ex.Message);
                dt = new DateTime(2000, 1, 1);
                return dt.Value;
            }
            catch (Exception ex)
            {
                EventLogger.Logger.WriteLine("转换时间错误!" + ex.Message);
                return dt.Value;
            }
        }

        /// <summary>
        /// 中止接口;
        /// </summary>
        public void Stop()
        {
            iObjectScanMethods.StopAct?.Invoke();
        }

        [HandleProcessCorruptedStateExceptions]
        public void FreeTask(IntPtr stFile)
        {
            iObjectScanMethods.FreeTaskAct?.Invoke(stFile);
        }

        /// <summary>
        /// 错误类型(开始搜寻);
        /// </summary>
        public int ErrorType { get; private set; }

        /// <summary>
        /// 设定区域大小接口;
        /// </summary>
        /// <param name="size"></param>
        [HandleProcessCorruptedStateExceptions]
        public void SetRegionsize(long size)
        {
            if (iObjectScanMethods.SetRegionSizeAct == null)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->SetRegionSize错误:接口未注册");
            }
            else
            {
                try
                {
                    iObjectScanMethods.SetRegionSizeAct?.Invoke(size);
                }
                catch (AccessViolationException ex)
                {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->SetRegionSize底层错误:" + ex.Message);
                }
                catch (Exception ex)
                {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->SetRegionSize未知错误:" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 析构接口;
        /// </summary>
        [HandleProcessCorruptedStateExceptions]
        public void Exit()
        {
            //若为扫描
            if (!loadedFromLogger)
            {
                #region 调用释放接口;
                if (iObjectScanMethods.ExitAct == null)
                {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->Dispose出错:接口未注册");
                }
                else
                {
                    try
                    {
                        if (!loadedFromLogger)
                        {
                            iObjectScanMethods.ExitAct();
                        }
                    }
                    catch (AccessViolationException ex)
                    {
                        EventLogger.Logger.WriteLine("DefultObjectScanner->Dispose底层错误：" + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        EventLogger.Logger.WriteLine("DefultObjectScanner->Dispose未知错误：" + ex.Message);
                    }
                }
            }
            else
            {
                #endregion
                try
                {
                    unmanagedPtrs?.ForEach(p => Marshal.FreeHGlobal(p));
                    unmanagedPtrs?.Clear();
                }
                catch (AccessViolationException ex)
                {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner释放非托管内存错误:" + ex.Message);
                }
                catch (Exception ex)
                {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner释放非托管内存未知错误:" + ex.Message);
                }
            }

            curCategories?.Clear();
            defaultRecoverer = null;
        }

        /// <summary>
        /// 设置簇大小接口;
        /// </summary>
        /// <param name="clusterSize"></param>
        /// <returns></returns>
        public bool SetClusterSize(int clusterSize)
        {
            if (iObjectScanMethods.SetClusterSizeFunc == null)
            {
                return false;
            }
            else
            {
                return iObjectScanMethods.SetClusterSizeFunc(clusterSize);
            }

        }


        public SearchState State { get; private set; }



    }
    public enum SearchState
    {
        Suspend,
        Searching,
        Conquiring,
        Finished
    }
    public partial class DefaultObjectScanner
    {
        //记录是否自日志载入;
        private bool loadedFromLogger = false;
        /// <summary>
        /// 自日志载入文件列表;
        /// </summary>
        /// <param name="fileList">非托管内存指针</param>
        /// <param name="ptrs">相关内存的内存队列</param>
        /// <returns>加载是否成功</returns>
        [HandleProcessCorruptedStateExceptions]
        public bool LoadFileList(IntPtr fileList, List<IntPtr> ptrs)
        {
            if (fileList == null || ptrs == null)
            {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->错误:参数不得为空!");
                return false;
            }

            loadedFromLogger = true;
            curCategories = new List<DateCategory>();
            //从前往后遍历;
            lock (curCategories)
            {
                var categoryNode = fileList;
                while (categoryNode != IntPtr.Zero)
                {
                    DateCategoryStruct categoryStruct;
                    try
                    {
                        categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
                        var category = IObjectDevice.CreateDatecategory(categoryStruct, scanMethod, DeviceType, true);
                        if (category != null)
                        {
                            curCategories.Add(category);
                        }
                        else
                        {
                            EventLogger.CaseLogger.WriteLine("DefaultObjectScanner->LoadFileList->CreateDatecategory错误:文件不得为空。");
                        }
                    }
                    catch (AccessViolationException ex)
                    {
                        EventLogger.CaseLogger.WriteLine("DefaultObjectScanner->LoadFileList操作错误:" + ex.Message);
                        break;
                    }
                    catch (Exception ex)
                    {
                        EventLogger.CaseLogger.WriteLine("DefaultObjectScanner->LoadFileList错误:" + ex.Message);
                        break;
                    }

                    categoryNode = categoryStruct.Next;
                }
            }
            lock (ptrs)
            {
                if (unmanagedPtrs == null)
                {
                    unmanagedPtrs = new List<IntPtr>();
                }
                unmanagedPtrs.AddRange(ptrs);
            }
            return true;
        }

        private List<IntPtr> unmanagedPtrs;
    }
}
