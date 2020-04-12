using System;

namespace CDFCEntities.Interfaces {
    /// <summary>
    /// 对象扫描方法接口;
    /// </summary>
    public interface IScanMethods {
        //初始化的委托;
        Func<ulong, ulong, int, ulong, ulong, IntPtr, bool> InitFunc { get; }

        /// <summary>
        /// 开始搜索的委托;返回值代表错误类型;
        /// </summary>
        Func<IntPtr, int, int> SearchStartFunc { get; }

        //开始搜索的委托（文件系统);
        Func<IntPtr, int, int> SearchStartFFunc { get; }
        
        //当前已扫扇区;
        long CurrentSector { get; }

        //当前文件链表的指针;
        IntPtr FileList { get; }

        //退出接口委托;
        Action ExitAct { get;}

        //停止接口委托;
        Action StopAct { get; }

        //时间转化器接口;
        Action<ulong, IntPtr> DateConvertFunc { get; }
        
        //设置簇大小的接口;
        Func<int,bool> SetClusterSizeFunc { get; }

        //释放接口委托;
        Action<IntPtr> FreeTaskAct { get; }

        //设置区域大小的委托;
        Action<long> SetRegionSizeAct { get; }

        //剩余空间搜索的委托;
        Func<IntPtr, int, int> SearchStartFreeFunc { get; }
    }
}

