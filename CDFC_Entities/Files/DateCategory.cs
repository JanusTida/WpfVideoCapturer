using CDFCEntities.Enums;
using CDFCEntities.Interfaces;
using CDFCEntities.Structs;
using CDFC.Util.PInvoke;
using System;
using System.Collections.Generic;


namespace CDFCEntities.Files {
    /// <summary>
    /// 视频日期分类;
    /// </summary>
    public class DateCategory {
        private DateCategory(DateCategoryStruct st, ScanMethod scanMethod) {
            Videos = new List<Video>(); 
            
        }
        
        public ScanMethod ScanMethod { get;private set; }
        //对象类型;
        public Enums.DriveType DriveType { get; set; }
        
        /// <summary>
        /// 构建文件分类实体;并构建文件列表;
        /// </summary>
        /// <param name="st">文件结构</param>
        /// <param name="scanMethod">扫描方法</param>
        /// <param name="iObjectDevice">文件分类所属对象</param>
        /// <returns></returns>
        internal DateCategory(DateCategoryStruct st, ScanMethod scanMethod, IObjectDevice iObjectDevice,DeviceTypeEnum deviceType,bool isFromLogger) {
            Date = st.Date;
            ScanMethod = scanMethod;
            IntPtr ptr = st.File;
            DriveType = iObjectDevice.DriveType;
            IObjectDevice = iObjectDevice;
            DeviceTypeEnum = deviceType;
            
            while (ptr != IntPtr.Zero) {
                var videoStruct = ptr.GetStructure<VideoStruct>();
                //若文件大小过小，或通道号过大，将无法写入;
                //videoStruct.ChannelNO >= 200
                if ( videoStruct.Size > 512) {
                    //对于大华通道号需加一;
                    videoStruct.ChannelNO += !isFromLogger&& DeviceTypeEnum == DeviceTypeEnum.DaHua ? (uint) 1 : 0;
                    // {&&( videoStruct.StartAddress <= iObjectDevice.Size || scanMethod == ScanMethod.FileSystem))
                    Video video = CreateVideo(ptr, videoStruct,isFromLogger);
                    Videos.Add(video);
                }
                
                ptr = videoStruct.Next;
            }

        }
        

        /// <summary>
        /// 构建Video实体;
        /// </summary>
        /// <param name="videoPtr">文件结构指针</param>
        /// <param name="st">文件结构</param>
        /// <param name="category">时间分类</param>
        /// <param name="recoverFunc">文件恢复委托</param>
        /// <returns></returns>
        public Video CreateVideo(IntPtr videoPtr, VideoStruct st,bool isFromLogger = false) {
            Video video = new Video(videoPtr,st,this);
            IntPtr iniAddressPtr = st.stStAdd;
            FileFragmentStruct fileFragmentStruct;
            try {
                //StreamWriter sw = new StreamWriter("D://Test.txt",true);
                
                while (iniAddressPtr != IntPtr.Zero) {
                    fileFragmentStruct = iniAddressPtr.GetStructure<FileFragmentStruct>();
                    fileFragmentStruct.ChannelNO = DeviceTypeEnum == DeviceTypeEnum.DaHua?fileFragmentStruct.ChannelNO+1 : fileFragmentStruct.ChannelNO;
                    //if (st.StartDate == 1042620578) {
                    //    sw.WriteLine(fileFragmentStruct.StartAddress);
                    //}
                    var fileFragment = FileFragment.Create(fileFragmentStruct);
                    
                    video.FileFragments.Add(fileFragment);
                    iniAddressPtr = fileFragmentStruct.Next;
                }
                //if(st.StartDate == 1042620578) {
                //    sw.WriteLine();
                //}
                //sw.Close();
            }
            catch(AccessViolationException ex) {
                EventLogger.Logger.WriteLine("Video->CreateVideo错误:" + ex.Message);
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("Video->CreateVideo错误:" + ex.Message);
            }
            return video;
        }

        public uint Date { get; private set; }

        public List<Video> Videos { get; private set; } = new List<Video>();
  
        public IObjectDevice IObjectDevice { get;private set; }
            
        public DeviceTypeEnum DeviceTypeEnum { get;private set; }
}
    
}
