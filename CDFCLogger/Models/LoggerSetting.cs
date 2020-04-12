using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDFCLogger.Models {
    /// <summary>
    /// 当前扫描的设定;
    /// </summary>
    [Table("Setting")]
    public class LoggerSetting {
        //SQLite强制使用一个主键;
        [Required, Key]
        public long Id { get; set; }
        //对象的驱动类型;
        [Required]
        public int DriveType { get; set; }
        //入口类型;
        [Required]
        public int EntranceType { get; set; }
        //对象的驱动品牌类型;
        [Required]
        public int DeviceTypeEnum { get; set; }
        //对象的设备类型;
        [Required]
        public int DeviceVersionType { get; set; }
        //簇大小;
        [Required]
        public int ClusterSize { get; set; }
        //时间偏移量;
        [Required]
        public long TimeOffset { get; set; }
        //文件LBA;
        [Required]
        public long LBAOffset { get; set; }
        //扫描方式;
        [Required]
        public int ScanMethod { get; set; }
        //拓展名;
        [Required]
        public string ExtensionName { get; set; }
        //起始扫描扇区;
        [Required]
        public long IniSector { get; set; }
        //终止扫描扇区;
        [Required]
        public long EndSector { get; set; }
        //扇区大小;
        [Required]
        public int SectorSize { get; set; }
        //对象大小;
        [Required]
        public long Size { get; set; }
        [Required]
        public int IsMP4Class { get; set; }
        [Required]
        public int RegionSize { get; set; }
        //产品序列号;(对应设备类型)
        public string SerialNumber { get; set; }
        //分区号;(对应分区类型)
        public string Sign { get; set; }
        //文件路径；(对应镜像类型)
        public string ImgPath { get; set; }
    }
}
