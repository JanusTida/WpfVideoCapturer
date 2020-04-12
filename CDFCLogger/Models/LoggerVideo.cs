using CDFCEntities.Files;
using CDFCEntities.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDFCLogger.Models {
    /// <summary>
    /// 条目设定;
    /// </summary>
    [Table("Videos")]
    [Serializable]
    public class LoggerVideo {
        private static LoggerVideo videoNode;
        /// <summary>
        /// 由文件实体映射到日志文件结构;
        /// </summary>
        /// <param name="video">需映射的文件实体</param>
        /// <returns></returns>
        public static LoggerVideo Create(Video video) {
            if(videoNode == null) {
                videoNode = new LoggerVideo();
            }
            try {
                videoNode.FrameNO = Convert.ToInt64(video.FrameNO);
                videoNode.ChannelNO = Convert.ToInt32(video.ChannelNO);
                videoNode.StartDate = Convert.ToInt64(video.StartDate);
                videoNode.EndDate = Convert.ToInt64(video.EndDate);
                videoNode.Size = Convert.ToInt64(video.Size);
                videoNode.SizeTrue = Convert.ToInt64(video.SizeTrue);
                videoNode.StartAddress = Convert.ToInt64(video.StartAddress);
                videoNode.Integrity = (byte)video.Integrity;
                //LoggerVideo loggerVideo = new LoggerVideo {
                //    FrameNO = Convert.ToInt64(video.FrameNO),
                //    ChannelNO = Convert.ToInt32(video.ChannelNO),
                //    StartDate = Convert.ToInt64(video.StartDate ),
                //    EndDate = Convert.ToInt64(video.EndDate),
                //    Size = Convert.ToInt64(video.Size),
                //    SizeTrue = Convert.ToInt64(video.SizeTrue),
                //    StartAddress = Convert.ToInt64(video.StartAddress)
                //};
            }
            catch(OverflowException ex) {
                EventLogger.Logger.WriteLine("LoggerVideo初始化异常:" + ex.Message + "\t" + ex.Source);
                return null;
            }
            return videoNode;
        }
        
        /// <summary>
        /// 由文件日志文件记录映射到文件结构;
        /// </summary>
        /// <param name="loggerVideo"></param>
        /// <returns></returns>
        [NotMapped]
        public VideoStruct VideoStruct {
            get { 
                VideoStruct vStruct = new VideoStruct {
                    FrameNO = Convert.ToUInt32( FrameNO ),
                    ChannelNO = Convert.ToUInt32( ChannelNO ),
                    StartDate = Convert.ToUInt32( StartDate ),
                    EndDate = Convert.ToUInt32( EndDate ),
                    StartAddress = Convert.ToUInt64(StartAddress),
                    Size = Convert.ToUInt64( Size ),
                    SizeTrue = Convert.ToUInt64( SizeTrue )
                };
                return vStruct;
            }
        }
        //文件所指非托管指针;
        [NotMapped]
        public IntPtr VideoPtr { get; set; }
        //文件碎片所指非托管指针;
        [NotMapped]
        public List<IntPtr> FragmentsPtr { get; set; } = new List<IntPtr>();
        //Sqlite ef强制使用一个主键;
        [Required,Key]
        public long Id { get; set; }
        //分类日期的ID;
        [Required]
        public int CategoryId { get; set; }
        //视频本体ID;
        [Required]
        public int VideoId { get; set; }
        //帧号;
        [Required]
        public long FrameNO { get; set; }             //帧号
        //通道号;
        [Required]
        public int ChannelNO { get; set; }                  //通道号
        //开始时间;
        [Required]
        public long StartDate { get; set; }                  //文件开始时间
        //终止时间；
        [Required]
        public long EndDate { get; set; }                    //文件结束时间
        //文件大小；
        [Required]
        public long Size { get; set; }             //文件大小
        [Required]
        public long SizeTrue { get; set; }
        [Required]
        public long StartAddress { get; set; }      //文件起始地址
        //文件完整度;
        [Required]
        public byte Integrity { get; set; }
        
    }
}
