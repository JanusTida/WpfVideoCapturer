using CDFCEntities.Files;
using CDFCEntities.Enums;
using System;
using CDFCEntities.Structs;

namespace CDFCEntities.Interfaces {
    //设备统一接口，包含磁盘，分区，镜像文件;
    public interface IObjectDevice {
        //对象大小;
        long Size { get; set; }
        //对象句柄;
        IntPtr Handle { get; }
        //扇区大小;
        int SectorSize { get; set; }
        //对象类型;
        DriveType DriveType { get; set; }
        
        int FileSystemType { get; }
        
        string GetSectorHexString(long lbaAddress);
        
        bool IniLoggerParition(DeviceTypeEnum deviceType, ScanMethod scanMethod);

        int Read(byte[] destination, int offset, int byteCount, long pos);
        /// <summary>
        /// 构建文件分类实体；
        /// </summary>
        /// <param name="st">文件分类所依托的文件结构</param>
        /// <param name="scanMethod">扫描方式</param>
        /// <returns></returns>
        DateCategory CreateDatecategory(DateCategoryStruct st, ScanMethod scanMethod, DeviceTypeEnum deviceType, bool isFromLogger = false);

        /// <summary>
        /// 构建文件分类实体(不构建文件列表);
        /// </summary>
        /// <param name="st"></param>
        /// <param name="scanMethod"></param>
        /// <returns></returns>
        //DateCategory CreateEmptyDatecategory(DateCategoryStruct st, ScanMethod scanMethod);
    }
    
}
