using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using CDFCEntities.Files;
using System.Windows;
using CDFCVideoExactor.Commands;
using System.Threading;
using CDFCMessageBoxes.MessageBoxes;
using CDFCUIContracts.Commands;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 帧预览器实体模型;
    /// </summary>
    public partial class FramesPreviewerViewModel : ViewModelBase,IDisposable {
        //默认的图片路径;
        private const string defaultImgPath = "/CDFCVideoExactor;component/Images/FramesPreviewer/DefaultIcon.jpg";
        //截图生成程序的位置;
        private const string pictureMakerPath = @"cdfcplayer2.exe";
        //截图生成唯一ID;
        private int ImgID = 0;
        /// <summary>
        /// 已经预览的文件项;
        /// </summary>
        public List<PreviewBook> PastedBooks { get; private set; } = new List<PreviewBook>();

        /// <summary>
        /// 当前正在预览的文件项;
        /// </summary>
        public PreviewBook CurPreviewBook { get; private set; }

        //当前正在显示的图像;
        public ObservableCollection<PreviewImg> PreviewImgs { get; private set; } = new ObservableCollection<PreviewImg>();

        public FramesPreviewerViewModel(MainWindowViewModel mainWindowViewModel) {
            if(mainWindowViewModel == null) {
                throw new ArgumentNullException(nameof(mainWindowViewModel));
            }
            this.mainWindowViewModel = mainWindowViewModel;
            PreviewImgs = new ObservableCollection<PreviewImg>();
            
            //加入十张默认图片;
            int index = 0;
            while (index++ < 10) {
                PreviewImgs.Add(new PreviewImg (defaultImgPath));
            }
        }

        private MainWindowViewModel mainWindowViewModel;

        /// <summary>
        /// 图像预览器的释放;
        /// </summary>
        public void Dispose() {
            //清除已经预览的项;
            PastedBooks.Clear();
            //清除正在显示的图像;
            PreviewImgs.Clear();
            disposed = true;
            CurPreviewBook = null;
            ImgID = 0;
            Page = 1;
            //加入十张默认图片;
            int index = 0;
            while (index++ < 10) {
                PreviewImgs.Add(new PreviewImg(defaultImgPath));
            }

        }

        /// <summary>
        /// 跳转至指定行;线程池方法;
        /// </summary>
        /// <param name="row">目标行</param>
        public void EscapeToRow(VideoRow row) {
            //MP4暂时不进行预览;
            //if (CDFCSetting.ScanSetting.VersionType.IsMp4Class) {
            //    return;
            //}
            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                ClearPath();
            });
            //寻找是否已经预览过该行;
            var targetBook = PastedBooks.FirstOrDefault(p => p.Video == row.Video);

            //若未预览当前行，则生成该文件，图像;
            //并加载在当前图像上;
            if(targetBook == null) {
                string curPath = AppDomain.CurrentDomain.BaseDirectory;

                //获取目标路径目录
                string targetPath = curPath + "temp";
                Random rand = new Random();

                //生成文件相对路径;
                StringBuilder sbFileRelativePath = new StringBuilder(120);
                sbFileRelativePath.Append(@"\");
                //随机生成一个文件名;
                for (int randBit = 0; randBit < 100; randBit++) {
                    sbFileRelativePath.Append(rand.Next(9));
                }
                
                //将此预览书加入已预览队列;
                targetBook = new PreviewBook(row.Video,targetPath+sbFileRelativePath.ToString());
                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                    Page = 1;
                });
                CurPreviewBook = targetBook;
                PastedBooks.Add(targetBook);

                # region 生成可预览的视频文件;
                try {
                    BuildPreviewFile(row.Video, targetPath, sbFileRelativePath.ToString());
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("FramesPreviewerViewModel->EscapeToRow错误:" + ex.Message);
                    CDFCMessageBox.Show("恢复文件错误!");
                    return;
                }
                #endregion

                #region 生成预览图像;
                try {
                    BuildPreviewImg(targetPath+sbFileRelativePath.ToString(),targetBook);
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("预览图生成错误:"+ex.Message);                
                }
                #endregion
            }
            //否则将预览文件跳转至未完成/已完成的文件;
            else {
                CurPreviewBook = targetBook;
                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                    if(CurPreviewBook.CurPage == null) {
                        return;
                    }
                    foreach (var img in CurPreviewBook.CurPage.PreviewImgs) {
                        if(CurPreviewBook == targetBook) {
                            PreviewImgs.Add(img);
                        }
                    }
                    try { 
                        Page = Convert.ToUInt32(targetBook.CurPage.Page + 1);
                    }
                    catch(Exception ex) {
                        EventLogger.Logger.WriteLine("FramesPreviewerViewModel->跳转至指定行错误:" + ex.Message);
                    }
                });
            }

            NotifyPropertyChanging(nameof(BackPageCanExecute));
        }

        /// <summary>
        /// 跳转至指定页;线程池方法;
        /// </summary>
        /// <param name="page"></param>
        public void EscapeToPage(short page) {
            var curPreviewBook = CurPreviewBook;
            if(curPreviewBook == null) {
                return;
            }

            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                PreviewImgs.Clear();
            });

            var previewPage = curPreviewBook.PastedPages.FirstOrDefault(p => p.Page == page);

            Page = Convert.ToUInt32(page + 1);

            //若未经过该页，则生成该页;
            if (previewPage == null) {
                try { 
                    BuildPreviewImg(curPreviewBook.PreviewFilePath,curPreviewBook, page);
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("预览图生成错误:" + ex.Message);
                }
            }
            //否则，可直接跳转至该页;
            else {
                curPreviewBook.CurPage = previewPage;
                foreach(var img in curPreviewBook.CurPage.PreviewImgs) {
                    mainWindowViewModel.UpdateInvoker.Invoke(() => {
                        PreviewImgs.Add(img);
                    });
                }
            }
        }

        /// <summary>
        /// 当前页（需加1）;
        /// </summary>
        private uint page;
        public uint Page {
            get {
                return page;
            }
            set {
                page = value;
                NotifyPropertyChanging(nameof(Page));
            }
        }
        
        /// <summary>
        /// 清空所有图片路径;UI线程方法;
        /// </summary>
        public void ClearPath() {
            PreviewImgs.Clear();
        }

        ////替换所有预览图像;
        //public void UpdatePaths(List<string> imgPaths) {
        //    lock (PreviewImgs) {
        //        imgPaths.ForEach(p => {
        //            PreviewImgs.Add(new PreviewImg { ImagePath = p });
        //        });
        //    }
        //}

        ///// <summary>
        ///// 加入新的图片;UI线程方法;
        ///// </summary>
        ///// <param name="curPriror">当前的优先级别</param>
        //public void AddPath(string path) {
        //    lock (PreviewImgs) {
        //        PreviewImgs.Add(new PreviewImg { ImagePath = path });
        //    }
        //}

        private bool disposed = false;
    }

    /// <summary>
    /// 生成预览部分;
    /// </summary>
    public partial class FramesPreviewerViewModel {
        private object buildingLocker = new object();
        /// <summary>
        /// 生成预览图像的方法;线程池方法;
        /// </summary>
        /// <param name="targetFile">源文件</param>
        /// <param name="page">目标预览书</param>
        /// <param name="row">目标文件行</param>
        private void BuildPreviewImg(string targetFile,PreviewBook previewingBook, short page = 0) {
            //lock (buildingLocker) {
            //记录下当前正在工作的预览书;
            var previewingPage = previewingBook.CreatePage(page);
            previewingBook.CurPage = previewingPage;
            previewingBook.PastedPages.Add(previewingPage);

            string curPath = AppDomain.CurrentDomain.BaseDirectory;

            //路径前后加双引号，防止空白路径无效;
            ProcessStartInfo startInfo = new ProcessStartInfo("\"" + curPath + pictureMakerPath + "\"");
            StringBuilder sbArguments = new StringBuilder();

            //若目标(图片)输出文件夹不存在;
            if (!Directory.Exists(string.Format("{0}previewFrames", curPath))) {
                Directory.CreateDirectory(string.Format("{0}previewFrames", curPath));
                File.SetAttributes(string.Format("{0}previewFrames", curPath), FileAttributes.Hidden);
            }

            //ViewList.Dispatcher.Invoke(() => {
            //    MainWindowViewModel.FramesPreviewerViewModel.ClearPath();
            //});

            for (int index = 0; index < 10; index++) {
                #region 文件生成过程;
                //添加目标视频文件;
                sbArguments.Append(" -i ");
                sbArguments.Append("\"" + targetFile + "\"");

                //设定截取时间区间;
                sbArguments.AppendFormat(" -y -ss {0} -t {1} -s ",
                    TimeSpan.FromSeconds(page * 10 + index),
                    TimeSpan.FromSeconds(page * 10 + index + 1));

                //设定生成图像尺寸;
                sbArguments.Append(" 320*320 ");

                //设定其他参数;
                sbArguments.Append(" -f mjpeg -vframes 10 ");

                //生成帧文件全名,双引号防止路径出现空格，导致不能正常执行第三方程序;
                string imagePath = string.Format("{0}previewFrames/{1}-{2}-{3}-{4}-{5}-{6}.jpg",
                    curPath, CurPreviewBook.Video.DateCategory.DeviceTypeEnum, CurPreviewBook.Video.ChannelNO,
                    CurPreviewBook.Video.StartDate, CurPreviewBook.Video.EndDate, page * 10 + index, ImgID++);
                string outputFileName = "\"" + imagePath + "\"";
                //图像文件是否存在;
                bool imgExists = true;

                try {
                    imgExists = File.Exists(outputFileName);
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("FramesPreviewerViewModel->BuildPreviewImg判断文件存在错误:"+ex.Message);
                }
                if (imgExists) {
                    File.Delete(outputFileName);
                }
                if (disposed) {
                    break;
                }
                sbArguments.Append(outputFileName);
                startInfo.Arguments = sbArguments.ToString();
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                var pro = Process.Start(startInfo);
                pro.EnableRaisingEvents = true;

                #endregion
                
                //等待截图程序退出，防止过多进程占用cpu。
                pro.WaitForExit();
                
                if (File.Exists(imagePath)) {
                    try {
                        if (PreviewImgs == null) {
                            EventLogger.Logger.WriteLine("FramesPreviewerViewModel->BuildPreviewImg进程释放错误:预览组为空.");
                            return;
                        }
                        var previewImg = new PreviewImg(imagePath);
                        //若仍选择当前行,且页码一致,则加入视图;
                        //mainWindowViewModel.VideoItemListViewerPageViewModel.SelectedRow != null &&
                        //mainWindowViewModel.VideoItemListViewerPageViewModel.SelectedRow.Video ==
                        if (previewingBook == CurPreviewBook
                        && CurPreviewBook.CurPage.Page == page) {
                            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                                PreviewImgs.Add(previewImg);
                            });
                        }
                        if (previewingPage.PreviewImgs == null) {
                            EventLogger.Logger.WriteLine("FramesPreviewerViewModel->BuildPreviewImg进程释放错误:目标页预览组为空.");
                            return;
                        }
                        else {
                            lock (previewingPage.PreviewImgs) {
                                previewingPage.PreviewImgs.Add(previewImg);
                            }
                        }
                    }
                    catch (Exception ex) {
                        EventLogger.Logger.WriteLine("FramesPreviewerViewModel->BuildPreviewImg进程释放未知错误:" + ex.Message);
                        return;
                    }

                }
                //若生成文件失败,则添加默认图像;
                else {
                    try {
                        var defaultImg = new PreviewImg(defaultImgPath);
                        lock (previewingPage.PreviewImgs) {
                            previewingPage.PreviewImgs.Add(defaultImg);
                        }
                        mainWindowViewModel.UpdateInvoker.Invoke(() => {
                            PreviewImgs.Add(defaultImg);
                        });
                    }
                    catch {
                        EventLogger.Logger.WriteLine("Not Sure the confirm is wrong!");
                        throw;
                    }
                }
                //pro.Exited += (sender, e) => {
                //    //若文件存在，则生成文件成功;

                //};
                sbArguments.Clear();
                //若本模型已经退出，则释放掉;
                if (disposed) {
                    break;
                }
            }
                //string s = pro.StandardOutput.ReadToEnd();
                //return outputFileName;
            
        }
        
        /// <summary>
        /// 生成预览文件;线程池方法;
        /// </summary>
        /// <param name="targetPath">目标路径</param>
        /// <param name="targetFileName">目标文件</param>
        /// <param name="page">页码</param>
        private void BuildPreviewFile(Video video,string targetPath, string targetFileName) {
            //若目标路径目录不存在,生成该文件夹;
            if (!Directory.Exists(targetPath)) {
                var directory = Directory.CreateDirectory(targetPath);
                File.SetAttributes(targetPath, FileAttributes.Hidden);
            }

            try {
                //设置预览大小;
                mainWindowViewModel.Scanner.DefaultRecoverer.SetPreview(2097152);
                mainWindowViewModel.Scanner.DefaultRecoverer.Init(video);
                mainWindowViewModel.Scanner.DefaultRecoverer.SaveAs(targetPath + targetFileName);
            }
            catch (AccessViolationException ex) {
                EventLogger.Logger.WriteLine("文件恢复错误!" + ex.Message + ex.Source);
            }
            catch (Exception ex) {
                EventLogger.Logger.WriteLine("预览:未知文件恢复错误!" + ex.Message + ex.Source);
                //MessageBox.GetOne("文件预览错误!");
            }
        }
    }

    /// <summary>
    /// 图片预览模型的命令绑定项;
    /// </summary>
    public partial class FramesPreviewerViewModel {
        /// <summary>
        /// 确认跳转的命令;
        /// </summary>
        private RelayCommand confirmCommand;
        public RelayCommand ConfirmCommand {
            get {
                return confirmCommand ??
                    (confirmCommand = new RelayCommand(() => {
                        ThreadPool.QueueUserWorkItem(callBack => {
                            short page;
                            if (short.TryParse((this.page / 10).ToString(), out page)) {
                                EscapeToPage(page);
                            }
                            NotifyPropertyChanging(nameof(BackPageCanExecute));
                        });
                    }));
            }
        }

        /// <summary>
        /// 跳转至上一页的命令;
        /// </summary>
        private RelayCommand backPageCommand;
        public RelayCommand BackPageCommand {
            get {
                return backPageCommand ??
                    (backPageCommand = new RelayCommand(
                        () => {
                            ThreadPool.QueueUserWorkItem(callBack => {
                                if (page > 1) {
                                    try {
                                        EscapeToPage(Convert.ToInt16(page - 2));
                                    }
                                    catch (Exception ex) {
                                        EventLogger.CaseLogger.WriteLine("FramesPreviewerViewModel->跳转至上一页错误:" + ex.Message);
                                    }
                                }
                            });
                        }
                        ,() => {
                            return BackPageCanExecute;
                        }
                    ));
            }
        }
        public bool BackPageCanExecute {
            get {
                return page > 1;
            }
        }

        /// <summary>
        /// 跳转至下一页的命令;
        /// </summary>
        private RelayCommand stepPageCommand;
        public RelayCommand StepPageCommand {
            get {
                return stepPageCommand ??
                    (stepPageCommand = new RelayCommand(
                        () => {
                            ThreadPool.QueueUserWorkItem(callBack => {
                                try { 
                                    EscapeToPage(Convert.ToInt16( page ));
                                    NotifyPropertyChanging(nameof(BackPageCanExecute));
                                }
                                catch (Exception ex){
                                    EventLogger.Logger.WriteLine("FramesPreviewerViewModel->跳转至下一页错误:" + ex.Message);
                                }
                            }
                        );


                        }));
            }
        }
    }
}
