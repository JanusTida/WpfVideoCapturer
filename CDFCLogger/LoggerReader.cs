
using CDFCLogger.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using CDFCLogger.Models;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCValueRanges;
using CDFC.Util.PInvoke;

namespace CDFCLogger {
    /// <summary>
    /// 读取日志信息所需静态类;
    /// </summary>
    public class LoggerReader {
        /// <summary>
        /// 日志读取器的构造方法;
        /// </summary>
        /// <param name="loggerPath">日志所在路径</param>
        /// <param name="dbName">所设定日志的名字</param>
        /// <param name="iObjectDevice">关联的对象设备</param>
        public LoggerReader(string dbName,IObjectDevice iObjectDevice) {
            if (!File.Exists(dbName)) {
                EventLogger.CaseLogger.WriteLine("LoggerReader构造错误:案件文件不存在。");
            }
            else {
                FileInfo dbInfo = new FileInfo(dbName);
                loggerPath = (dbInfo.Directory.FullName+"/").Replace('\\','/');
                if (!File.Exists(fragPath)) {
                    EventLogger.CaseLogger.WriteLine("LoggerReader构造错误:文件碎片记录不存在。");
                    //throw new FileNotFoundException("文件碎片记录不存在");
                }
                else {
                    this.dbName = dbInfo.Name;
                    this.iObjectDevice = iObjectDevice;
                }
            }
        }

        //单个日志文件目录路径;
        private string loggerPath;
        //日志文件名;
        private string dbName;
        //日志对象;
        private IObjectDevice iObjectDevice;
        //碎片文件地址;
        private string fragPath {
            get {
                return loggerPath + "tick.bin";
            }
        }
        
        /// <summary>
        /// 获得所有记录，时间分类，碎片，文件;
        /// </summary>
        /// <param name="loggerPath">Sqlite文件所在路径</param>
        /// <param name="iObjectDevice">所在设备信息</param>
        /// <returns></returns>
        public IntPtr FileList {
            get { 
                //构建时间分类列表;
                List<DateCategory> categories = new List<DateCategory>();

                //使用所需的二进制序列器,以获得高性能表现;
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream;
                try { 
                    stream = new FileStream(fragPath,FileMode.Open);
                }
                catch(Exception ex) {
                    EventLogger.CaseLogger.WriteLine("LoggerReader->DateCategiries错误:" +ex.Message);
                    return IntPtr.Zero;
                }
                long streamLength = stream.Length;
                //获得数据库上下文;
                LoggerContext context = new LoggerContext(loggerPath+dbName);
            
                //使用分类指针以连接分类链表;
                IntPtr categoryPtrNode = IntPtr.Zero;

                var categoriesList = context.Categories.ToList();
                var videoList = context.Videos.ToList();
                int videoCount = context.Videos.Count();

//                List<DateCategoryStruct> categoryStructs = new List<DateCategoryStruct>();

                if (videoList == null || videoList.Count == 0 || categoriesList == null || categoriesList.Count == 0) {
                    EventLogger.CaseLogger.WriteLine("LoggerReader->FileList错误:列表为空");
                    return IntPtr.Zero;
                }

                
                int videoIndex = 0;

                try { 
                categoriesList.ForEach(p => {
                    LoggerVideo videoNode = videoList[videoIndex];
                    var categoryStructNode = p.CategoryStruct;
                    var videoPtrNode = IntPtr.Zero;
                    var videoStructNode = videoNode.VideoStruct;
                
                    while (videoNode.CategoryId == p.CategoryId) {
                        var fragmentList = (List<LoggerFragment>)formatter.Deserialize(stream);
                        var fragmentPtrNode = IntPtr.Zero;
                        
                        if (fragmentList.Count != 0) {
                            if (fragmentList[0].CategoryId == videoNode.CategoryId && fragmentList[0].VideoId == videoNode.VideoId) {
                                fragmentList.ForEach(q => {
                                    var fragmentNode = q;
                                    var fragmentStructNode = fragmentNode.FragmentStruct;
                                    fragmentStructNode.Next = fragmentPtrNode;

                                    //封送碎片节点结构，并得到该节点指针,以便与上一个节点连结;
                                    fragmentPtrNode = fragmentStructNode.GetPtrFromStructure();
                                    //将该碎片节点指针加入非托管内存指针队列;
                                    UnManagedPtrs.Add(fragmentPtrNode);
                                });
                            }
                            else {
                                break;
                            }
                        }
                        
                        videoStructNode.stStAdd = fragmentPtrNode;
                        videoStructNode.Next = videoPtrNode;
                        
                        //封送文件节点结构，并得到该节点指针，以便与上一个节点连结;
                        videoPtrNode = videoStructNode.GetPtrFromStructure();
                        //将该文件节点指针加入非托管内存指针队列;
                        UnManagedPtrs.Add(videoPtrNode);
                        
                        videoIndex++;
                        if(videoIndex >= videoCount) {
                            break;
                        }
                        videoNode = videoList[videoIndex];
                        videoStructNode = videoNode.VideoStruct;
                    }

                    categoryStructNode.Next = categoryPtrNode;
                    categoryStructNode.File = videoPtrNode;

                    //封送文件分类节点结构，并得到该节点指针，以便与上一个节点连结;
                    categoryPtrNode = categoryStructNode.GetPtrFromStructure();
                    //将文件分类节点指针加入非托管内存指针队列;

                    UnManagedPtrs.Add(categoryPtrNode);
                    //categoryStructs.Add(categoryStructNode);
                });
                }
                catch(Exception ex) {
                    EventLogger.CaseLogger.WriteLine("文件链表还原错误:"+ex.Message);
                }
                #region 构建文件分类列表(原版本)
                //categoryStructs.ForEach(p => {
                //    var category = iObjectDevice.CreateDatecategory(p,(ScanMethod)loggerSetting.ScanMethod);
                //    category.DeviceTypeEnum = (DeviceTypeEnum)LoggerSetting.DeviceTypeEnum;
                //    categories.Add(category);
                //});

                //析构数据库上下文;
                #endregion

                context.Dispose();
                stream.Close();
                return categoryPtrNode;
            }
        }

        /// <summary>
        /// 获得文件系统的文件范围:(对应需校验类型)
        /// </summary>
        public ValueRangeList RangeList {
            get {
                try {
                    using(var context = new LoggerContext(loggerPath + dbName)) {
                        var ranges = context.Ranges.ToList();
                        ValueRangeList rangeList = new ValueRangeList();
                        ranges.ForEach(p => {
                            rangeList.Concatenate(p.IniValue,p.EndValue);
                        });
                        return rangeList;
                    }
                }
                catch(Exception ex) {
                    EventLogger.CaseLogger.WriteLine("LoggerReader->RangeList错误:" + ex.Message);
                    return null;
                }
            }
        }

        //非托管内存指针队列;以便于释放;
        public List<IntPtr> UnManagedPtrs { get;private set; } = new List<IntPtr>();
        private LoggerSetting loggerSetting;
        /// <summary>
        /// 获得日志中的设定;
        /// </summary>
        public LoggerSetting LoggerSetting{
            get { 
                if(loggerSetting == null) {
                    try { 
                        LoggerContext context = new LoggerContext(loggerPath+dbName);
                        loggerSetting = context.Setting.FirstOrDefault();
                        context.Dispose();
                    }
                    catch(Exception ex) {
                        EventLogger.CaseLogger.WriteLine("LoggerReader->LoggerSetting错误:"+ex.Message);
                        return null;
                    }
                }
                return loggerSetting;
            }
        }

        
          /// <summary>
        /// 当前读取的进度;
        /// </summary>
        public byte CurPercentage { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public void FreeTask() {

        }
    }
}
