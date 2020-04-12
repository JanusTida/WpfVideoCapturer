using CDFCLogger.Models;
using Microsoft.EntityFrameworkCore;

namespace CDFCLogger.Contexts {
    public class LoggerContext:DbContext {
        //设定的数据映射;
        public DbSet<LoggerSetting> Setting { set; get; }
        //分类的数据映射;
        public DbSet<LoggerCategory> Categories { get; set; }
        //视频的数据映射;
        public DbSet<LoggerVideo> Videos { get; set; }
        //文件范围的数据映射;
        public DbSet<LoggerRange> Ranges { get; set; }
        /// <summary>
        /// 数据库上下文的构造方法;
        /// </summary>
        /// <param name="connString">连接字符串</param>
        public LoggerContext(string connString){
            //base.Database.Connection.ConnectionString = "data source = "+ connString;
        }
        //protected override void OnModelCreating(DbModelBuilder modelBuilder) {
        //    modelBuilder.Entity<LoggerSetting>().ToTable("Setting");
        //}
    }
}
