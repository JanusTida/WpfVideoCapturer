using CDFC.Util;
using CDFCEntities.Interfaces;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace CDFCEntities.CScanMethods {
    /// <summary>
    /// 大华所需要方法;
    /// </summary>
    public partial class DHScanMethods : GenericStaticInstance<DHScanMethods>,IScanMethods {
        /// <summary>
        /// 初始化接口;
        /// </summary>
        public Func<ulong, ulong, int, ulong, ulong, IntPtr, bool> InitFunc {
            get {
                return cdfc_object_init;
            }
        }

        /// <summary>
        /// 开始搜寻接口;
        /// </summary>
        public Func<IntPtr, int, int> SearchStartFunc { get; private set; }

        /// <summary>
        /// 开始文件系统搜寻接口;
        /// </summary>
        public Func<IntPtr, int, int> SearchStartFFunc { get; private set; }

        /// <summary>
        /// 开始搜寻(剩余空间）接口;
        /// </summary>
        public Func<IntPtr, int, int> SearchStartFreeFunc { get; private set; }

        /// <summary>
        /// 设置簇大小的接口;
        /// </summary>
        public Func<int, bool> SetClusterSizeFunc { get; private set; }

        /// <summary>
        /// 设定区域大小接口;
        /// </summary>
        public Action<long> SetRegionSizeAct {
            get {
                return rSize => cdfc_object_set_regionsize((ulong)rSize);
            }
        }

        /// <summary>
        /// 获得当前扇区号:
        /// </summary>
        public long CurrentSector {
            [HandleProcessCorruptedStateExceptions]
            get {
                try {
                    return (long)cdfc_object_current_sector();
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("获得当前扇区底层出现问题:Devicetype:anlian" + ex.Message);
                    return 0;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("获得当前扇区底层出现问题:Devicetype:anlian" + ex.Message);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获得当前文件;
        /// </summary>
        public IntPtr FileList {
            get {
                try {
                    return cdfc_object_filelist();
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("获取文件链表指针头出现问题:" + ex.Message);
                    return IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// 设备退出接口;
        /// </summary>
        public Action ExitAct {
            get {
                return cdfc_object_exit;
            }
        }

        /// <summary>
        /// 终止搜寻接口;
        /// </summary>
        public Action StopAct {
            get {
                return cdfc_object_stop;
            }
        }

        /// <summary>
        /// 日期转换接口;
        /// </summary>
        public Action<ulong, IntPtr> DateConvertFunc {
            get {
                return cdfc_object_date_converter;
            }
        }


        /// <summary>
        /// 释放接口;
        /// </summary>
        public Action<IntPtr> FreeTaskAct {
            get {
                return cdfc_object_freetask;
            }
        }
    }

    /// <summary>
    /// 大华所需底层方法;
    /// </summary>
    public partial class DHScanMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_init")]
        private extern static bool cdfc_object_init(ulong nStartSec, ulong nEndSec, int nSecSize,
            ulong nTimePos, ulong nLBAPos, IntPtr hDisk);

        [HandleProcessCorruptedStateExceptions]
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_search_start")]
        private extern static IntPtr cdfc_object_search_start(IntPtr handle, int type, IntPtr error);

        [HandleProcessCorruptedStateExceptions]
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_search_start_f")]
        private extern static IntPtr cdfc_object_search_start_f(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_current_sector")]
        private extern static ulong cdfc_object_current_sector();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_filelist")]
        private extern static IntPtr cdfc_object_filelist();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_exit")]
        private extern static void cdfc_object_exit();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_stop")]
        private extern static void cdfc_object_stop();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_date_converter")]
        private extern static void cdfc_object_date_converter(ulong date, IntPtr resDate);
        

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_freetask")]
        private extern static void cdfc_object_freetask(IntPtr stFile);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_set_regionsize")]
        private extern static void cdfc_object_set_regionsize(ulong regionSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_search_start_free")]
        private extern static IntPtr cdfc_object_search_start_free(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dahua_filelist_2")]
        private extern static IntPtr cdfc_object_filelist_system();
        
    }

    /// <summary>
    /// 大华扫描器构造器;
    /// </summary>
    public partial class DHScanMethods {
        /// <summary>
        /// 文件系统返回的指针;
        /// </summary>
        public IntPtr FileSystemFileList {
            [HandleProcessCorruptedStateExceptions]
            get {
                try { 
                    return cdfc_object_filelist_system();
                }
                catch(AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("DHScanMethods->FileSystemFileList错误:"+ex.Message);
                    return IntPtr.Zero;
                }
            }
        }
             
        public DHScanMethods() {
            SearchStartFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                cdfc_object_search_start_free(handle, type, errorPtr);
                //cdfc_object_search_start(handle, type, errorPtr);
                int error = Marshal.ReadInt32(errorPtr);
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
            SearchStartFFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                var s = cdfc_object_search_start_f(handle, type, errorPtr);
                //var dateCategory = s.GetStructure<DateCategoryStruct>();
                //var v1 = dateCategory.File.GetStructure<VideoStruct>();
                //var v2 = v1.Next.GetStructure<VideoStruct>();
                int error = Marshal.ReadInt32(errorPtr);
                //var f1 = v1.stStAdd.GetStructure<FileFragmentStruct>();
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
            SearchStartFreeFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                cdfc_object_search_start_free(handle, type, errorPtr);
                int error = Marshal.ReadInt32(errorPtr);
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
        }
    }
    



    /// <summary>
    /// 大华所需要方法;
    /// </summary>
    public partial class DHNVRScanMethods : GenericStaticInstance<DHNVRScanMethods>, IScanMethods {
        /// <summary>
        /// 初始化接口;
        /// </summary>
        public Func<ulong, ulong, int, ulong, ulong, IntPtr, bool> InitFunc {
            get {
                return cdfc_object_init;
            }
        }

        /// <summary>
        /// 开始搜寻接口;
        /// </summary>
        public Func<IntPtr, int, int> SearchStartFunc { get; private set; }

        /// <summary>
        /// 开始文件系统搜寻接口;
        /// </summary>
        public Func<IntPtr, int, int> SearchStartFFunc { get; private set; }

        /// <summary>
        /// 开始搜寻(剩余空间）接口;
        /// </summary>
        public Func<IntPtr, int, int> SearchStartFreeFunc { get; private set; }

        /// <summary>
        /// 设置簇大小的接口;
        /// </summary>
        public Func<int, bool> SetClusterSizeFunc { get; private set; }

        /// <summary>
        /// 设定区域大小接口;
        /// </summary>
        public Action<long> SetRegionSizeAct {
            get {
                return rSize => cdfc_object_set_regionsize((ulong)rSize);
            }
        }

        /// <summary>
        /// 获得当前扇区号:
        /// </summary>
        public long CurrentSector {
            [HandleProcessCorruptedStateExceptions]
            get {
                try {
                    return (long)cdfc_object_current_sector();
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("获得当前扇区底层出现问题:Devicetype:anlian" + ex.Message);
                    return 0;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("获得当前扇区底层出现问题:Devicetype:anlian" + ex.Message);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获得当前文件;
        /// </summary>
        public IntPtr FileList {
            get {
                try {
                    return cdfc_object_filelist();
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("获取文件链表指针头出现问题:" + ex.Message);
                    return IntPtr.Zero;
                }
            }
        }

        /// <summary>
        /// 设备退出接口;
        /// </summary>
        public Action ExitAct {
            get {
                return cdfc_object_exit;
            }
        }

        /// <summary>
        /// 终止搜寻接口;
        /// </summary>
        public Action StopAct {
            get {
                return cdfc_object_stop;
            }
        }

        /// <summary>
        /// 日期转换接口;
        /// </summary>
        public Action<ulong, IntPtr> DateConvertFunc {
            get {
                return cdfc_object_date_converter;
            }
        }


        /// <summary>
        /// 释放接口;
        /// </summary>
        public Action<IntPtr> FreeTaskAct {
            get {
                return cdfc_object_freetask;
            }
        }
    }

    /// <summary>
    /// 大华所需底层方法;
    /// </summary>
    public partial class DHNVRScanMethods {
        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_init")]
        private extern static bool cdfc_object_init(ulong nStartSec, ulong nEndSec, int nSecSize,
            ulong nTimePos, ulong nLBAPos, IntPtr hDisk);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_search_start")]
        private extern static IntPtr cdfc_object_search_start(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_search_start_f")]
        private extern static IntPtr cdfc_object_search_start_f(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_current_sector")]
        private extern static ulong cdfc_object_current_sector();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_filelist")]
        private extern static IntPtr cdfc_object_filelist();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_exit")]
        private extern static void cdfc_object_exit();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_stop")]
        private extern static void cdfc_object_stop();

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_date_converter")]
        private extern static void cdfc_object_date_converter(ulong date, IntPtr resDate);


        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_freetask")]
        private extern static void cdfc_object_freetask(IntPtr stFile);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_set_regionsize")]
        private extern static void cdfc_object_set_regionsize(ulong regionSize);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_search_start_free")]
        private extern static IntPtr cdfc_object_search_start_free(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject2.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_dh_nvr_filelist_2")]
        private extern static IntPtr cdfc_object_filelist_system();
    }

    /// <summary>
    /// 大华扫描器构造器;
    /// </summary>
    public partial class DHNVRScanMethods {
        /// <summary>
        /// 文件系统返回的指针;
        /// </summary>
        public IntPtr FileSystemFileList {
            [HandleProcessCorruptedStateExceptions]
            get {
                try {
                    return cdfc_object_filelist_system();
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("DHScanMethods->FileSystemFileList错误:" + ex.Message);
                    return IntPtr.Zero;
                }
            }
        }

        public DHNVRScanMethods() {
            SearchStartFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                cdfc_object_search_start(handle, type, errorPtr);
                //cdfc_object_search_start(handle, type, errorPtr);
                int error = Marshal.ReadInt32(errorPtr);
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
            SearchStartFFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                var s = cdfc_object_search_start_f(handle, type, errorPtr);
                //var dateCategory = s.GetStructure<DateCategoryStruct>();
                //var v1 = dateCategory.File.GetStructure<VideoStruct>();
                //var v2 = v1.Next.GetStructure<VideoStruct>();
                int error = Marshal.ReadInt32(errorPtr);
                //var f1 = v1.stStAdd.GetStructure<FileFragmentStruct>();
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
            SearchStartFreeFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                cdfc_object_search_start_free(handle, type, errorPtr);
                int error = Marshal.ReadInt32(errorPtr);
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
        }
    }
}
