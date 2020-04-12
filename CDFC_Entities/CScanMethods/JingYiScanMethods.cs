using CDFCEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CDFCEntities.CScanMethods {
    public partial class JingYiScanMethods : IScanMethods {
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
    public partial class JingYiScanMethods  {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_init")]
        private extern static bool cdfc_object_init(ulong nStartSec, ulong nEndSec, int nSecSize,
            ulong nTimePos, ulong nLBAPos, IntPtr hDisk);

        [HandleProcessCorruptedStateExceptions]
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_search_start")]
        private extern static IntPtr cdfc_object_search_start(IntPtr handle, int type, IntPtr error);

        [HandleProcessCorruptedStateExceptions]
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_search_start_f")]
        private extern static IntPtr cdfc_object_search_start_f(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_current_sector")]
        private extern static ulong cdfc_object_current_sector();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_filelist")]
        private extern static IntPtr cdfc_object_filelist();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_exit")]
        private extern static void cdfc_object_exit();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_stop")]
        private extern static void cdfc_object_stop();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_date_converter")]
        public extern static void cdfc_object_date_converter(ulong date, IntPtr resDate);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_freetask")]
        private extern static void cdfc_object_freetask(IntPtr stFile);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_set_regionsize")]
        private extern static void cdfc_object_set_regionsize(ulong regionSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_search_start_free")]
        private extern static IntPtr cdfc_object_search_start_free(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_jingyi_cluster")]
        public extern static void cdfc_object_set_clustersize(int clusterSize);
    }
    public partial class JingYiScanMethods {
        public JingYiScanMethods() {
            SearchStartFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                cdfc_object_search_start(handle, type, errorPtr);
                int error = Marshal.ReadInt32(errorPtr);
                Marshal.FreeHGlobal(errorPtr);
                return error;
            };
            SearchStartFFunc = (handle, type) => {
                IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
                Marshal.WriteInt32(errorPtr, 0);
                cdfc_object_search_start_f(handle, type, errorPtr);
                int error = Marshal.ReadInt32(errorPtr);
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
            SetClusterSizeFunc = (clusterSize) => {
                try {
                    cdfc_object_set_clustersize(clusterSize);
                    return true;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("初始化簇大小错误:" + ex.Message);
                    return false;
                }
            };
        }
    }
}
