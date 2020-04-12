using CDFCEntities.Enums;
using CDFCEntities.Abstracts;
using CDFCEntities.Interfaces;
using CDFCEntities.Files;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using System;
using CDFCEntities.Structs;
using CDFC.Util.PInvoke;
using CDFCEntities.CScanMethods;

namespace CDFCEntities.Scanners {
    /// <summary>
    /// 大华扫描器;
    /// </summary>
    public class DHScanner :DefaultObjectScanner {
        public DHScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.DaHua, iObjectDevice) {
            iObjectScanMethods = new DHScanMethods();
        }

        //private DeviceTypeEnum localDeviceType = DeviceTypeEnum.DaHua;

        private ScanMethod localScanMethod = ScanMethod.EntireDisk;

        /// <summary>
        /// 获得当前的文件列表(文件系统，做全盘扫描时需要)
        /// </summary>
        // private List<DateCategory> curFileSystemCategories;
        public List<DateCategory> CurFileSystemCategories {
            [HandleProcessCorruptedStateExceptions]
            get {
                #region 获得文件列表(原版本)

                //if (curFileSystemCategories == null) {
                //    curFileSystemCategories = new List<DateCategory>();
                //}
                //lock (curFileSystemCategories) {
                //    IntPtr categoryPtr = IntPtr.Zero;
                //    IntPtr categoryNode;
                //    try {
                //        var dhScanMethods = iObjectScanMethods as DHScanMethods;
                //        if (dhScanMethods != null) {
                //            categoryPtr = dhScanMethods.FileSystemFileList;
                //        }
                //        else {
                //            EventLogger.Logger.WriteLine("DefaultObjectScannerCurFileSystemCategories错误:扫描方法不支持");
                //            return curFileSystemCategories;
                //        }
                //    }
                //    catch (Exception ex) {
                //        EventLogger.Logger.WriteLine("DefaultObject->CurDateCategories出错:" + ex.Message);
                //        return curFileSystemCategories;
                //    }
                //    categoryNode = categoryPtr;

                //    //轮询文件分类列表;
                //    while (categoryNode != IntPtr.Zero) {
                //        DateCategoryStruct categoryStruct;
                //        try {
                //            categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
                //        }
                //        catch (AccessViolationException ex) {
                //            EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->categoryNode.GetStructure出错:" + ex.Message);
                //            break;
                //        }
                //        //Check the category is existing in the list;
                //        var category = curFileSystemCategories.FirstOrDefault(p => p.Date == categoryStruct.Date);
                //        //if it is not,try to create it and add it into the list;
                //        if (category == null) {
                //            category = IObjectDevice.CreateDatecategory(categoryStruct,localScanMethod);
                //            category.DeviceTypeEnum = localDeviceType;
                //            if (category != null) {
                //                curFileSystemCategories.Add(category);
                //            }
                //        }
                //        //If not just make sure every video in the list is unique;
                //        else {
                //            var videoPtr = categoryStruct.File;
                //            var videoNode = videoPtr;
                //            while (videoNode != IntPtr.Zero) {
                //                VideoStruct videoStruct;

                //                #region 尝试获取文件结构体，若失败，则退出;
                //                try {
                //                    videoStruct = videoNode.GetStructure<VideoStruct>();
                //                }
                //                catch (AccessViolationException ex) {
                //                    EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->videoNode.GetStructure出错!" + ex.Message + ex.Source);
                //                    break;
                //                }
                //                //验证文件;
                //                if (videoStruct.ChannelNO > 200 || videoStruct.Size == 0 || videoStruct.StartAddress > IObjectDevice.Size) {
                //                    videoNode = videoStruct.Next;
                //                    continue;
                //                }
                //                #endregion

                //                var video = category.CreateVideo(videoNode, videoStruct);

                //                //查询非托管环境下是否存在已有指针,以确定唯一项;
                //                Video videoEntity = category.Videos.FirstOrDefault(p => p.VideoPtr == videoNode);

                //                #region 若没有则添加新的项;
                //                if (videoEntity == null) {
                //                    category.Videos.Add(video);
                //                    video.DateCategory = category;
                //                }
                //                #endregion

                //                #region 若存在，则刷新值;
                //                else {
                //                    videoEntity.FileFragments = video.FileFragments;
                //                    videoEntity.Size = video.Size;
                //                    videoEntity.EndDate = video.EndDate;
                //                }
                //                #endregion

                //                videoNode = videoStruct.Next;
                //            }
                //        }
                //        categoryNode = categoryStruct.Next;
                //    }
                //    return curFileSystemCategories;
                #endregion

                List<DateCategory> curFileSystemCategories = new List<DateCategory>();
                IntPtr categoryPtr = IntPtr.Zero;
                IntPtr categoryNode;
                try {
                    var dhScanMethods = iObjectScanMethods as DHScanMethods;
                    if (dhScanMethods != null) {
                        categoryPtr = dhScanMethods.FileSystemFileList;
                    }
                    else {
                        EventLogger.Logger.WriteLine("DefaultObjectScannerCurFileSystemCategories错误:扫描方法不支持");
                        return curFileSystemCategories;
                    }
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("DefaultObject->CurDateCategories出错:" + ex.Message);
                    return curFileSystemCategories;
                }
                categoryNode = categoryPtr;

                //轮询文件分类列表;
                while (categoryNode != IntPtr.Zero) {
                    DateCategoryStruct categoryStruct;
                    try {
                        categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
                    }
                    catch (AccessViolationException ex) {
                        EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->categoryNode.GetStructure出错:" + ex.Message);
                        break;
                    }
                    var category = IObjectDevice.CreateDatecategory(categoryStruct, localScanMethod,DeviceTypeEnum.DaHua);
                    
                    if (category != null) {
                        curFileSystemCategories.Add(category);
                    }
                    categoryNode = categoryStruct.Next;
                }
                return curFileSystemCategories;
            }
        }

        public override bool Init(ScanMethod scanMethod, long nStartSec, long nEndSec, int nSecSize, long nTimePos, long nLBAPos,int typeIndex) {
            if(typeIndex == 16) {
                iObjectScanMethods = DHNVRScanMethods.StaticInstance; 
            }
            else {
                iObjectScanMethods = DHScanMethods.StaticInstance;
            }
            return base.Init(scanMethod, nStartSec, nEndSec, nSecSize, nTimePos, nLBAPos,typeIndex);
        }


    }
}
