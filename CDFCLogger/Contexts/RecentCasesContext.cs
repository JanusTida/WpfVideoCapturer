using CDFCLogger.Models;
using Microsoft.EntityFrameworkCore;

namespace CDFCLogger.Contexts {
    /// <summary>
    /// 最近案件上下文;
    /// </summary>
    public class RecentCasesContext:DbContext {
        public DbSet<LoggerCase> Cases { get; set; }
        public RecentCasesContext()  {

        }
    }
}
