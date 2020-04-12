using CDFCEntities.Files;
using CDFCLogger.Contexts;
using CDFCLogger.Models;
using CDFCStatic.IOMethods;
using CDFCValueRanges;
using EventLogger;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace CDFCLogger {
    /// <summary>
    /// 案件写入器;
    /// </summary>
    public partial class CaseWriter {
        /// <summary>
        /// 案件写入器的构造方法;
        /// </summary>
        /// <param name="savedPath">案件保存的路径</param>
        /// <param name="caseName">案件名称</param>
        public CaseWriter(LoggerCase Case) {
            if(Case == null) {
                EventLogger.CaseLogger.WriteLine("CaseWriter构造错误:案件不得为空!");
            }
            //验证是否存在目标路径,若不存在则创建该路径;
            IOStaticMethods.ValidateDirectory(Case.Path+"/"+Case.Name);
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.FailIfMissing = false;
            sb.DataSource = Case.Path + "/" + Case.Name+"/" + defaultCaseDbName;
            sb.Pooling = true;
            this.SavedPath = Case.Path;
            
            //由于sqlite ef不支持codefirst，所以需要手动创建表结构;
            var conn = new SQLiteConnection(sb.ConnectionString);

            #region 创建案件表;
            SQLiteCommand commCreateCase = new SQLiteCommand(
                            @"Create Table if not exists Cases(
                            ID INTEGER PRIMARY KEY NOT NULL,
                            Name TEXT  NOT NULL,
                            Path TEXT NOT NULL,
                            CreateTime TEXT NULL,
                            CaseType TEXT NULL,
                            CaseNum TEXT NULL,
                            Description TEXT NULL,
                            Info TEXT NULL
                            )", conn);
            #endregion

            #region 创建记录表;
            SQLiteCommand commCreateRecords = new SQLiteCommand(
                            @"Create Table if not exists Records(
                            ID INTEGER PRIMARY KEY NOT NULL,
                            Path TEXT NOT NULL
                            )", conn);
            #endregion



            //防止出错,回滚更改;
            SQLiteTransaction ts = null;
            #region 执行过程;
            try {
                conn.Open();
                ts = conn.BeginTransaction();
                commCreateCase.ExecuteNonQuery();
                commCreateRecords.ExecuteNonQuery();
                ts.Commit();
            }
            catch(SQLiteException ex) {
                if(ts != null) {
                    ts.Rollback();
                }
                CaseLogger.WriteLine("CaseWriter构建文件错误:" + ex.Message + ex.InnerException == null ? "" : ex.InnerException.Message);
            }
            catch(Exception ex) {
                if (ts != null) {
                    ts.Rollback();
                }
                CaseLogger.WriteLine("CaseWriter构建文件未知错误:" + ex.Message);
            }
            finally {
                conn.Close();
            }
            #endregion
            
            CaseContext context = null;

            #region 清空案件,记录表;
            try {
                context = new CaseContext(SavedPath + "/" + Case.Name+"/" + defaultCaseDbName);
                context.Case.RemoveRange(context.Case);
                //context.Records.RemoveRange(context.Records);
            }
            catch (Exception ex) {
                EventLogger.CaseLogger.WriteLine("CaseWriter->构造清空案件未知错误:" + ex.Message);
            }
            #endregion
            try {
                context.Case.Add(Case);
                context.SaveChanges();
                this.Case = Case;
            }
            catch (Exception ex) {
                EventLogger.CaseLogger.WriteLine("CaseWriter错误:" + ex.Message);   
            }
        }

        //默认的案件存储器名称;
        private static string defaultCaseDbName = "case.bhproj";
        ///// <summary>
        ///// 写入案件;
        ///// </summary>
        ///// <param name="loggerCase"></param>
        ///// <returns></returns>
        //public bool WriteCase(LoggerCase loggerCase) {
            
        //}

        //案件本体;
        public LoggerCase Case { get;private set; }
        //案件文件所保存的路径;
        public string SavedPath { get; private set; }
        
    }

    /// <summary>
    /// 关于记录相关;
    /// </summary>
    public partial class CaseWriter {
        /// <summary>
        /// 写入文件分类列表;
        /// </summary>
        /// <param name="categories">需写入的文件列表</param>
        /// <param name="rangeList">对应需文件校验的类型</param>
        /// <returns>执行是否成功</returns>
        public bool WriterCategories(List<DateCategory> categories, string recordName, LoggerSetting setting, ValueRangeList rangeList = null) {
            if (categories == null || recordName == null) {
                EventLogger.CaseLogger.WriteLine("CaseWriter->WriterCategories错误:参数" + recordName == null ? "案件名称" : "" + "不得为空!");
                return false;
            }
            if (Case == null) {
                EventLogger.CaseLogger.WriteLine("CaseWriter->WriterCategories错误:案件不得为空!");
                return false;
            }

        
            LoggerWriter writer = new LoggerWriter(SavedPath+"/"+Case.Name, recordName);
            
            if (categories.Count() == 0) {
                return true;
            }
            try {
                // 写入设定;
                var set = writer.WriteSetting(setting);
                if (set) {
                    if(rangeList != null) {
                        writer.WriteRanges(rangeList);
                    }
                    try { 
                        using(var context = new CaseContext(SavedPath+"/"+Case.Name+"/"+ defaultCaseDbName)) {
                            context.Records.Add(new LoggerRecord { Path=recordName});
                            context.SaveChanges();
                        }
                        writer.WriteCategories(categories);
                        return true;
                    }
                    catch(Exception ex) {
                        EventLogger.CaseLogger.WriteLine("CaseWriter->WriterCategories错误:写入设定记录成功!"+ex.Message);
                        return false;
                    }
                }
                //若失败,则返回空;
                else {
                    EventLogger.CaseLogger.WriteLine("CaseWriter->WriterCategories错误:写入设定未成功!");
                    return false;
                }
                
            }
            catch (Exception ex) {
                EventLogger.CaseLogger.WriteLine("CaseWriter->WriterCategories写入记录错误!"+ex.Message);
                return false;
            }
        }

    }

    public partial class CaseWriter {
        /// <summary>
        /// 写入最近案件;
        /// </summary>
        /// <param name="loggerCase"></param>
        /// <returns></returns>
        public static bool WriteRecentcase(LoggerCase loggerCase) {
            try {
                using (RecentCasesContext context = new RecentCasesContext()) {
                    var connString = context.Database.ProviderName;
                    
                    //由于sqlite ef不支持codefirst，所以需要手动创建表结构;
                    var conn = new SQLiteConnection(connString);
                    #region 手动创建案件表;
                    SQLiteCommand commCreateCase = new SQLiteCommand(
                            @"Create Table if not exists Cases(
                            ID INTEGER PRIMARY KEY NOT NULL,
                            Name TEXT  NOT NULL,
                            Path TEXT NOT NULL,
                            CreateTime TEXT NULL,
                            CaseType TEXT NULL,
                            CaseNum TEXT NULL,
                            Description TEXT NULL,
                            Info TEXT NULL
                            )", conn);
                    #endregion
                    SQLiteTransaction ts = null;
                    try {
                        conn.Open();
                        ts = conn.BeginTransaction();
                        commCreateCase.ExecuteNonQuery();
                        ts.Commit();
                    }
                    catch(SQLiteException ex) {
                        ts.Rollback();
                        EventLogger.CaseLogger.WriteLine("CaseWriterWriter->RecentCase创建最近案件错误。"+ex.Message+ex.InnerException==null?"":ex.InnerException.Message);
                        return false;
                    }
                    catch(Exception ex) {
                        EventLogger.CaseLogger.WriteLine("CaseWriterWriter->RecentCase创建最近案件未知错误。" + ex.Message);
                        return false;
                    }
                    context.Cases.Add(loggerCase);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex) {
                EventLogger.CaseLogger.WriteLine("CaseReader->RecentCases写入最近案件错误" + ex.Message);
                return false;
            }
        }
    }
}
