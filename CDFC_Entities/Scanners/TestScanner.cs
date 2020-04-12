using CDFC.Util;
using CDFCEntities.Enums;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCEntities.Recoverers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using CDFCEntities.Structs;
using CDFC.Util.PInvoke;

namespace CDFCEntities.Scanners {
    public enum SearchType {
        SearchType_FULL,
        SearchType_FREE,
        SearchType_FS,
        SearchType_QUICK
    };

    public class TestScanner : GenericStaticInstance<TestScanner> {
        private const string entryName = "cdfcproject2.dll";
        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr object_searchstart(IntPtr hDisk, SearchType eType, ulong nStartSec, ulong nEndSec,
    int nSecSize, ulong nAreaSize, int nClusterSize, ulong nLBAPos, bool bJournal, IntPtr nError);
        
        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void object_exit(IntPtr stFile);

        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void object_stop();

        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void object_get_data(IntPtr nOffsetSec, IntPtr nFileCount);

        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool object_preview(IntPtr szFile, SearchType eType, IntPtr hDisk, IntPtr szBuffer, ulong nBuffSize);

        [DllImport(entryName, CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        private static extern void object_date_converter(uint nDate, IntPtr date);

        
        public IObjectDevice IObjectDevice { get; set; }

        public TestRecoverer DefaultRecoverer => TestRecoverer.StaticInstance;

        public (long curSec,long fileCount)? GetData() {
            var offSetPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
            var fileCoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
            try {
                object_get_data(offSetPtr, fileCoPtr);
                return (Marshal.ReadInt64(offSetPtr), Marshal.ReadInt64(fileCoPtr));
            }
            catch {
                return null;
            }
            finally {
                Marshal.FreeHGlobal(offSetPtr);
                Marshal.FreeHGlobal(fileCoPtr);
            }
        }
        
        public DateTime? DateConvert(uint date) {
            DateTime? dt = null;
            int byteSize = Marshal.SizeOf(typeof(byte));
            IntPtr dateNumsPtr = Marshal.AllocHGlobal(6 * byteSize);
            object_date_converter(date, dateNumsPtr);
            var dateNums = new short[6];
            for (int index = 0; index < 6; index++) {
                dateNums[index] = Marshal.ReadByte(dateNumsPtr + index * byteSize);
            }
            Marshal.FreeHGlobal(dateNumsPtr);
            try {
                dt = new DateTime(dateNums[0] + 2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                return dt.Value;
            }
            //若时间构造失败;
            catch (AccessViolationException ex) {
                EventLogger.Logger.WriteLine("非托管转换时间错误!" + ex.Message);
                dt = new DateTime(2000, 1, 1);
                return null;
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("转换时间错误!" + ex.Message);
                return null;
            }
        }
        
        public bool Init(ScanMethod scanMethod, long nStartSec, long nEndSec, int nSecSize, long nTimePos, long nLBAPos, int typeIndex) {
            throw new NotImplementedException();
        }

        private IntPtr list;
        public void SearchStart() {
            try {
                list = object_searchstart(
                    IObjectDevice.Handle,
                    SearchType.SearchType_FULL,
                    0, 
                    (ulong)( IObjectDevice.Size / IObjectDevice.SectorSize),
                    IObjectDevice.SectorSize, 0, 0, 0, false,IntPtr.Zero);
            }
            catch {
                throw;
            }
            
        }

        public bool SetClusterSize(int clusterSize) {
            throw new NotImplementedException();
        }

        public void SetRegionsize(long size) {
            throw new NotImplementedException();
        }

        public void Stop() {
            object_stop();
        }

        public void Exit() {
            if(list != IntPtr.Zero) {
                object_exit(list);
                list = IntPtr.Zero;
            }
            
        }

        public IEnumerable<DateCategory> GetCateGories() {
            if(list == IntPtr.Zero) {
                return null;
            }

            var curCategories = new List<DateCategory>();
            
            lock (curCategories) {
                IntPtr categoryPtr = IntPtr.Zero;
                IntPtr categoryNode;
                try {
                    categoryPtr = list;
                }
                catch (Exception ex) {
                    EventLogger.Logger.WriteLine("DefaultObject->CurDateCategories出错:" + ex.Message);
                    return curCategories;
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
                    var category = curCategories.FirstOrDefault(p => p.Date == categoryStruct.Date);
                    
                    //if it is not,try to create it and add it into the list;
                    if (category == null) {
                        category = IObjectDevice.CreateDatecategory(categoryStruct, ScanMethod.EntireDisk, DeviceTypeEnum.Unknown);
                        if (category != null) {
                            curCategories.Add(category);
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
                                EventLogger.Logger.WriteLine("DefaultObjectScanner->Curcategories->videoNode.GetStructure出错!" + ex.Message + ex.Source);
                                break;
                            }

                            //验证文件;
                            //若文件通道号过大或文件大小过小,或文件起始范围过大（非文件系统扫描）,则继续轮询;
                            if (videoStruct.Size < 512) {//videoStruct.ChannelNO >= 200
                                                         //||(scanMethod == ScanMethod.FileSystem&&videoStruct.StartAddress >= IObjectDevice.Size)
                                                         //videoStruct.ChannelNO > 200 || videoStruct.Size <= 1048576
                                videoNode = videoStruct.Next;
                                continue;
                            }
                            else {

                            }
                            #endregion

                            var video = category.CreateVideo(videoNode, videoStruct);
                           

                            //查询非托管环境下是否存在已有指针,以确定唯一项;
                            Video videoEntity = category.Videos.FirstOrDefault(p => p.VideoPtr == videoNode);

                            #region 若没有，添加新的项;
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
                
                return curCategories;
            }
        }
    }
}
