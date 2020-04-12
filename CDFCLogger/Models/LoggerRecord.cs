using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDFCLogger.Models {
    /// <summary>
    /// 记录实体类型;
    /// </summary>
    [Table("Records")]
    public class LoggerRecord {
        [Required,Key]
        public int ID { get; set; }
        //记录目录位置(相对);
        [Required]
        public string Path { get; set; }
    }
}
