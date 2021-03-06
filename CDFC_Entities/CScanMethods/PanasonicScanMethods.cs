﻿using CDFCEntities.Interfaces;
using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace CDFCEntities.CScanMethods {
    /// <summary>
    /// 松下的扫描方法;
    /// </summary>
    public partial class PanasonicScanMethods : IScanMethods {
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
        /// 获得当前扇区号:
        /// </summary>
        public long CurrentSector {
            [HandleProcessCorruptedStateExceptions]
            get {
                try {
                    return (long) cdfc_object_current_sector();
                }
                catch (AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("获得当前扇区底层出现问题:Devicetype:panasonic" + ex.Message);
                    return 0;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("获得当前扇区底层出现问题:Devicetype:panasonic" + ex.Message);
                    return 0;
                }
            }
        }

        /// <summary>
        /// 获得当前文件;
        /// </summary>
        public IntPtr FileList {
            [HandleProcessCorruptedStateExceptions]
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

        /// <summary>
        /// 设定区域大小接口;
        /// </summary>
        public Action<long> SetRegionSizeAct {
            get {
                return rSize =>  cdfc_object_set_regionsize ((ulong)rSize);
            }
        }

        public Action<int,int> Set2048Act {
            get {
                return cdfc_object_setb2048;
            }
        }
    }

    /// <summary>
    /// 松下所需底层方法;
    /// </summary>
    public partial class PanasonicScanMethods {
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_init")]
        private extern static bool cdfc_object_init(ulong nStartSec, ulong nEndSec, int nSecSize,
            ulong nTimePos, ulong nLBAPos, IntPtr hDisk);

        [HandleProcessCorruptedStateExceptions]
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_search_start")]
        private extern static IntPtr cdfc_object_search_start(IntPtr handle, int type, IntPtr error);

        [HandleProcessCorruptedStateExceptions]
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_search_start_f")]
        private extern static IntPtr cdfc_object_search_start_f(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_current_sector")]
        private extern static ulong cdfc_object_current_sector();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_filelist")]
        private extern static IntPtr cdfc_object_filelist();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_exit")]
        private extern static void cdfc_object_exit();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_stop")]
        private extern static void cdfc_object_stop();

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_date_converter")]
        public extern static void cdfc_object_date_converter(ulong date, IntPtr resDate);
        
        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_freetask")]
        private extern static void cdfc_object_freetask(IntPtr stFile);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_set_regionsize")]
        private extern static void cdfc_object_set_regionsize(ulong regionSize);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_search_start_free")]
        private extern static IntPtr cdfc_object_search_start_free(IntPtr handle, int type, IntPtr error);

        [DllImport("cdfcproject.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cdfc_panasonic_set_b2048")]
        private extern static void cdfc_object_setb2048(int secSize, int clusterSize);
    }

    /// <summary>
    /// 松下扫描器构造器;
    /// </summary>
    public partial class PanasonicScanMethods {
        public PanasonicScanMethods() {
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
        }
    }
}
