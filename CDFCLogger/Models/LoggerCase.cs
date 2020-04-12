using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CDFCLogger.Models {
    [Table("Cases")]
    public class LoggerCase {
        //案件ID
        [Required,Key]
        public int ID { get; set; }
        //案件名称;
        [Required]
        public string Name { get; set; }
        //案件保存位置;(绝对)
        [Required]
        public string Path { get; set; }
        //创建时间;
        public string CreateTime { get; set; }
        //案件类型;
        public string CaseType { get; set; }
        //案件编号;
        public string CaseNum { get; set; }
        public string Description { get; set; }
        public string Info { get; set; }
    }
}
