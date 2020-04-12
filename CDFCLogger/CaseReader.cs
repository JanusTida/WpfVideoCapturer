using CDFCEntities.Interfaces;
using CDFCLogger.Contexts;
using CDFCLogger.Models;
using CDFCValueRanges;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDFCLogger {
    /// <summary>
    /// 案件读取器;
    /// </summary>
    public partial class CaseReader {
        private static readonly string defaultCaseDbName = "case.bhproj";
        private const string defaultRecordDbName = "records.db";
        /// <summary>
        /// 案件读取器构造方法;
        /// </summary>
        /// <param name="dbName">案件文件路径</param>
        public CaseReader(string casePath,string caseName):this(casePath+"/"+caseName+"/"+defaultCaseDbName) {
            
        }
        public CaseReader(string dbName) {
            if (!File.Exists(dbName)) {
                EventLogger.CaseLogger.WriteLine("LoggerReader构造错误:案件文件不存在。找不到文件"+dbName);
                throw new FileNotFoundException("Can't Find File" + dbName);    
            }
            else {
                //FileInfo dbInfo = new FileInfo(dbName);
                //casePath = (dbInfo.Directory.FullName + "/").Replace('\\', '/');
                this.dbName = dbName;
            }
        }

        public LoggerSetting GetSetting(string recordName) {
            LoggerReader reader = new LoggerReader(Case.Path + "/" + Case.Name + "/" + recordName + "/" + defaultRecordDbName, null);
            return reader.LoggerSetting;
        }

        public List<LoggerRecord> Records {
            get {
                try { 
                    using (var context = new CaseContext(dbName)) {
                        return context.Records.ToList();
                    }
                }
                catch(Exception ex) {
                    EventLogger.CaseLogger.WriteLine("CaseReader->Records错误:"+ex.Message);
                    return null;
                }
            }
        }
        
        private string dbName;

        private LoggerCase loggerCase;
        public LoggerCase Case {
            get {
                try {
                    if(loggerCase == null) {
                        using (var context = new CaseContext(dbName)) {
                            loggerCase = context.Case.First();
                        }
                    }
                    return loggerCase;
                        
                    
                }
                catch(Exception ex) {
                    EventLogger.CaseLogger.WriteLine("CaseReader->Case错误:" + ex.Message);
                    return null;
                }
            }
        }

        private LoggerSetting GetLoggerSetting(string recordName) {

            return null;
        }

        /// <summary>
        /// 获得非托管文件列表指针头;
        /// </summary>
        /// <param name="recordName"></param>
        /// <param name="iObjectDevice"></param>
        /// <returns></returns>
        public IntPtr GetFileList(string recordName,IObjectDevice iObjectDevice, out List<IntPtr> unManagedPtrs) {
            try {
                var reader = new LoggerReader(Case.Path+"/"+Case.Name + "/" + recordName + "/" + defaultRecordDbName, iObjectDevice);
                unManagedPtrs = reader.UnManagedPtrs;
                return reader.FileList;
            }
            catch(Exception ex) {
                EventLogger.CaseLogger.WriteLine("CaseReader获取文件列表错误:" + ex.Message);
                unManagedPtrs = null;
                return IntPtr.Zero;
            }
        }
        /// <summary>
        /// 获得文件系统文件范围;(对应需校验类型)
        /// </summary>
        /// <param name="recordName"></param>
        /// <param name="iObjectDevice"></param>
        /// <returns></returns>
        public ValueRangeList RangeList(string recordName,IObjectDevice iObjectDevice) {
            try {
                var reader = new LoggerReader(Case.Path + "/" + Case.Name + "/" + recordName + "/" + defaultRecordDbName, iObjectDevice);
                return reader.RangeList;
            }
            catch(Exception ex) {
                EventLogger.CaseLogger.WriteLine("CaseReader->RangeList错误:"+ex.Message);
                return null;
            }
        }
        //public List<IntPtr> GetUnManagedPtrs(string recordName,IObjectDevice iObjectDevice) {
        //    try {
        //        var reader = new LoggerReader(Case.Path + "/" + Case.Name + "/" + recordName + "/" + defaultRecordDbName, iObjectDevice);
        //        return reader.UnManagedPtrs;
        //    }
        //    catch (Exception ex) {
        //        EventLogger.CaseLogger.WriteLine("CaseReader->GetUnManagedPtrs错误:" + ex.Message);
        //        return null;
        //    }
        //}
    }
    public partial class CaseReader {
        /// <summary>
        /// 最近的案件表;
        /// </summary>
        public static List<LoggerCase> RecentCases {
            get {
                try {
                    using (RecentCasesContext context = new RecentCasesContext()) {
                        return context.Cases.ToList();
                    }
                }
                catch (Exception ex) {
                    EventLogger.CaseLogger.WriteLine("CaseReader->RecentCases错误" + ex.Message);
                    return null;
                }
            }
        }
    }

}
