using CDFCEntities.Enums;
using CDFCEntities.Files;
using System;
using System.Collections.Generic;

namespace CDFCEntities.Interfaces {
    public interface IScanner {
        DeviceTypeEnum DeviceType { get; }
        /// <summary>
        /// 初始扫描的接口;
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="scanMethod"></param>
        /// <param name="nStartSec"></param>
        /// <param name="nEndSec"></param>
        /// <param name="nSecSize"></param>
        /// <param name="nTimePos"></param>
        /// <param name="nLBAPos"></param>
        /// <param name="hDisk"></param>
        /// <returns></returns>
        bool Init(ScanMethod scanMethod,
            long nStartSec, long nEndSec, int nSecSize,
                long nTimePos, long nLBAPos,int typeIndex);

        /// <summary>
        /// 当前使用的对象;
        /// </summary>
        IObjectDevice IObjectDevice { get; }
        
        /// <summary>
        /// 默认的恢复器;
        /// </summary>
        IRecoverer DefaultRecoverer { get;}

        /// <summary>
        /// 开始搜寻的接口
        /// </summary>
        /// <param name="type"></param>
        void SearchStart();
        
        /// <summary>
        /// 当前扇区号;
        /// </summary>
        long CurrentSector { get; }

        /// <summary>
        /// 当前获得的时间分类列表;
        /// </summary>
        List<DateCategory> CurCategories { get; }

        /// <summary>
        /// 当前获得的文件列表;
        /// </summary>
        /// <param name="iOjbectDevice"></param>
        /// <returns></returns>
        //List<Video> CurVideos { get; }
        
        /// <summary>
        /// 停止接口;
        /// </summary>
        void Stop();

        /// <summary>
        /// 时间转换接口;
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        DateTime DateConvert(uint date);
        
        /// <summary>
        /// 释放接口;
        /// </summary>
        /// <param name="stFile"></param>
        void FreeTask(IntPtr stFile);

        /// <summary>
        /// 设置区域大小;
        /// </summary>
        /// <param name="size"></param>
        void SetRegionsize(long size);

        /// <summary>
        /// 设定簇大小；
        /// </summary>
        /// <param name="clusterSize"></param>
        /// <returns></returns>
        bool SetClusterSize(int clusterSize);

        /// <summary>
        /// 错误类型(开始搜寻)
        /// </summary>
        int ErrorType { get; }

        /// <summary>
        /// 是否在进行搜寻;
        /// </summary>
        bool Searching { get; }

        /// <summary>
        /// 自指定非托管内存指针加载文件分类列表;
        /// </summary>
        /// <param name="ptr">非托管内存指针所对应的头指针</param>
        /// <param name="fileList">非托管内存的指针队列</param>
        /// <param name="rangeList">保存文件系统的文件范围列表(对应大华，wfs的全盘扫描)</param>
        /// <returns>加载是否成功</returns>
        bool LoadFileList(IntPtr fileList,List<IntPtr> unmanagedPtrs);

        void Exit();

    }
}
