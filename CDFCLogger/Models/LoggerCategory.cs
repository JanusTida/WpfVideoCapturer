using CDFCEntities.Files;
using CDFCEntities.Structs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CDFCLogger.Models {
    /// <summary>
    /// 日志时间分类;
    /// </summary>
    [Table("Categories")]
    public class LoggerCategory {
        /// <summary>
        /// 由时间分类实体映射到日志时间分类
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static LoggerCategory Create(DateCategory category) {
            LoggerCategory loggerCategory = new LoggerCategory();
            loggerCategory.Date =  category.Date;
            return loggerCategory;
        }
        /// <summary>
        /// 由日志时间分类映射到时间分类结构;
        /// </summary>
        /// <param name="loggerCategory">需要映射的日志时间分类</param>
        /// <returns></returns>
        [NotMapped]
        public DateCategoryStruct CategoryStruct {
            get { 
                DateCategoryStruct dcStruct = new DateCategoryStruct {
                    Date = Convert.ToUInt32( Date )
                };
                return dcStruct;
            }
        }

        [Required,Key]
        public long Id { get; set; }
        //自定义主键;
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public long Date { get; set; }
    }
}
