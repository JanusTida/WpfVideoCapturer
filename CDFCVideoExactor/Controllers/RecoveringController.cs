using CDFCConverters.Converters;
using CDFCEntities.Enums;
using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.ViewModels;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using EventLogger;
namespace CDFCVideoExactor.Controllers {
    /// <summary>
    /// 文件恢复的控制器;
    /// </summary>
    public partial class RecoveringController:IRecoveringController {
        /// <summary>
        /// 文件恢复控制器的构造方法;
        /// </summary>
        /// <param name="iobjectDevice">选择的对象</param>
        /// <param name="videos">恢复的文件项</param>
        public RecoveringController(MainWindowViewModel mainWindowViewModel, List<Video> videos) {
            if(mainWindowViewModel == null || videos == null) {
                EventLogger.Logger.WriteLine("RecoveringController初始化错误:参数为空");
                throw new NullReferenceException("IobjectDevice or videos can't be null!");
            }
            
            this.scanner = mainWindowViewModel.Scanner;
            this.videos = videos;
            this.mainWindowViewModel = mainWindowViewModel;

            totalSize = videos.Sum(p => Convert.ToInt64(p.Size));
            recoveringInfoWindowViewModel = new RecoveringInfoWindowViewModel(this);
        }

        //总大小;
        private long totalSize;
        //已恢复大小;
        private long recoveredSize;
        //已恢复文件数量;
        private int recoveredFileCount;
        
        
        private List<Video> videos;
        private RecoveringInfoWindowViewModel recoveringInfoWindowViewModel;
        private IScanner scanner;
        private MainWindowViewModel mainWindowViewModel;
        //恢复的目标路径;
        public string RecoveringPath {
            get;private set;
        }

        //是否正在恢复状态中;
        private bool isRecovering = true;
        public bool IsRecovering {
            get {
                return isRecovering;
            }
            set {
                isRecovering = value;
                recoveringInfoWindowViewModel.IsRecovering = value;
            }
        }

        //当前使用的恢复器;
        IRecoverer recoverer;
    }


    public partial class RecoveringController {
        //开始文件恢复;
        public void Start() {
            //询问恢复位置;
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            
            dialog.Description = "请选择一个位置";
            dialog.ShowNewFolderButton = true;

            Logger.WriteCallerLine("Start Choosing Places...");
            //若未确定,则退出;
            if (dialog.ShowDialog() == false) {
                Logger.WriteCallerLine("ShowDialog returned false.");
                return;
            }
            Logger.WriteCallerLine("ShowDialog returned not false.");

            RecoveringPath = dialog.SelectedPath;
            try { 
                scanner.DefaultRecoverer.SetPreview(0);
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("RecoveringController->Start获得恢复器错误:" + ex.Message);
                return;
            }
            //部署后台恢复工作器;
            BackgroundWorker worker = new BackgroundWorker();
            
            worker.DoWork += (sender, e) => {
                StringBuilder sbFile = new StringBuilder();

                //获得绝对路径;
                string deviceTypeString = CDFCSetting.ScanSetting.DeviceTypeInfo.Info;
                string versionTypeString = CDFCSetting.ScanSetting.VersionType.Info;

                //获得文件的绝对存储路径(不包含文件名)
                sbFile.AppendFormat("{0}\\{1}\\{2}\\{3}\\",RecoveringPath,
                    DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day, 
                    deviceTypeString, versionTypeString);
                //获得文件的绝对存储路径长度，以多次重置;
                int relativeLength = sbFile.Length;
                recoverer = scanner.DefaultRecoverer;
                
                videos.ForEach(p => {
                    int count = sbFile.Length - relativeLength;
                    sbFile.Remove(relativeLength, count);
                    if(mainWindowViewModel.Scanner.DeviceType.GetDeviceCategory() == DeviceCategory.Capturer) {
                        if(mainWindowViewModel.Scanner.DeviceType == DeviceTypeEnum.JVS) {
                            sbFile.Append($"\\通道-未知\\");
                        }
                        else {
                            sbFile.Append($"\\通道{p.ChannelNO}\\");
                        }
                        
                    }
                    
                    if (!Directory.Exists(sbFile.ToString())) {
                        Directory.CreateDirectory(sbFile.ToString());
                    }
                    sbFile.Append(DateNumToDateStringConverter.ConvertToStorageDateString(p.StartDate) + "." + CDFCSetting.ScanSetting.ExtensionName);
                    recoveringVideo = p;
                    try {
                        recoverer.Init(p);
                        var res = recoverer.SaveAs(sbFile.ToString());
                        if (res) {
                            recoveredSize += p.Size;
                        }
                        recoveredFileCount += 1;
                    }
                    catch(AccessViolationException ex) {
                        EventLogger.Logger.WriteLine("RecoveringController->Start->Do_Work出错:"+ex.Message+ex.Source);
                    }
                });

                lock (recoverer) {
                    recoverer = null;
                }
            };

            worker.RunWorkerCompleted += (sender, e) => {
                IsRecovering = false;
            };


            recoveringInfoWindowViewModel = new RecoveringInfoWindowViewModel(this);
            recoveringInfoWindowViewModel.TotalFileCount = videos.Count;
            recoveringInfoWindowViewModel.TotalFileSize = totalSize;
            RecoveringInfoWindow window = new RecoveringInfoWindow(recoveringInfoWindowViewModel);
            worker.RunWorkerAsync();
            ThreadPool.QueueUserWorkItem(callBack => {
                UpdateAction();
            });
            window.ShowDialog();
        }

        //执行更新视图控制线程;
        private void UpdateAction() {
            while (true) {
                if (isRecovering) {
                    UpdateView();
                }
                else {
                    UpdateView();
                    recoveringInfoWindowViewModel.Percentage = 100;
                    break;
                }
                Thread.Sleep(100);
            }
        }

        //执行一次视图更新的方法;
        private void UpdateView() {
            long curProgressSize = 0;
            //获取当前正在恢复的对象进度;
            try {
                curProgressSize = recoverer?.CurProgressSize ?? 0;
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("RecoveringController->UpdateView->curProgressSize错误:" + ex.Message);
            }
            recoveringInfoWindowViewModel.RecoveredFileCount = recoveredFileCount;
            recoveringInfoWindowViewModel.RecoveredFileSize = recoveredSize + curProgressSize;
            
            var percentage = (recoveredSize + curProgressSize) * 100 / totalSize;
            recoveringInfoWindowViewModel.Percentage = Convert.ToByte(percentage > 100 ? 100 : percentage);
        }

        private Video recoveringVideo;
    }
}
