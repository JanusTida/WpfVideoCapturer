using CDFCEntities.Files;
using CDFCLogger.Contexts;
using CDFCStatic.IOMethods;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using CDFCLogger.Models;
using CDFCValueRanges;
using System.Data.SQLite;

namespace CDFCLogger {
    public class LoggerWriter
    {
        private LoggerContext context;
        private string dbConnString {
            get {
                return savedPath+"/" + dbName+"/records.db";
            }
        }
        private string savedPath;
        private string dbName;
        public LoggerWriter(string savedPath,string dbName) {
            //验证是否存在目标路径,若不存在则创建该路径;
            IOStaticMethods.ValidateDirectory(savedPath+"/"+dbName);
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder();
            sb.FailIfMissing = false;

            this.savedPath = savedPath;
            this.dbName = dbName;
            sb.DataSource = dbConnString;
            sb.Pooling = true;

            //由于sqlite ef不支持codefirst，所以需要手动创建表结构;
            var conn = new SQLiteConnection(sb.ConnectionString);
            #region 创建设定表;
            SQLiteCommand commCreateSetting = new SQLiteCommand(
                            @"Create Table if not exists Setting(
                            Id INTEGER PRIMARY KEY NOT NULL,
                            DriveType INTEGER  NOT NULL,
                            EntranceType INTEGER NOT NULL,
                            DeviceTypeEnum INTEGER NOT NULL,
                            DeviceVersionType INTEGER NOT NULL,
                            ClusterSize INTEGER NOT NULL,
                            TimeOffset INTEGER NOT NULL,
                            LBAOffset INTEGER NOT NULL,
                            ScanMethod INTEGER NOT NULL,
                            ExtensionName Text not null,
                            IniSector INTEGER not null,
                            EndSector INTEGER not null,
                            SectorSize INTEGER not null,
                            Size INTEGER not null,
                            IsMP4Class int not null,
                            RegionSize int not null,
                            SerialNumber Text null,
                            ImgPath TEXT null,
                            Sign TEXT null
                            )", conn);
            #endregion
            #region 创建时间分类表;
            SQLiteCommand commCreateCategories = new SQLiteCommand(
                                @"create table if not exists Categories(
                                        Id INTEGER not null primary key,
                                        CategoryId INTEGER NOT NULL,
                                        Date INTEGER not null
                                )"
                            , conn);
            #endregion
            #region 创建视频列表;
            SQLiteCommand commCreateVideos = new SQLiteCommand(
                                @"create table if not exists  Videos(
                                        Id INTEGER not null primary key,
                                        CategoryId INTEGER NOT NULL,
                                        VideoId INTEGER NOT NULL,
                                        FrameNO INTEGER NOT NULL,
                                        ChannelNO INTEGER NOT NULL,
                                        StartDate INTEGER NOT NULL,
                                        EndDate INTEGER NOT NULL,
                                        Size INTEGER NOT NULL,
                                        SizeTrue INTEGER NOT NULL,
                                        StartAddress INTEGER NOT NULL,
                                        Integrity INTEGER NOT NULL
                                )"
                            , conn);
            #endregion
            #region 创建文件范围列表；
            SQLiteCommand commCreateRanges = new SQLiteCommand(
                                @"create table if not exists  Ranges(
                                    ID INTEGER not null primary key,
                                    IniValue INTEGER NOT NULL,
                                    EndValue INTEGER NOT NULL)",
                                conn);
            #endregion
            ////创建碎片列表;
            //SQLiteCommand commCreateFragments = new SQLiteCommand(
            //                    @"Create Table Fragments(
            //                            Id INTEGER NOT NULL PRIMARY KEY,
            //                            CategoryId INTEGER NOT NULL,
            //                            VideoId INTEGER NOT NULL,
            //                            FragmentIndex INTEGER NOT NULL,
            //                            StartAddress INTEGER NOT NULL,
            //                            StartAddress1 INTEGER NOT NULL,
            //                            StartAddress2 INTEGER NOT NULL,
            //                            Size INTEGER NOT NULL,
            //                            ChannelNO INTEGER NOT NULL,
            //                            StartDate INTEGER NOT NULL,
            //                            EndDate INTEGER NOT NULL
            //                    )",
            //    conn);

            //防止出错,回滚更改;
            SQLiteTransaction ts = null;
            try { 
                conn.Open();
                ts = conn.BeginTransaction();
                commCreateSetting.ExecuteNonQuery();
                commCreateCategories.ExecuteNonQuery();
                commCreateVideos.ExecuteNonQuery();
                commCreateRanges.ExecuteNonQuery();
                //commCreateFragments.ExecuteNonQuery();
                ts.Commit();
            }
            catch(SQLiteException ex) {
                if(ts != null) { 
                    ts.Rollback();
                }
                EventLogger.CaseLogger.WriteLine("LoggerWriter构建文件错误:"+ ex.Message+(ex.InnerException==null?"":ex.InnerException.Message));

            }
            catch(Exception ex) {
                EventLogger.CaseLogger.WriteLine("LoggerWriter构建文件未知错误:" + ex.Message);
            }
            finally {
                conn.Close();
            }

            conn.Dispose();
        }
        
        /// <summary>
        /// 写入扫描设定;
        /// </summary>
        /// <param name="setting">设定参数记录的对象</param>
        /// <returns></returns>
        public bool WriteSetting(LoggerSetting setting) {
            context = new LoggerContext(dbConnString);
            try {
                context.Setting.RemoveRange(context.Setting);
                context.Setting.Add(setting);
                context.SaveChanges();
                //var set = context.Setting.FirstOrDefault();
            }
            catch(System.Exception ex) {
                EventLogger.CaseLogger.WriteLine(ex.Message);
                return false;
            }
            finally {
                context.Dispose();
                context = null;
            }
            return true;
        }

        /// <summary>
        /// 写入文件记录实体内容;
        /// </summary>
        /// <param name="categories">写入的内容</param>
        /// <returns></returns>
        public bool WriteCategories(List<DateCategory> categories)  {
            Stream stream = new FileStream(savedPath+"/"+dbName+"/tick.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            stream.SetLength(0);
            BinaryFormatter formatter = new BinaryFormatter();
            try {
                using (context = new LoggerContext(dbConnString)) {

                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                    //context.ChangeTracker.vale.ValidateOnSaveEnabled = false;
                    var count = context.Categories.Count();

                    //清空分类列表;
                    foreach (var p in context.Categories) {
                        context.Categories.Remove(p);
                    }
                    //清空视频实体列表;
                    foreach (var p in context.Videos) {
                        context.Videos.Remove(p);
                    }
                    context.SaveChanges();
                }
            }
            catch(Exception ex) {
                EventLogger.CaseLogger.WriteLine("LoggerWritter->WriteCategories记录清空错误:"+ex.Message);
                return false;
            }

            context = new LoggerContext(dbConnString);
            List<LoggerFragment> fragmentList;
            //= new List<LoggerFragment>();
            //部署视频分类标识;
            int categoryId = categories.Count - 1;
            categories.Reverse();
            
            categories.ForEach(p => {
                //部署视频本体标识;
                int videoId = p.Videos.Count - 1;
                //反转结构以方便后继链表的形成; 
                p.Videos.Reverse();
                var loggerCategory = LoggerCategory.Create(p);
                loggerCategory.CategoryId = categoryId;
                try {
                    context.Categories.Add(loggerCategory);
                }
                catch (Exception ex) {
                    EventLogger.CaseLogger.WriteLine(ex.Message);
                    throw ex;
                }

                if (p.Videos == null) {
                    return;
                }

                p.Videos.ForEach(q => {
                    fragmentList = new List<LoggerFragment>();
                    //部署文件碎片标识;
                    q.FileFragments.Reverse();
                    int fragmenIndex = q.FileFragments.Count - 1;
                    
                    var loggerVideo = LoggerVideo.Create(q);
                    if(loggerVideo == null) {
                        return;
                    }
                    loggerVideo.VideoId = videoId;
                    loggerVideo.CategoryId = categoryId;
                    context.Videos.Add(loggerVideo);

                    q.FileFragments.ForEach(t => {
                        var loggerFragment = LoggerFragment.Create(t);
                        loggerFragment.CategoryId = categoryId;
                        loggerFragment.VideoId = videoId;
                        loggerFragment.FragmentIndex = fragmenIndex;
                        try {
                            fragmentList.Add(loggerFragment);
                        }
                        catch(Exception ex) {
                            EventLogger.CaseLogger.WriteLine("LoggerWriter->Writecategories->添加碎片节点错误:" + ex.Message);
                        }
                        
                        fragmenIndex--;
                    });
                    formatter.Serialize(stream, fragmentList);
                    context.SaveChanges();
                    videoId--;
                });
                
                categoryId--;
            });

            categories.Reverse();
            //formatter.Serialize(stream, entity);
            context.Dispose();
            
            stream.Close();
            
            return false;
        }
        
        /// <summary>
        /// 写入文件范围表;(对应需文件校验的类型)
        /// </summary>
        /// <param name="rangeList"></param>
        /// <returns></returns>
        public bool WriteRanges(ValueRangeList rangeList) {
            if(rangeList == null) {
                EventLogger.CaseLogger.WriteLine("LoggerWriter->WriteRange错误:参数不得为空!");
                return false;
            }
            try {
                using (var context = new LoggerContext(dbConnString)) {
                    context.Ranges.RemoveRange(context.Ranges);
                    rangeList.Ranges.ForEach(p => {
                        context.Ranges.Add(new LoggerRange {
                            IniValue = Convert.ToInt64(p.IniValue),
                            EndValue = Convert.ToInt64(p.EndValue)
                        });
                    });
                    context.SaveChanges();
                }
                
                return true;
            }
            catch(Exception ex) {
                EventLogger.CaseLogger.WriteLine("LoggerWriter->WriteRange未知错误:"+ex.Message);
                return false;
            }
        }
    }
   
}
