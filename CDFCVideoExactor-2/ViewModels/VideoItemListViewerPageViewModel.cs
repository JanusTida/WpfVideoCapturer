using CDFCEntities.Files;
using CDFCEntities.Interfaces;
using CDFCMessageBoxes.MessageBoxes;
using CDFCPlayer;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Controllers;
using CDFCVideoExactor.Interfaces;
using CDFCVideoExactor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// 文件列表视图;
    /// </summary>
    public partial class VideoItemListViewerPageViewModel : ViewModelBase {
        public VideoItemListViewerPageViewModel(MainWindowViewModel mainWindowViewModel) : base(5) {
            if (mainWindowViewModel == null) {
                EventLogger.Logger.WriteLine("VideoItemListViewerPageViewModel初始化出错:参数mainWindowViewModel不得为空");
                MessageBox.Show("抱歉，文件列表视图初始化错误!");
                return;
            }

            this.mainWindowViewModel = mainWindowViewModel;
            //InitEmptyRows();
        }

        public List<VideoRow> EmptyRows { get; private set; } = new List<VideoRow>();
        private MainWindowViewModel mainWindowViewModel;

        /// <summary>
        /// 初始化空行;防止行少导致无内容;
        /// </summary>
        private void InitEmptyRows() {
            EmptyRows.Clear();
            for (int index = 0; index < 40; index++) {
                var emptyRow = VideoRow.Empty;
                CurRows.Add(emptyRow);
                EmptyRows.Add(emptyRow);
            }
        }

        //清除可见行;
        public void ClearRows() {
            try {
                //清空可见行;
                CurRows.Clear();
                //初始化空行;
                //InitEmptyRows();
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("videoItemListViewerPageViewModel->ClearRows错误:"+ex.Message);
            }
        }



        /// <summary>
        /// 添加新的有效行;
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(VideoRow row, int itemID = -1) {
            if (itemID == -1) {
                CurRows.Insert(ItemID++, row);
            }
            else {
                CurRows.Insert(itemID, row);
            }
        }

        //当前的对象;
        private IObjectDevice iObjectDevice;

        //获得当前的对象;
        public void GetIObjectDevice(IObjectDevice iObjectDevice) {
            this.iObjectDevice = iObjectDevice;
            ObjectHexViewerViewModel.GetIObjectDevice(iObjectDevice);
        }
        /// <summary>
        /// 添加新的有效行集合;
        /// </summary>
        /// <param name="newRows"></param>
        public void AddRangeRows(List<VideoRow> newRows) {
            newRows.ForEach(p => {
                AddRow(p);
            });
            ActualRows.AddRange(newRows);
        }

        private int ItemID;

        /// <summary>
        /// 当前可见的行;
        /// </summary>
        public ObservableCollection<VideoRow> CurRows { get; private set; } = new ObservableCollection<VideoRow>();

        /// <summary>
        /// 所有的行;
        /// </summary>
        public List<VideoRow> ActualRows { get; private set; } = new List<VideoRow>();

        /// <summary>
        /// 释放接口；
        /// </summary>
        public void Exit() {
            try {
                ClearRows();
                //清空经过行;
                pastedRows.Clear();
                //清空所有行;
                ActualRows.Clear();

                //帧预览器退出;
                if(FramesPreviewerViewModel != null) {
                    FramesPreviewerViewModel.Dispose();
                    FramesPreviewerViewModel = null;
                }
                
                //十六进制查看器释放;
                objectHexViewerViewModel.Exit();

                //施放任务对象;
                mainWindowViewModel.Scanner.Dispose();
                mainWindowViewModel.Scanner = null;
                
                //任务对象置空;
                iObjectDevice = null;
                //初始化排序序列号;
                ItemID = 0;
                //初始化正在显示的行;
                showingRow = null;
                //初始化通道号排序状态;
                ChannelSortDirection = null;
                //取消所有行的选中状态;
                AllSelected = false;

            }
            catch(NullReferenceException ex) {
                EventLogger.Logger.WriteLine("文件列表查看器释放出错:空引用-" + ex.Source);
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("文件列表查看器释放出错:" + ex.Message);
            }
            //删除临时文件;
            ThreadPool.QueueUserWorkItem(callBack => {
                PlayerMethods.DisposeTemp();
                PlayerMethods.DisposeFrames();
            });
            #region 碎片图表的释放;
            if (fragmentAnalyzerWindowViewModel != null) {
                fragmentAnalyzerWindowViewModel.Dispose();
                fragmentAnalyzerWindowViewModel = null;
            }
            #endregion
        }
    }

    /// <summary>
    /// 文件列表项目的状态;
    /// </summary>
    public partial class VideoItemListViewerPageViewModel {
        //截图生成程序的位置;
        private const string pictureMakerPath = @"cdfcplayer2.exe";

        /// <summary>
        /// 预览帧的唯一ID(限本次启动);
        /// </summary>
        private int previewID;
        /// <summary>
        /// 选中的行;
        /// </summary>
        private VideoRow selectedRow;
        public VideoRow SelectedRow {
            get {
                return selectedRow;
            }
            set {
                selectedRow = value;
                var curRow = value;

                if (value == null || value.IsEmpty) {
                    return;
                }
                //FramesPreviewerViewModel.ClearPath();
                ThreadPool.QueueUserWorkItem(callBack => {
                    FramesPreviewerViewModel.EscapeToRow(curRow);
                });

                /*
                ////若已点击过该行;
                //if (pastedRows.Contains(curRow)) {
                //    lock (curRow.PreviewedImgs) {
                //        FramesPreviewerViewModel.UpdatePaths(curRow.PreviewedImgs);
                //    }
                //}
                ////否则将自行生成预览帧;
                //else {
                //    pastedRows.Add(curRow);
                //    //线程池调用防止界面响应迟钝,生成预览文件;
                //    ThreadPool.QueueUserWorkItem(callback => {
                //        string curPath = AppDomain.CurrentDomain.BaseDirectory;
                //        //获取目标路径目录
                //        string targetPath = curPath + "temp";
                //        Random rand = new Random();
                //        //生成文件相对路径;
                //        StringBuilder sbFileRelativePath = new StringBuilder(120);
                //        sbFileRelativePath.Append(@"\");//添加路径分隔符;

                //        //随机生成一个文件名;
                //        for (int randBit = 0; randBit < 100; randBit++) {
                //            sbFileRelativePath.Append(rand.Next(9));
                //        }
                //        sbFileRelativePath.Append(CDFCSetting.ScanSetting.ExtensionName);

                //        //生成可预览的视频文件;
                //        try {
                //            BuildPreviewFile(value.Video, targetPath, sbFileRelativePath.ToString());
                //        }
                //        catch {
                //            MessageBox.GetOne("恢复文件出错!");
                //            return;
                //        }

                //        //生成预览图像;
                //        BuildPreviewImg(targetPath, sbFileRelativePath.ToString(), curRow);
                //        curRow.PreviewedPath = targetPath + sbFileRelativePath.ToString();
                //    });
                //}
                */
                //设定Hex显示的区域;
                ObjectHexViewerViewModel.SetSectorRange(
                    selectedRow.Video.StartAddress, selectedRow.Video.Size);
                
                NotifyPropertyChanging(nameof(SelectedRow));
            }
        }

        //已点击过的行;
        private List<VideoRow> pastedRows = new List<VideoRow>();

        //控制所有行的Check状态;
        private bool? allSelected = false;
        public bool? AllSelected {
            get {
                return allSelected;
            }
            set {
                //是否是全选;
                if (value != null) {
                    ActualRows.ForEach(p => {
                        if (!p.IsEmpty) {
                            p.IsChecked = (bool)value;
                        }
                    });
                    allSelected = value;
                }
                else {
                    ActualRows.ForEach(p => {
                        if (!p.IsEmpty) {
                            p.IsChecked = false;
                        }
                    });

                    allSelected = !allSelected;
                }
                NotifyPropertyChanging(nameof(AllSelected));
            }
        }

        /// <summary>
        /// 通道号的排序逻辑;
        /// </summary>
        private ListSortDirection?channelSortDirection = null;
        public ListSortDirection? ChannelSortDirection {
            get {
                return channelSortDirection;
            }
            set {
                channelSortDirection = value;
                NotifyPropertyChanging(nameof(ChannelSortDirection));
            }
        }

        /// <summary>
        /// 通道号列是否可见;(当选择监控类型时);
        /// </summary>
        public bool IsChannelColVisible {
            get {
                if(mainWindowViewModel == null) {
                    EventLogger.Logger.WriteLine("VideoItemListViewerPage->IsChannelColVisible错误:参数为空!");
                    return true;
                }
                return mainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer;
            }
        }

        public bool IsStateColVisible {
            get {
                if (mainWindowViewModel == null) {
                    EventLogger.Logger.WriteLine("VideoItemListViewerPage->IsChannelColVisible错误:参数为空!");
                    return true;
                }
                return (CDFCSetting.ScanSetting.DeviceType == CDFCEntities.Enums.DeviceTypeEnum.DaHua || CDFCSetting.ScanSetting.DeviceType == CDFCEntities.Enums.DeviceTypeEnum.WFS)
                    && CDFCSetting.ScanSetting.ScanMethod == CDFCEntities.Enums.ScanMethod.EntireDisk;
            }
        }
    }


    /// <summary>
    /// 处理文件预览帧的部分;
    /// </summary>
    public partial class VideoItemListViewerPageViewModel {
        /// <summary>
        /// 生成预览图像的方法;线程池方法;
        /// </summary>
        /// <param name="targetPath">输出路径</param>
        /// <param name="targetFile">源文件</param>
        /// <param name="row">目标文件行</param>
        private void BuildPreviewImg(string targetPath, string targetFile, VideoRow row) {
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
                //添加目标视频文件;
                sbArguments.Append(" -i ");
                sbArguments.Append("\"" + targetPath + targetFile + "\"");

                //设定截取时间区间;
                sbArguments.Append($" -y -ss 00:00:0{index} -t 00:00:{index+2} -s ");

                //设定生成图像尺寸;
                sbArguments.Append(" 320*320 ");

                //设定其他参数;
                sbArguments.Append(" -f mjpeg -vframes 10 ");

                //生成帧文件全名,双引号防止路径出现空格，导致不能正常执行第三方程序;
                string outputFileName = string.Format("\"{0}previewFrames/previewFrame{1}.jpg\"", curPath, previewID);

                string imgPath = string.Format("{0}previewFrames/previewFrame{1}.jpg", curPath, previewID++);

                if (File.Exists(outputFileName)) {
                    File.Delete(outputFileName);
                }
                sbArguments.Append(outputFileName);
                startInfo.Arguments = sbArguments.ToString();
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                var pro = Process.Start(startInfo);
                pro.EnableRaisingEvents = true;
                pro.Exited += (sender, e) => {
                    //若文件存在，则生成文件成功;
                    if (File.Exists(imgPath)) {
                        //若为当前行;
                        if (row == selectedRow) {
                            mainWindowViewModel.UpdateInvoker.Invoke(() => {
                               //FramesPreviewerViewModel.AddPath(imgPath);
                            });
                        }
                        lock (row.PreviewedImgs) {
                            row.PreviewedImgs.Add(imgPath);
                        }
                    }
                };
                sbArguments.Clear();
            }
            //string s = pro.StandardOutput.ReadToEnd();
            //return outputFileName;
        }

        /// <summary>
        /// 生成预览文件;线程池方法;
        /// </summary>
        /// <param name="targetPath"></param>
        /// <param name="targetFileName"></param>
        private void BuildPreviewFile(Video video, string targetPath, string targetFileName) {
            //若目标路径目录不存在,生成该文件夹;
            if (!Directory.Exists(targetPath)) {
                var directory = Directory.CreateDirectory(targetPath);
                File.SetAttributes(targetPath, FileAttributes.Hidden);
            }

            try {
                //设置预览大小;
                mainWindowViewModel.Scanner.DefaultRecoverer.SetPreview(2097152);
                mainWindowViewModel.Scanner.DefaultRecoverer.Init(video);
                mainWindowViewModel.Scanner.DefaultRecoverer.SaveAs(targetPath+targetFileName);
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
    /// 视频文件列表视图模型的分部模型;
    /// </summary>
    public partial class VideoItemListViewerPageViewModel {
        //十六进制查看器模型;
        private ObjectHexViewerViewModel objectHexViewerViewModel;
        public ObjectHexViewerViewModel ObjectHexViewerViewModel {
            get {
                return objectHexViewerViewModel ??
                    (objectHexViewerViewModel = new ObjectHexViewerViewModel());
            }
        }

        /// <summary>
        /// 预览帧查看器;
        /// </summary>
        private FramesPreviewerViewModel framesPreviewerViewModel;
        public FramesPreviewerViewModel FramesPreviewerViewModel {
            get {
                return framesPreviewerViewModel ??
                    (framesPreviewerViewModel = new FramesPreviewerViewModel(mainWindowViewModel));
            }
            set {
                framesPreviewerViewModel = value;
                NotifyPropertyChanging(nameof(FramesPreviewerViewModel));
            }
        }
    }

    /// <summary>
    /// 文件列表项的命令绑定项;
    /// </summary>
    public partial class VideoItemListViewerPageViewModel {
        #region 恢复当前文件的命令;
        private RelayCommand recoverCurFileCommand;
        public RelayCommand RecoverCurFileCommand {
            get {
                return recoverCurFileCommand ??
                    (recoverCurFileCommand = new RelayCommand(RecoverCurFileExecuted, RecoverCurFileCanExecuted));
            }
        }

        //恢复当前文件的动作;
        private void RecoverCurFileExecuted() {
            if (selectedRow != null && !selectedRow.IsEmpty) {
                RecoverFiles(new List<Video> { selectedRow.Video });
            }
        }
        //若当前行为空或未选或多选，则不能执行恢复选中行;
        private bool RecoverCurFileCanExecuted() {
            if (selectedRow == null || selectedRow.IsEmpty) {
                return false;
            }
            if(CurRows.Count(p => p.IsSelected) != 1) {
                return false;
            }
            return true;
        }
        #endregion

        #region 选中/反选刷黑行
        private DelegateCommand<bool> confirmSelectedRowsCommand;
        public DelegateCommand<bool> ConfirmSelectedRowsCommand {
            get {
                return confirmSelectedRowsCommand ??
                    (confirmSelectedRowsCommand = new DelegateCommand<bool>(ConfirmSelectedRowsExecuted, (para) => {
                        return CurRows.FirstOrDefault(p => !p.IsEmpty&&p.IsSelected) != null;
                    }));
            }
        }
        /// <summary>
        /// 执行选中/反选刷黑行的命令;
        /// </summary>
        /// <param name="para">全选/反选</param>
        private void ConfirmSelectedRowsExecuted(bool para) {
            var rows = CurRows.Where(p => p.IsSelected && !p.IsEmpty);
            if (rows.Count() != 0) {
                foreach (var row in rows) {
                    row.IsChecked = para;
                }
            }
        }
        #endregion

        #region 恢复选中文件
        private RelayCommand recoverCheckedFilesCommand;
        public RelayCommand RecoverCheckedFilesCommand {
            get {
                return recoverCheckedFilesCommand ??
                    (recoverCheckedFilesCommand = new RelayCommand(RecoverCheckedFilesExecuted,
                    RecoverCheckedFilesCanExecute));
            }
        }
        //执行恢复选中文件;
        private void RecoverCheckedFilesExecuted() {
            var checkedRows = CurRows.Where(p => p.IsChecked);
            if (checkedRows.Count() != 0) {
                RecoverFiles(checkedRows.Select(p => p.Video).ToList());
            }
        }
        //若无选中行,则不可执行;
        private bool RecoverCheckedFilesCanExecute() {
            return CurRows.FirstOrDefault(p => !p.IsEmpty && p.IsChecked) != null;
        }
        #endregion

        #region 预览文件的命令
        private RelayCommand previewItemCommand;
        public RelayCommand PreviewItemCommand {
            get {
                return previewItemCommand ??
                    (previewItemCommand = new RelayCommand(PreviewItemExecuted, PreviewItemCanExecute));
            }
        }
        //预览播放器路径;
        private const string previewerPath = @"cdfcplayer.exe";

        private void PreviewItemExecuted() {
            if (!selectedRow.IsEmpty) {
                if ( FramesPreviewerViewModel.CurPreviewBook != null) {
                    var row = selectedRow;
                    string curPath = AppDomain.CurrentDomain.BaseDirectory;
                    //var curPath = System.Environment.CurrentDirectory;
                    var playerInfo = new ProcessStartInfo("\"" + curPath + previewerPath + "\"", "\"" + framesPreviewerViewModel.CurPreviewBook.PreviewFilePath + "\"");
                    EventLogger.Logger.WriteLine("播放器位置:" + "\"" + curPath + previewerPath + "\"");
                    EventLogger.Logger.WriteLine("文件位置:" + "\"" + framesPreviewerViewModel.CurPreviewBook.PreviewFilePath + "\"");
                    playerInfo.CreateNoWindow = true ;
                    playerInfo.UseShellExecute = false;
                    playerInfo.RedirectStandardOutput = true;
                    try {
                        Process player = Process.Start(playerInfo);

                        player.EnableRaisingEvents = true;
                        player.Exited += (sender, e) => {
                            var error = player.StandardOutput.ReadToEnd();
                        };

                    }
                    catch (Exception ex) {
                        EventLogger.Logger.WriteLine("VideoItemListViewerPageViewModel->PreviewItemExecuted出错:" + ex.Message);
                        CDFCMessageBox.Show(ex.Message + curPath + previewerPath, framesPreviewerViewModel.CurPreviewBook.PreviewFilePath,MessageBoxButton.OK);
                    }

                }
            }
        }
        
        private bool PreviewItemCanExecute() {
            bool res = false;
            if(selectedRow != null && !selectedRow.IsEmpty) {
                if (CurRows.Count(p => p.IsSelected) == 1)
                    res = true;
            }
            return res;
        }
        #endregion

        #region 全选命令;
        private DelegateCommand<bool> checkAllRowsCommand;
        public DelegateCommand<bool> CheckAllRowsCommand {
            get {
                return checkAllRowsCommand ??
                    (checkAllRowsCommand = new DelegateCommand<bool>(CheckAllRowsExecuted));
            }
        }
        /// <summary>
        /// 全选/反选所有行
        /// </summary>
        /// <param name="para">全选/反选</param>
        private void CheckAllRowsExecuted(bool para) {
            foreach (var row in CurRows) {
                if (!row.IsEmpty) {
                    row.IsChecked = para;
                }
            }
        }
        #endregion

        /// <summary>
        /// 恢复文件的接口
        /// </summary>
        /// <param name="videos">所需要恢复的文件项</param>
        private void RecoverFiles(List<Video> videos) {
            //MessageBox.GetOne("使用此功能，请联系厂商!");
            IRecoveringController RecoveringController = new RecoveringController(mainWindowViewModel, videos);
            RecoveringController.Start();
        }
    }

    /// <summary>
    /// 碎片图表分析的逻辑;
    /// </summary>
    public partial class VideoItemListViewerPageViewModel {
        #region 在图表中显示某个文件的位置;
        private FragmentAnalyzerWindowViewModel fragmentAnalyzerWindowViewModel;
        private RelayCommand showVideoPositionCommand;
        public RelayCommand ShowVideoPositionCommand {
            get {
                return showVideoPositionCommand ??
                    (showVideoPositionCommand = new RelayCommand(ShowVideoPositionExecuted, ShowVideoPositionCanExecuted));
            }
        }
        /// <summary>
        /// 需在扫描方式为全盘或剩余空间扫描时才可;
        /// </summary>
        /// <returns></returns>
        private bool ShowVideoPositionCanExecuted() {
            bool res = false;
            if (selectedRow != null && !selectedRow.IsEmpty) {
                if (CurRows.Count(p => p.IsSelected) == 1) {
                    if(CDFCSetting.ScanSetting.ScanMethod == CDFCEntities.Enums.ScanMethod.Area
                        || CDFCSetting.ScanSetting.ScanMethod == CDFCEntities.Enums.ScanMethod.EntireDisk
                        || CDFCSetting.ScanSetting.ScanMethod == CDFCEntities.Enums.ScanMethod.Left)
                    res = true;
                }
            }
            return res;
        }

        private void ShowVideoPositionExecuted() {
            if (fragmentAnalyzerWindowViewModel == null) {
                fragmentAnalyzerWindowViewModel = new FragmentAnalyzerWindowViewModel(
                mainWindowViewModel);
                FragmentAnalyzerWindow window = new FragmentAnalyzerWindow(fragmentAnalyzerWindowViewModel,iObjectDevice);
                window.Show();
                ThreadPool.QueueUserWorkItem(callBack => {
                    fragmentAnalyzerWindowViewModel.InitCells(iObjectDevice.Size / (ulong)iObjectDevice.SectorSize);
                    RefreshFragments(mainWindowViewModel.Scanner.CurCategories);
                    //ShowTheVideoPosition();
                });
            }
            else if(fragmentAnalyzerWindowViewModel.Initializing) {
                CDFCMessageBox.Show("碎片图表生成中!");
            }
            else {
                ShowTheVideoPosition();
            }
        }

        private void ShowTheVideoPosition() {
            List<FileFragment> chosenFragments = null;
            if(fragmentAnalyzerWindowViewModel != null) {
                fragmentAnalyzerWindowViewModel.ClearChosenFragments();

                //若碎片图表分析模型为不可用，则可能该窗体已经关闭;
                //重启该窗体;
                if(fragmentAnalyzerWindowViewModel.Visible == Visibility.Hidden) {
                    fragmentAnalyzerWindowViewModel.Visible = Visibility.Visible;
                }
                if (CDFCSetting.ScanSetting.IsMP4Class) {
                    #region 如果为MP4扫描方式;则添加头尾文件节点;
                    if (selectedRow.Video.FileFragments.Count == 0) {
                        EventLogger.Logger.WriteLine("文件碎片显示错误!无碎片");
                        CDFCMessageBox.Show("该文件无碎片!");
                        return;
                    }
                    var headFrag = selectedRow.Video.FileFragments[0];
                    var tileFrag = new FileFragment {
                        StartAddress = headFrag.StartAddress1
                    };
                    chosenFragments = new List<FileFragment> { headFrag, tileFrag };
                    fragmentAnalyzerWindowViewModel?.SetChosenFragment(headFrag);
                    fragmentAnalyzerWindowViewModel?.SetChosenFragment(tileFrag);
                    #endregion
                }
                else {
                    chosenFragments = selectedRow.Video.FileFragments;
                }
                
                //若当前行已在图表中显示;
                if (selectedRow.HasFragShown) {
                    selectedRow.HasFragShown = false;
                    showingRow = null;
                }
                else if (chosenFragments != null) {
                    if (showingRow != null) {
                        showingRow.HasFragShown = false;
                    }
                    if (!CDFCSetting.ScanSetting.IsMP4Class) {
                        chosenFragments.ForEach(p => {
                            fragmentAnalyzerWindowViewModel?.SetChosenFragment(p);
                        });
                    }
                    selectedRow.HasFragShown = true;
                    showingRow = selectedRow;
                }
            }
            
        }

        #endregion

        //当前正在显示位置的文件行;
        public VideoRow showingRow;
        
        /// <summary>
        /// 将所有余下的碎片添加到图表中。
        /// </summary>
        /// <param name="categories">文件分类列表</param>
        /// <param name="video">已选定的文件</param>
        private void RefreshFragments(List<DateCategory> categories) {
            List<FileFragment> newFragments = new List<FileFragment>();
            List<FileFragment> newHeaderFragments = new List<FileFragment>();
            List<FileFragment> newTileFragments = new List<FileFragment>();
            if (categories == null) {
                return;
            }
            //若为Mp4扫描方式，则将文件的头尾加入碎片文件分析图表中;
            if (CDFCSetting.ScanSetting.IsMP4Class) {
                categories.ForEach(p => {
                    p.Videos.ForEach(q => {
                        //if(fragmentAnalyzerWindowViewModel == null || fragmentAnalyzerWindowViewModel.IsEnabled == false
                        //||(showingRow != null && q == showingRow.Video)) {
                        //    return;
                        //}
                        var headerFragment = q.HeaderFragment;
                        if(headerFragment != null) {
                            if(headerFragment.StartAddress == 24536064) {

                            }
                            var tileFragment = q.TileFragment;
                            //加入文件头节点显示;
                            newHeaderFragments.Add(headerFragment);
                            //加入文件尾节点显示;
                            newTileFragments.Add(tileFragment);
                        }
                        else {

                        }
                    });
                });
                var sw = new System.IO.StreamWriter("D://ff.txt");
                sw.WriteLine("Head");
                newHeaderFragments.ForEach(p => {
                    sw.WriteLine($"{p.StartAddress}:{p.Size}");
                });
                sw.WriteLine("\n\n\n");
                sw.WriteLine("Tile");
                newTileFragments.ForEach(p => {
                    sw.WriteLine($"{p.StartAddress}:{p.Size}");
                });
                sw.Close();
                mainWindowViewModel.UpdateInvoker.Invoke(() => {
                    newHeaderFragments.ForEach(p => {
                        fragmentAnalyzerWindowViewModel.AddHeaderFragment(p);
                        //var s = p.ChannelNO;
                        //var ss = fragmentAnalyzerWindowViewModel.BlockSize;
                    });

                    newTileFragments.ForEach(p => {
                        fragmentAnalyzerWindowViewModel.AddTileFragment(p);
                        //var s = p.ChannelNO;
                        //var ss = fragmentAnalyzerWindowViewModel.BlockSize;
                    });
                });
            }
            else {
                categories.ForEach(p => {        
                    //遍历文件碎片，查看是否存在不同项;若存在，则添加入curFileFragments;
                    p.Videos.ForEach(q => {
                        //////若属于已选定文件，则不将其加入剩余碎片;
                        //if (showingRow != null && showingRow.Video == q) {
                        //    return;
                        //}
                        q.FileFragments.ForEach(t => {
                            newFragments.Add(t);
                        });
                    });
                });
                lock (this) {
                    newFragments.ForEach(p => {
                        fragmentAnalyzerWindowViewModel?.AddFileFragment(p);
                    });
                }
            }


        }
        
    }
}