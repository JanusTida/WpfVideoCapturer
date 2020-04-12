using CDFCLogger.Models;
using Microsoft.EntityFrameworkCore;

namespace CDFCLogger.Contexts {
    /// <summary>
    /// 案件上下文;
    /// </summary>
    public class CaseContext:DbContext {
        public DbSet<LoggerCase> Case { get; set; }
        public DbSet<LoggerRecord> Records { get; set; }
        /// <summary>
        /// 数据库上下文的构造方法;
        /// </summary>
        /// <param name="connString">连接字符串</param>
        public CaseContext(string connString){
            
        }

    }
}
