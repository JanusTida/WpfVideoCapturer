using CDFCEntities.Interfaces;
using System;
using CDFCEntities.Files;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using CDFCEntities.Enums;
using CDFCEntities.CRecoveryMethods;
using System.IO;
using CDFCStatic.IOMethods;
using System.Threading;
using EventLogger;

namespace CDFCEntities.Abstracts {
    /// <summary>
    /// 默认文件恢复器;
    /// </summary>
    public class DefaultObjectRecoverer:IRecoverer {
        /// <summary>
        /// 默认文件恢复器构造方法;
        /// </summary>
        /// <param name="deviceType">品牌类型</param>
        /// <param name="scanMethod">扫描方式</param>
        /// <param name="iObjectDevice">扫描对象</param>
        /// <param name="video">恢复对象</param>
        public DefaultObjectRecoverer(DeviceTypeEnum deviceType,ScanMethod scanMethod,IObjectDevice iObjectDevice) {
            switch (deviceType) {
                #region 安联的处理;
                case DeviceTypeEnum.AnLian:
                    objectRecoveryMethods = AnlianRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 佳能的处理；
                case DeviceTypeEnum.Canon:
                    objectRecoveryMethods = CanonRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 创泽的处理
                case DeviceTypeEnum.ChuangZe:
                    objectRecoveryMethods = ChuangZeRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 大华的处理
                case DeviceTypeEnum.DaHua:
                    objectRecoveryMethods = DHRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 海思的处理；
                case DeviceTypeEnum.HaiSi:
                    objectRecoveryMethods = HaiSiRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 汉邦的处理
                case DeviceTypeEnum.HanBang:
                    objectRecoveryMethods = HanBangRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 海康的处理
                case DeviceTypeEnum.HaiKang:
                    objectRecoveryMethods = HKRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 海视泰的处理
                case DeviceTypeEnum.HaiShiTai:
                    objectRecoveryMethods = HSTRecoveryMethods.StaticInstance;
                    break;
                #endregion
                     
                #region 中维，兴康的处理
                case DeviceTypeEnum.ZhongWei:
                case DeviceTypeEnum.XingKang:
                case DeviceTypeEnum.H264:
                    objectRecoveryMethods = MP4RecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 瑞视的处理;
                case DeviceTypeEnum.RuiShi:
                    objectRecoveryMethods = RuiShiRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 索尼的处理;
                case DeviceTypeEnum.Sony:
                    objectRecoveryMethods = SonyRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 松下的处理;
                case DeviceTypeEnum.Panasonic:
                    objectRecoveryMethods = PanasonicRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region wfs的处理
                case DeviceTypeEnum.WFS:
                    objectRecoveryMethods = WFSRecoveryMethods.StaticInstance;
                    break;
                #endregion
                #region JVS处理;
                case DeviceTypeEnum.JVS:
                    objectRecoveryMethods = JVSRecoveryMethods.StaticInstance;
                    break;
                #endregion
                #region MOV的处理
                case DeviceTypeEnum.MOV:
                    objectRecoveryMethods = new MOVRecoveryMethods();
                    break;
                #endregion

                #region GoPro的处理:
                case DeviceTypeEnum.GoPro:
                    objectRecoveryMethods = GoProRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 凌渡的处理;
                case DeviceTypeEnum.LingDu:
                    objectRecoveryMethods = LingDuRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 警翼的处理;
                case DeviceTypeEnum.JingYi:
                    objectRecoveryMethods = JingYiRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 善领的处理;
                case DeviceTypeEnum.ShanLing:
                    objectRecoveryMethods = ShanLingRecoveryMethods.StaticInstance;
                    break;
                #endregion
                    
                #region 未知行车记录仪的处理;
                case DeviceTypeEnum.UnknownCar:
                    objectRecoveryMethods = UnknownCarRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 未佳长凌;
                case DeviceTypeEnum.WJCL:
                    objectRecoveryMethods = WJCLRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 鹰潭;
                case DeviceTypeEnum.YinTan:
                    objectRecoveryMethods = YinTanRecoveryMethods.StaticInstance;
                    break;
                #endregion

                #region 奇盾
                case DeviceTypeEnum.QiDun:
                    objectRecoveryMethods = QiDunRecoeryMethods.StaticInstance;
                    break;
                #endregion
                default:
                    throw new NotImplementedException("当前设备不支持!");
            }

            this.deviceType = deviceType;
            IObjectDevice = iObjectDevice;
            this.scanMethod = scanMethod;
        }

        /// <summary>
        /// 当前设备类型;
        /// </summary>
        private DeviceTypeEnum deviceType;
        /// <summary>
        /// 当前所使用的设备;
        /// </summary>
        public IObjectDevice IObjectDevice { get; private set; }

        /// <summary>
        /// 当前恢复的对象;
        /// </summary>
        public Video Video { get; private set; }
        
        private ScanMethod scanMethod;

        /// <summary>
        /// 当前恢复所需方法;
        /// </summary>
        protected readonly IRecoveryMethods objectRecoveryMethods;

        /// <summary>
        /// 多线程锁,用于控制文件保存时的节奏，避免previewsize等变量的不一致;
        /// </summary>
        private object locker = new object();
        /// <summary>
        /// 设定预览大小接口;
        /// </summary>
        /// <param name="nSize"></param>
        [HandleProcessCorruptedStateExceptions]
        public void SetPreview(ulong nSize) {
            if (objectRecoveryMethods.SetPreviewSizeAct == null) {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->SetPreview错误:接口未注册");
            }
            else {
                try {
                    objectRecoveryMethods.SetPreviewSizeAct.Invoke(nSize);
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->SetPreview底层错误" + ex.Message);
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->SetPreview未知错误" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 当前扇区号;
        /// </summary>
        protected long curProgressSector;
        protected IntPtr progressPtr = IntPtr.Zero;
        public long CurProgressSize {
            [HandleProcessCorruptedStateExceptions]
            get {
                if(progressPtr == IntPtr.Zero) {
                    return curProgressSector;
                }
                try {
                    return Marshal.ReadInt64(progressPtr);
                }
                catch(AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectRecoverer->CurProgress获取错误" + ex.Message);
                    return 0;
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectRecoverer->CurProgress未知错误" + ex.Message);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 错误类型;
        /// </summary>
        protected int errorType;
        protected IntPtr errorPtr = IntPtr.Zero;
        public int ErrorType {
            [HandleProcessCorruptedStateExceptions]
            get {
                if (errorPtr == IntPtr.Zero) {
                    return errorType;
                }
                try {
                    return Marshal.ReadInt32(errorPtr);
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectRecoverer->ErrorType获取错误" + ex.Message);
                    return 0;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectRecoverer->ErrorType未知错误" + ex.Message);
                    return 0;
                }
            }
        }
        
        [HandleProcessCorruptedStateExceptions]
        //public bool FileSave(IntPtr szFile, IntPtr hDisk, IntPtr saveFileHandle, IntPtr nCurrSizeDW, IntPtr nError) {
        public virtual bool SaveAs(string saveLocation) {
            //是否出现了错误:
            bool res = false;
            saveLocation = IOStaticMethods.GetValidPath(saveLocation);

            if (objectRecoveryMethods.FileSaveFunc == null) {
                EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave错误:接口未注册");
                res = false;
            }
            else {
                //保存文件的文件流;
                FileStream fs = new FileStream(saveLocation, FileMode.Create);

                #region 部署文件恢复方法出参;
                progressPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(long)));
                errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt64(progressPtr, 0);
                Marshal.WriteInt32(errorPtr, 0);
                #endregion

                try {
                    switch (scanMethod) {
                        case ScanMethod.FileSystem:
                            res = objectRecoveryMethods.FileSaveFFunc(Video.VideoPtr, IObjectDevice.Handle, fs.SafeFileHandle.DangerousGetHandle(), progressPtr, errorPtr);
                            
                            break;
                        case ScanMethod.EntireDisk:
                            //若对象类型为大华，或者wfs的全盘扫描类型;
                            //if((deviceType == DeviceTypeEnum.DaHua || deviceType == DeviceTypeEnum.WFS)
                            //    && Video.Integrity == VideoIntegrity.Whole) {
                            //    //若为完整文件，则调用文件系统保存接口;
                            //    res = iObjectRecoveryMethods.FileSaveFFunc(Video.VideoPtr, IObjectDevice.Handle, fs.SafeFileHandle.DangerousGetHandle(), progressPtr, errorPtr);
                            //}
                            //else {
                                res = objectRecoveryMethods.FileSaveFunc(Video.VideoPtr, IObjectDevice.Handle, fs.SafeFileHandle.DangerousGetHandle(), progressPtr, errorPtr);
                            //}
                            break;
                        default:
                            res = objectRecoveryMethods.FileSaveFunc(Video.VideoPtr, IObjectDevice.Handle, fs.SafeFileHandle.DangerousGetHandle(), progressPtr, errorPtr);
                            break;
                    }
                    #region 释放文件恢复出参;
                    curProgressSector = Marshal.ReadInt64(progressPtr);
                    errorType = Marshal.ReadInt32(errorPtr);
                    //进行指针保存，并释放地址;
                    var proPtr = progressPtr;
                    var errPtr = errorPtr;
                    //首先将其指向零;
                    progressPtr = IntPtr.Zero;
                    errorPtr = IntPtr.Zero;
                    //再进行释放;
                    Marshal.FreeHGlobal(proPtr);
                    Marshal.FreeHGlobal(errPtr);
                    #endregion
                }

                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave底层错误:" + ex.Message);
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("DefaultObjectScanner->FileSave未知错误:" + ex.Message);
                }
                finally {
                    fs.Close();
                }
            }
           
            return res;
        }
        
        /// <summary>
        /// 初始化接口;
        /// </summary>
        public void Init(Video video) {
            curProgressSector = 0;
            progressPtr = IntPtr.Zero;
            errorType = 0;
            errorPtr = IntPtr.Zero;
            this.Video = video;
        }

        private IntPtr bufferPtr;

        public const int previewMaxSize = 10485760;
        [HandleProcessCorruptedStateExceptions]
        public IntPtr ReadToBuffer() {
            if(bufferPtr != IntPtr.Zero) {
                try {
                    Marshal.FreeHGlobal(bufferPtr);
                    bufferPtr = IntPtr.Zero;
                }
                catch(Exception ex) {
                    Logger.WriteLine($"{nameof(DefaultObjectRecoverer)}->{nameof(ReadToBuffer)}:{ex.Message}");
                }
            }
            if(Video != null) {
                bufferPtr = Marshal.AllocHGlobal( previewMaxSize );
                try {
                    if (deviceType == DeviceTypeEnum.HaiKang) {
                        if (objectRecoveryMethods.ReadToBuffer(this.Video.VideoPtr, IObjectDevice.Handle, bufferPtr, previewMaxSize)) {
                            return bufferPtr;
                        }
                    }
                    //若读取成功,则输出缓冲区内容;
                    else if (scanMethod == ScanMethod.FileSystem) {
                        if (objectRecoveryMethods.ReadToBuffer_F(this.Video.VideoPtr, IObjectDevice.Handle, bufferPtr, previewMaxSize)) {
                            return bufferPtr;
                        }
                    }
                    else {
                        var s = objectRecoveryMethods.ReadToBuffer(this.Video.VideoPtr, IObjectDevice.Handle, bufferPtr, previewMaxSize);
                        if (s) {
                            var bs = new byte[102400];
                            Marshal.Copy(bufferPtr, bs, 0, bs.Length);
                            return bufferPtr;
                        }
                    }
                    
                }
                catch(Exception ex) {
                    Logger.WriteLine($"{nameof(DefaultObjectRecoverer)}->{nameof(ReadToBuffer)}:{ex.Message}");
                }

                //若读取不成功，则释放缓冲区，并返回空;
                Marshal.FreeHGlobal(bufferPtr);
                bufferPtr = IntPtr.Zero;
                return IntPtr.Zero;
            }
            return IntPtr.Zero;
        }

    }
}
