using CDFCEntities.Abstracts;
using CDFCEntities.CScanMethods;
using CDFCEntities.Enums;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCEntities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using CDFC.Util.PInvoke;

namespace CDFCEntities.Scanners {
    public class WFSScanner : DefaultObjectScanner {
        public WFSScanner(IObjectDevice iObjectDevice) : base(DeviceTypeEnum.WFS,iObjectDevice) { }
        //private DeviceTypeEnum localDeviceType = DeviceTypeEnum.WFS;

        private ScanMethod localScanMethod = ScanMethod.EntireDisk;

        /// <summary>
        /// 获得当前的文件列表(文件系统，做全盘扫描时需要)
        /// </summary>
        private List<DateCategory> curFileSystemCategories;
        public List<DateCategory> CurFileSystemCategories {
            [HandleProcessCorruptedStateExceptions]
            get {
                if (curFileSystemCategories == null) {
                    curFileSystemCategories = new List<DateCategory>();
                }
                lock (curFileSystemCategories) {
                    IntPtr categoryPtr = IntPtr.Zero;
                    IntPtr categoryNode;
                    try {
                        var wfsScanMethods = iObjectScanMethods as WFSScanMethods;
                        if (wfsScanMethods != null) {
                            categoryPtr = wfsScanMethods.FileSystemFileList;
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
                        //Check the category is existing in the list;
                        var category = curFileSystemCategories.FirstOrDefault(p => p.Date == categoryStruct.Date);
                        //if it is not,try to create it and add it into the list;
                        if (category == null) {
                            category = IObjectDevice.CreateDatecategory(categoryStruct, localScanMethod,DeviceTypeEnum.WFS);
                            
                            if (category != null) {
                                curFileSystemCategories.Add(category);
                            }
                        }
                        //If not just make sure every video in the list is unique;
                        else {
                            var videoPtr = categoryStruct.File;
                            var videoNode = videoPtr;
                            while (videoNode != IntPtr.Zero) {
                                VideoStruct videoStruct;

                                #region 尝试获取文件结构体，若失败，则退出;
                                try {
                                    videoStruct = videoNode.GetStructure<VideoStruct>();
                                }
                                catch (AccessViolationException ex) {
                                    EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->videoNode.GetStructure出错!" 
                                        + ex.Message + ex.Source);
                                    break;
                                }
                                
                                //验证文件;
                                //videoStruct.ChannelNO > 200 || 
                                if (videoStruct.Size == 0 || (long)videoStruct.StartAddress > IObjectDevice.Size) {
                                    videoNode = videoStruct.Next;
                                    continue;
                                }
                               
                                #endregion

                                var video = category.CreateVideo(videoNode, videoStruct);

                                //查询非托管环境下是否存在已有指针,以确定唯一项;
                                Video videoEntity = category.Videos.FirstOrDefault(p => p.VideoPtr == videoNode);

                                #region 若没有则添加新的项;
                                if (videoEntity == null) {
                                    category.Videos.Add(video);
                                    video.DateCategory = category;
                                }
                                #endregion

                                #region 若存在，则刷新值;
                                else {
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
                    return curFileSystemCategories;
                }
            }
        }

        /// <summary>
        /// 获得当前未分配文件列表;
        /// </summary>
        /// <param name="iObjectDevice">所属的设备对象</param>
        //private List<DateCategory> curCategories;
        //public List<DateCategory> CurUnlocatedCategories {
        //    [HandleProcessCorruptedStateExceptions]
        //    get {
        //        if (curCategories == null) {
        //            curCategories = new List<DateCategory>();
        //        }

        //        lock (curCategories) {
        //            IntPtr categoryPtr = IntPtr.Zero;
        //            IntPtr categoryNode;
        //            try {
        //                categoryPtr = iObjectScanMethods.FileList;
        //            }
        //            catch (Exception ex) {
        //                EventLogger.Logger.WriteLine("DefaultObject->CurDateCategories出错:" + ex.Message);
        //                return curCategories;
        //            }
        //            categoryNode = categoryPtr;

        //            //轮询文件分类列表;
        //            while (categoryNode != IntPtr.Zero) {
        //                DateCategoryStruct categoryStruct;
        //                try {
        //                    categoryStruct = categoryNode.GetStructure<DateCategoryStruct>();
        //                }
        //                catch (AccessViolationException ex) {
        //                    EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->categoryNode.GetStructure出错:" + ex.Message);
        //                    break;
        //                }
        //                //Check the category is existing in the list;
        //                var category = curCategories.FirstOrDefault(p => p.Date == categoryStruct.Date);
        //                //if it is not,try to create it and add it into the list;
        //                if (category == null) {
        //                    category = IObjectDevice.CreateDatecategory(categoryStruct, localScanMethod,DeviceTypeEnum.WFS);
        //                    category.DeviceTypeEnum = localDeviceType;
        //                    if (category != null) {
        //                        curCategories.Add(category);
        //                    }
        //                }
        //                //If not just make sure every video in the list is unique;
        //                else {
        //                    var videoPtr = categoryStruct.File;
        //                    var videoNode = videoPtr;
        //                    while (videoNode != IntPtr.Zero) {
        //                        VideoStruct videoStruct;

        //                        #region 尝试获取文件结构体，若失败，则退出;
        //                        try {
        //                            videoStruct = videoNode.GetStructure<VideoStruct>();
        //                        }
        //                        catch (AccessViolationException ex) {
        //                            EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->videoNode.GetStructure出错!" + ex.Message + ex.Source);
        //                            break;
        //                        }
        //                        //验证文件;
        //                        if (videoStruct.ChannelNO > 200 || videoStruct.Size == 0) {
        //                            videoNode = videoStruct.Next;
        //                            continue;
        //                        }
        //                        #endregion

        //                        var video = category.CreateVideo(videoNode, videoStruct);

        //                        //查询非托管环境下是否存在已有指针,以确定唯一项;
        //                        Video videoEntity = category.Videos.FirstOrDefault(p => p.VideoPtr == videoNode);

        //                        #region 若没有则，添加新的项;
        //                        if (videoEntity == null) {
        //                            category.Videos.Add(video);
        //                            video.DateCategory = category;
        //                        }
        //                        #endregion

        //                        #region 若存在，则刷新值;
        //                        else {
        //                            videoEntity.FileFragments = video.FileFragments;
        //                            videoEntity.Size = video.Size;
        //                            videoEntity.EndDate = video.EndDate;
        //                        }
        //                        #endregion

        //                        videoNode = videoStruct.Next;
        //                    }
        //                }
        //                categoryNode = categoryStruct.Next;
        //            }
        //            return curCategories;

        //        }
        //    }
        //}
    }
}
