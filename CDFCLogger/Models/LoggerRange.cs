using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDFCLogger.Models {
    /// <summary>
    /// 记录文件范围的数据实体;(对应需文件校验的类型);
    /// </summary>
    [Table("Ranges")]
    public class LoggerRange {
        [Required,Key]
        public int ID { get; set; }
        //初始值；
        [Required]
        public long IniValue { get; set; }
        //终止值;
        [Required]
        public long EndValue { get; set; }
    }
}
