using CDFCEntities.Files;
using CDFCEntities.Structs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDFCLogger.Models {
    [Table("Fragments")]
    [Serializable]
    public class LoggerFragment {
        /// <summary>
        ///从文件碎片实体映射日志文件碎片;
        /// </summary>
        /// <param name="fragment">需映射的实体文件碎片</param>
        /// <returns></returns>
        /// 
        public static LoggerFragment Create(FileFragment fragment) {
            LoggerFragment loggerFragment = new LoggerFragment {
                StartAddress = Convert.ToInt64(fragment.StartAddress),
                StartAddress1 = Convert.ToInt64(fragment.StartAddress1),
                StartAddress2 = Convert.ToInt64(fragment.StartAddress2),
                Size = Convert.ToInt64(fragment.Size),
                ChannelNO = fragment.ChannelNO,
                StartDate = fragment.StartDate,
                EndDate = fragment.EndDate
            };
            return loggerFragment;
        }

        /// <summary>
        /// 由日志文件碎片实体映射到实体文件碎片实体结构体;
        /// </summary>
        /// <param name="loggerFragment"></param>
        /// <returns></returns>
        [NotMapped]
        public FileFragmentStruct FragmentStruct {
            get { 
                FileFragmentStruct ffStruct = new FileFragmentStruct {
                    StartAddress = Convert.ToUInt64(StartAddress),
                    StartAddress1 = Convert.ToUInt64(StartAddress1),
                    StartAddress2 = Convert.ToUInt64(StartAddress2),
                    Size = Convert.ToUInt64(Size),
                    ChannelNO = ChannelNO,
                    EndDate = EndDate,
                    StartDate = StartDate
                };
                return ffStruct;
            }
        }
        [Required,Key]
        public int Id { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int VideoId { get; set; }
        [Required]
        public int FragmentIndex { get; set; }
        [Required]
        public long StartAddress { get; set; }
        [Required]
        public long StartAddress1 { get; set; }
        [Required]
        public long StartAddress2 { get; set; }
        [Required]
        public long Size { get; set; }
        [Required]
        public int ChannelNO { get; set; }
        [Required]
        public uint StartDate { get; set; }                  //文件开始时间
        [Required]
        public uint EndDate { get; set; }                    //文件结束时间
    }
}
