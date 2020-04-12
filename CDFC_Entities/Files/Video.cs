using CDFCEntities.Enums;
using CDFCEntities.Structs;
using System;
using System.Collections.Generic;

namespace CDFCEntities.Files {
    /// <summary>
    /// 视频本体;
    /// </summary>
    public class Video {
        /// 设定时间起始点（针对MOV)
        private static DateTime? dtIni;

        /// <summary>
        /// 构建文件本体：
        /// </summary>
        /// <param name="videoPtr">文件非托管指针</param>
        /// <param name="st">文件结构</param>
        /// <param name="dateCategory">文件日期分类</param>
        internal Video(IntPtr videoPtr,VideoStruct st,DateCategory dateCategory) {
            FrameNO = st.FrameNO;
            ChannelNO = st.ChannelNO;

            if(dateCategory != null &&( dateCategory.DeviceTypeEnum == DeviceTypeEnum.MOV || dateCategory.DeviceTypeEnum == DeviceTypeEnum.Canon)) {
                dtIni = dtIni == null ? DateTime.Parse("2000/01/01") : dtIni;
                DateTime dtItem;
                try {
                    var dateString = string.Format("{0}/{1}/{2} {3}:{4}:{5}",
                    st.StartDate.ToString().Substring(0, 4),
                    st.StartDate.ToString().Substring(4, 2),
                    st.StartDate.ToString().Substring(6, 2),
                    st.EndDate.ToString().Substring(0, 2),
                    st.EndDate.ToString().Substring(2, 2),
                    st.EndDate.ToString().Substring(4, 2));
                    if (DateTime.TryParse(dateString, out dtItem)) {
                        try {
                            st.StartDate = Convert.ToUInt32((dtItem - dtIni).Value.TotalSeconds);
                            StartDate = st.StartDate;
                        }
                        catch (Exception ex) {
                            EventLogger.Logger.WriteLine("Video->文件实体构造错误:" + ex.Message);
                        }
                    }
                    else {
                        EventLogger.Logger.WriteLine("Video->文件实体时间未成功!" + st.StartDate + st.EndDate);
                    }
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("Video->文件实体时间解析未成功:" + ex.Message);
                }
                
            }
            else {
                StartDate = st.StartDate;
                EndDate = st.EndDate;
            }

            Size = (long)st.Size;
            SizeTrue = (long)st.SizeTrue;
            OuterStartAddress = (long)st.StartAddress;
            DateCategory = dateCategory;
            VideoPtr = videoPtr;
        }

        private Video() { }
        //public byte CurRecoveringPercentage {
        //    get {
        //        try {
        //            long curSize = (long)Marshal.ReadInt64(ProgressPtr);
        //            long curPercentage = curSize * 100 / this.Size;
        //            curPercentage = curPercentage < 100 ? curPercentage:100;
        //            int percentage = Convert.ToInt32(curPercentage);
        //            return Convert.ToByte(percentage);
        //        }
        //        catch(Exception ex) {
        //            EventLogger.Logger.WriteLine("Video -> CurRecoveringPercentage出错:" + ex.Message);
        //            return 0;
        //        }
        //    }
        //}

        //public static Video Create(IntPtr videoPtr,VideoStruct st) {
        //    var video = new Video();
        //    video.FrameNO = st.FrameNO;
        //    video.ChannelNO = st.ChannelNO;
        //    video.StartDate = st.StartDate;
        //    video.EndDate = st.EndDate;
        //    video.Size = st.Size;
        //    video.SizeTrue = st.SizeTrue;
        //    video.OuterStartAddress = st.StartAddress;
        //    video.VideoPtr = videoPtr;
        //    return video;
        //}

        /// <summary>
        /// 文件碎片列表；
        /// </summary>
        public List<FileFragment> FileFragments { get; set; } = new List<FileFragment>();

        public long StartAddress {
            get {
                if (DateCategory?.ScanMethod == ScanMethod.EntireDisk) {
                    try {
                        if(FileFragments.Count != 0) {
                            long startAddress = FileFragments[0].StartAddress;
                            return (long)startAddress;
                        }
                        return OuterStartAddress;
                    }
                    catch(Exception ex) {
                        EventLogger.Logger.WriteLine("Video -> StartAddress读取错误!,碎片列表为空或不存在"+ex.Message);
                        return OuterStartAddress;
                    }
                }
                
                return OuterStartAddress;
            }
            
        }

        /// <summary>
        /// 获取文件的尾节点;
        /// </summary>
        //private FileFragment tileFragement;
        public FileFragment TileFragment {
            get {
                var fragment = new FileFragment {
                    ChannelNO = this.FileFragments[0].ChannelNO,
                    Size = this.FileFragments[0].StartAddress2,
                    StartAddress = this.FileFragments[0].StartAddress1,
                    StartDate = this.StartDate,
                    EndDate = this.EndDate
                };
                return fragment;
            }
        }
        /// <summary>
        /// 获取文件的头节点;
        /// </summary>
        public FileFragment HeaderFragment {
            get {
                if(FileFragments != null && FileFragments.Count != 0) { 
                    return this.FileFragments[0];
                }
                else {
                    EventLogger.Logger.WriteLine("Video->HeaderFragment获取出错:FileFragments为空或null");
                    return null;
                }
            }
        }

        public IntPtr VideoPtr { get;private set; } = IntPtr.Zero;
        
        public uint FrameNO { get; set; }                   //帧号
        public uint ChannelNO { get;private set; }                  //通道号
        public uint StartDate { get;private set; }                  //文件开始时间
        public uint EndDate { get; set; }                    //文件结束时间
        public long Size { get;set; }             //文件大小
        public long SizeTrue { get; set; }
        public long OuterStartAddress { get; set; }//文件起始地址
        public VideoIntegrity Integrity { get; set; } //文件完整度;

        /// <summary>
        /// 文件终止地址;
        /// </summary>
        public long EndAddress { 
            get{
                if(FileFragments.Count != 0) {
                    return FileFragments[0].StartAddress1;
                }
                else {
                    return 0;
                }
            }
        }
        public DateCategory DateCategory { get; set; }
    }
}
