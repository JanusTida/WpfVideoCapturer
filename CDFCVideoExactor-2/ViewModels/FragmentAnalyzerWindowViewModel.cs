using CDFCConverter.Enums;
using CDFCEntities.Files;
using CDFCUIContracts.Commands;
using CDFCVideoExactor.Abstracts;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using CDFCMessageBoxes.MessageBoxes;
using CDFCEntities.CRecoveryMethods;
using CDFCEntities.Structs;
using CDFCStatic.CMethods;
using Ookii.Dialogs.Wpf;
using CDFCEntities.Interfaces;
using System.Diagnostics;

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// //碎片图表查看器
    /// </summary>
    public partial class FragmentAnalyzerWindowViewModel : ViewModelBase,IDisposable {
        public static readonly int PrimaryRowCount = 64;
        public static readonly int PrimaryColCount = 72;
        public double PrimaryViewerWidth { get; private set; } = 1024;
        public double PrimaryViewerHeight { get; private set; } = 768;
        public double PrimaryCellWidth {
            get {
                return PrimaryViewerWidth / PrimaryColCount;
            }
        }
        public double PrimaryCellHeight {
            get {
                return PrimaryViewerHeight / PrimaryRowCount;
            }
        }

        public static readonly int SeniorRowCount = 32;
        public static readonly int SeniorColCount = 72;
        public double SeniorViewerWidth { get; private set; } = 1024;
        public double SeniorViewerHeight { get; private set; } = 768;
        public double SeniorCellWidth {
            get {
                return SeniorViewerWidth / SeniorColCount;
            }
        }
        public double SeniorCellHeight {
            get {
                return SeniorViewerHeight / SeniorRowCount;
            }
        }
        
        public readonly MainWindowViewModel MainWindowViewModel;
        public IObjectDevice ObjectDevice {
            get {
                return null;// MainWindowViewModel.Scanner.IObjectDevice;
            }
        }

        public FragmentAnalyzerWindowViewModel(MainWindowViewModel mainWindowViewModel) {
            this.MainWindowViewModel = mainWindowViewModel;
        }

        public event EventHandler<bool> InitializingChanged;
        private bool initialLizing;
        public bool Initializing {
            get {
                return initialLizing;
            }
            private set {
                initialLizing = value;
                InitializingChanged?.Invoke(this, initialLizing);
            }
        }

        //初始化细胞(线程池方法);
        public void InitCells(ulong secCount) {
            MainWindowViewModel.UpdateInvoker(() => {
                Initializing = true;
                IsLoading = true;
            });
            
            AutoResetEvent[] waitHandles = new AutoResetEvent[] {
                new AutoResetEvent(false),
                new AutoResetEvent(false)
            };

            MainWindowViewModel.UpdateInvoker(() => {
                PrimaryCells.Clear();
            });
            ThreadPool.QueueUserWorkItem(callBack => {
                var curRecNo = 0;
                ulong sizePerBlock1 = secCount / (ulong)(PrimaryColCount * PrimaryRowCount);
                ulong sizePerBlock2 = 0;
                int primaryCellNum = PrimaryRowCount * PrimaryColCount;
                int block2StartIndex = primaryCellNum;

                if(secCount % (ulong)(PrimaryColCount * PrimaryRowCount) != 0) {
                    sizePerBlock2 = sizePerBlock1;
                    sizePerBlock1++;
                    block2StartIndex =(int)( secCount % (ulong)(PrimaryColCount * PrimaryRowCount) );
                }

                var updatePerTime = 200;
                var cells = new List<ObjectSectorCell>();
                //62914560
                ObjectSectorCell newCell = null;
                while (curRecNo < primaryCellNum) {
                    newCell = new ObjectSectorCell {
                        Width = PrimaryCellWidth,
                        Height = PrimaryCellHeight,
                        X = curRecNo % PrimaryColCount * PrimaryCellWidth,
                        Y = curRecNo / PrimaryColCount * PrimaryCellHeight
                    };
                    if(curRecNo < block2StartIndex) {
                        newCell.IniSector = (ulong) curRecNo  * sizePerBlock1;
                        newCell.EndSector = newCell.IniSector + sizePerBlock1;
                    }
                    else {
                        newCell.IniSector = (ulong) curRecNo  * sizePerBlock1 + (ulong)( block2StartIndex - curRecNo);
                        newCell.EndSector = newCell.IniSector + sizePerBlock2;
                    }
                    cells.Add(newCell);
                    curRecNo++;
                    if (curRecNo % updatePerTime == 0) {
                        MainWindowViewModel.UpdateInvoker(() => {
                            cells.ForEach(p => PrimaryCells.Add(p));
                        });
                        cells.Clear();
                        
                        Thread.Sleep(50);
                    }
                }
                MainWindowViewModel.UpdateInvoker(() => {
                    cells.ForEach(p => PrimaryCells.Add(p));
                });
                cells.Clear();
                waitHandles[0].Set();
            });

            MainWindowViewModel.UpdateInvoker(() => {
                SeniorCells.Clear();
            });
            ThreadPool.QueueUserWorkItem(callBack => {
                var curRecNo = 0;
                var updatePerTime = 200;
                var cells = new List<ObjectSectorCell>();

                while (curRecNo < SeniorColCount * SeniorRowCount) {
                    cells.Add(new ObjectSectorCell {
                        Width = SeniorCellWidth,
                        Height = SeniorCellHeight,
                        X = curRecNo % SeniorColCount * SeniorCellWidth,
                        Y = curRecNo / SeniorColCount * SeniorCellHeight
                    });
                    curRecNo++;
                    
                    if (curRecNo % updatePerTime == 10) {
                        MainWindowViewModel.UpdateInvoker(() => {
                            cells.ForEach(p => SeniorCells.Add(p));
                        });
                        cells.Clear();
                        Thread.Sleep(100);
                    }
                }
                MainWindowViewModel.UpdateInvoker(() => {
                    cells.ForEach(p => SeniorCells.Add(p));
                });
                cells.Clear();
                waitHandles[1].Set();
            });

            WaitHandle.WaitAll(waitHandles);
            MainWindowViewModel.UpdateInvoker(() => {
                Initializing = false;
                IsLoading = false;
            });
        }
        
        /// <summary>
        /// 释放碎片节点分析器对象;
        /// </summary>
        public void Dispose() {
            ThreadPool.QueueUserWorkItem(callBack => {
                //清除所有的选定区间(MP4);
                ClearSelectedCells();
                
                //清除文件头部节点;
                ClearHeaderCells();
                //清除文件尾部节点;
                ClearTileCells();
                ClearMethod();

                //清除所有被选定的节点(Size);
                PrimaryTickedCell = null;
                SeniorTickedCell = null;

                IsRanged = false;
                
                //初始化被选定的某个扇区单元(三级视图)
                SelectedSector = null;

                //初始化选定区间;
                BorderCell = null;
                ViceBorderCell = null;
            });

            //清除绑定项中的项目;
            PrimaryCells.Clear();
            SeniorCells.Clear();
            DetailSectorCells.Clear();
            IsEnabled = false;
        }
    }

    /// <summary>
    /// 碎片图表查看器的状态;
    /// </summary>
    public partial class FragmentAnalyzerWindowViewModel  {
        //所有的碎片;
        private List<FileFragment> normalFragments = new List<FileFragment>();
        private List<FileFragment> tileFragments = new List<FileFragment>();
        private List<FileFragment> headFragments = new List<FileFragment>();

        #region 一二级视图的行，属性，方法;
        //第一级设定的行(可见);
        public ObservableCollection<ObjectSectorCell> PrimaryCells { get; set; } = new ObservableCollection<ObjectSectorCell>();
        //第二级设定的行(可见);
        public ObservableCollection<ObjectSectorCell> SeniorCells { get; set; } = new ObservableCollection<ObjectSectorCell>();
        
        #endregion
        
        
        //第一级被(鼠标)选定的节点;
        private ObjectSectorCell primaryTickedCell;
        public ObjectSectorCell PrimaryTickedCell {
            get {
                return primaryTickedCell;
            }
            set {
                if(primaryTickedCell != null) {
                    primaryTickedCell.CellState &= ~CellStatement.Ticked;
                }
                primaryTickedCell = value;
                if (primaryTickedCell != null) {
                    primaryTickedCell.CellState |= CellStatement.Ticked;
                }

            }
        }
        //第二级被(鼠标)选定的节点;
        private ObjectSectorCell seniorTickedCell;
        public ObjectSectorCell SeniorTickedCell {
            get {
                return seniorTickedCell;
            }
            set {
                if (seniorTickedCell != null) {
                    seniorTickedCell.CellState &= ~CellStatement.Ticked;
                }
                seniorTickedCell = value;
                if (seniorTickedCell != null) {
                    seniorTickedCell.CellState |= CellStatement.Ticked;
                }
            }
        }

        //第三级被选定的扇区单元;
        public SectorItemCell SelectedSector { get; set; }
        /// <summary>
        /// 各级别的选定单元集合;
        /// </summary>
        private List<ObjectSectorCell> primarySelectedCells = new List<ObjectSectorCell>();
        private List<ObjectSectorCell> seniorSelectedCells = new List<ObjectSectorCell>();
        private List<SectorItemCell> detailSelecetedCells = new List<SectorItemCell>();
        //被置为临界节点扇区;
        public SectorItemCell BorderCell { get; set; }
        public SectorItemCell ViceBorderCell { get; set; }
        //两个临界区节点是否都被选定;
        public bool IsRanged { get; set; }
        /// <summary>
        ///  重置选定区间的选定单元状态（MP4);
        /// </summary>
        private void ResetSelectedCells() {
            primarySelectedCells.ForEach(p => p.CellState &= ~CellStatement.Selected);
            seniorSelectedCells.ForEach(p => p.CellState &= ~CellStatement.Selected);
            detailSelecetedCells.ForEach(p => p.CellState &= ~CellStatement.Selected);
        }

        /// <summary>
        /// 释放选定区间的选定状态单元（MP4);
        /// </summary>
        private void ClearSelectedCells() {
            primarySelectedCells.Clear();
            seniorSelectedCells.Clear();
            detailSelecetedCells.Clear();
        }
        /// <summary>
        /// 设定选定区间，并即时显示在屏幕上;
        /// </summary>
        /// <param name="sectorAddress">临界1</param>
        /// <param name="viceSectorAddress">临界2</param>
        public void SetSelectedRange() {
            //RefreshSelectedRange();
            IsRanged = true;
        }
        //public void RefreshSelectedRange() {
        //    ResetSelectedCells();
        //    ClearSelectedCells();

        //    var add1 = BorderCell.SectorAddress;
        //    var add2 = ViceBorderCell.SectorAddress;
        //    var iniAddress = add1 > add2 ? add2 : add1;
        //    var endAddress = add1 > add2 ? add1 : add2;
        //    foreach (var row in Rows) {
        //        var cells = row.Cells.Where(p => p.IniSector >= iniAddress
        //        && p.EndSector <= endAddress);
        //        primarySelectedCells.AddRange(cells);
        //    }
        //    foreach (var row in DetailRows) {
        //        var cells = row.Cells.Where(p => p.IniSector >= iniAddress
        //        && p.EndSector <= endAddress);
        //        seniorSelectedCells.AddRange(cells);
        //    }
        //    detailSelecetedCells = DetailSectorCells.Where(p => p.SectorAddress >= iniAddress &&
        //    p.SectorAddress <= endAddress).ToList();

        //    seniorSelectedCells.ForEach(p => { p.CellState |= CellStatement.Selected; });
        //    primarySelectedCells.ForEach(p => { p.CellState |= CellStatement.Selected; });
        //    detailSelecetedCells.ForEach(p => { p.CellState |= CellStatement.Selected; });

        //}
        

        /// <summary>
        /// 增加总的文件存在碎片显示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddFileFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            
            foreach (var cell in PrimaryCells) {
                if (!((startAddress / (ulong)SectorSize > cell.EndSector) || (startAddress + fileFragment.Size) / (ulong)SectorSize < cell.IniSector)) {
                    cell.CellState |= CellStatement.HasFile;
                    cell.CellFragments.Add(new CellFragment { Fragment = fileFragment, FragmentStatement = CellStatement.HasFile });
                    //reached = true;
                }
            }
            normalFragments.Add(fileFragment);
        }
        
        #region 被文件选中碎片节点的处理(右键)
        /// <summary>
        /// 清除被文件选中的碎片节点显示;
        /// </summary>
        public void UpdateChosenCells() {
            
        }
        
        #endregion

        #region 文件头尾的处理;
        
        /// <summary>
        /// 清除所有文件碎片尾的显示
        /// </summary>
        public void ClearTileCells() {
            
        }
        /// <summary>
        /// 增加文件尾部的碎片表示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddTileFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            foreach(var cell in PrimaryCells) {
                if (startAddress / (ulong)SectorSize >= cell.IniSector && startAddress / (ulong)SectorSize < cell.EndSector) {
                    cell.CellFragments.Add(new CellFragment { Fragment = fileFragment, FragmentStatement = CellStatement.Tile });
                    cell.CellState |= CellStatement.Tile;
                    break;
                }
            }
            tileFragments.Add(fileFragment);
        }
        
        /// <summary>
        /// 清除文件头的碎片表示;
        /// </summary>
        public void ClearHeaderCells() {
            
        }
        /// <summary>
        /// 增加文件头的碎片表示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddHeaderFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            foreach (var cell in PrimaryCells) {
                if (startAddress / (ulong)SectorSize >= cell.IniSector && startAddress / (ulong)SectorSize < cell.EndSector) {
                    cell.CellFragments.Add(new CellFragment { Fragment = fileFragment, FragmentStatement = CellStatement.Head });
                    cell.CellState |= CellStatement.Head;
                    break;
                }
            }
            headFragments.Add(fileFragment);
        }
        #endregion

        private void ClearMethod() {
            //IniRows(Rows);
            //IniRows(DetailRows);
        }

        public ObservableCollection<SectorItemCell> DetailSectorCells { get; set; } = new ObservableCollection<SectorItemCell>();

        private SectorItemCell selectedDetailSectorCell;
        public SectorItemCell SelectedDetailSectorCell {
            get {
                return selectedDetailSectorCell;
            }
            set {
                selectedDetailSectorCell = value;
            }
        }

        //每个单元格的大小;
        private int blockSize;
        public int BlockSize {
            get {
                return blockSize;
            }
            set {
                blockSize = value;
                NotifyPropertyChanging(nameof(BlockSize));
            }
        }
        
        public int SectorSize {
            get {
                if(MainWindowViewModel != null && MainWindowViewModel.Scanner != null) {
                    return MainWindowViewModel.Scanner.IObjectDevice.SectorSize;
                }
                else{
                    return 512;
                }
            }
        }
        
        //窗体是否可用,用于窗体的关闭动作;
        private bool isEnabled = true;
        public bool IsEnabled {
            get {
                return isEnabled;
            }
            set {
                isEnabled = value;
                NotifyPropertyChanging(nameof(IsEnabled));
            }
        }

        /// <summary>
        /// 窗体是否可见，用于控制窗体的隐藏;
        /// </summary>
        private Visibility visible = Visibility.Visible;
        public Visibility Visible {
            get {
                return visible;
            }
            set {
                visible = value;
                if (value == Visibility.Hidden) {
                    MainWindowViewModel.UpdateInvoker(() => {
                        HiddenAct?.Invoke();
                    });
                }
                else {
                    MainWindowViewModel.UpdateInvoker(() => {
                        ShowAct?.Invoke();
                    });
                }
                NotifyPropertyChanging(nameof(Visible));
            }
        }

        /// <summary>
        /// 是否正在等待;
        /// </summary>
        private bool isLoading = false;
        public bool IsLoading {
            get {
                return isLoading;
            }
            set {
                isLoading = value;
                NotifyPropertyChanging(nameof(IsLoading));
            }
        }

        private ulong? selectionStartSector;
        public ulong? SelectionStartSector {
            get {
                return selectionStartSector;
            }
            set {
                if(SelectionEndSector != null) {
                    if(value == SelectionEndSector) {
                        CDFCMessageBox.Show("起始扇区与终止扇区不可相同!");
                        return;
                    }
                    else if(value > SelectionEndSector) {
                        SelectionEndSector = null;
                    }
                }
                selectionStartSector = value;
                //1471778
                //1485537
                if (selectionStartSector != null) {
                    UpdateSelectionView();
                }
            }
        }

        private ulong? selectionEndSector;
        public ulong? SelectionEndSector {
            get {
                return selectionEndSector; ;
            }
            set {
                if (SelectionStartSector != null) {
                    if (value == SelectionStartSector) {
                        CDFCMessageBox.Show("起始扇区与终止扇区不可相同!");
                        return;
                    }
                    else if(value < SelectionStartSector) {
                        SelectionStartSector = null;
                    }
                    
                }
                selectionEndSector = value;
                
                if (selectionEndSector != null) {
                    UpdateSelectionView();
                }
            }
        }

        public void ClearChosenFragments() {
            foreach (var cell in PrimaryCells) {
                cell.CellState &= ~CellStatement.Chosen;
            }
            foreach(var cell in SeniorCells) {
                cell.CellState &= ~CellStatement.Chosen;
            }
            chosenCellFrags.ForEach(p => p.FragmentStatement &= ~CellStatement.Chosen);
            chosenCellFrags.Clear();
        }

        private List<CellFragment> chosenCellFrags = new List<CellFragment>();

        public void SetChosenFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            //是否触及到了细胞;
            //bool reached = false;
            foreach (var cell in PrimaryCells) {
                var ownedFragment = cell.CellFragments.FirstOrDefault(p => p.Fragment == fileFragment);
                if (ownedFragment != null) {
                    cell.CellState |= CellStatement.Chosen;
                    ownedFragment.FragmentStatement |= CellStatement.Chosen;
                    chosenCellFrags.Add(ownedFragment);
                    
                }
                //if (reached) {

                //    break;
                //}
            }
            foreach(var cell in SeniorCells) {
                var ownedFragment = cell.CellFragments.FirstOrDefault(p => p.Fragment == fileFragment);
                if (cell.CellFragments.FirstOrDefault(p => p.Fragment == fileFragment) != null) {
                    cell.CellState |= CellStatement.Chosen;
                    if (ownedFragment != null) {
                        ownedFragment.FragmentStatement |= CellStatement.Chosen;
                    }
                }
            }
        }

        private void UpdateSeniorChosenState() {

        }
        private void UpdateSelectionView() {
            if(SelectionStartSector != null && SelectionEndSector != null) {
                foreach (var cell in PrimaryCells) {
                    if (!(cell.IniSector > SelectionEndSector || cell.EndSector <= SelectionStartSector) ){
                        cell.CellState |= CellStatement.Selected;
                    }
                    else {
                        cell.CellState &= ~CellStatement.Selected;
                    }
                }
                foreach (var cell in SeniorCells) {
                    if (!(cell.IniSector > SelectionEndSector || cell.EndSector <= SelectionStartSector)) {
                        cell.CellState |= CellStatement.Selected;
                    }
                    else {
                        cell.CellState &= ~CellStatement.Selected;
                    }
                }
                
                foreach(var cell in DetailSectorCells) {
                    if(SelectionStartSector <= cell.SectorAddress && cell.SectorAddress <= SelectionEndSector) {
                        cell.CellState |= CellStatement.Selected;
                    }
                    else {
                        cell.CellState &= ~CellStatement.Selected;
                    }
                }
            }
            else {
                foreach (var cell in PrimaryCells) {cell.CellState &= ~CellStatement.Selected;}
                foreach (var cell in SeniorCells) { cell.CellState &= ~CellStatement.Selected; }
                foreach (var cell in DetailSectorCells) { cell.CellState &= ~CellStatement.Selected; }
            }
        }

        //设定次级试图的选定单元;
        public void SetSeniorRange(ObjectSectorCell cell) {
            //清除第二级的放大节点显示;
            DetailSectorCells.Clear();
            
            SeniorTickedCell = null;
            
            ulong objectSize = cell.EndSector - cell.IniSector;
            ulong seniorCellNum = (ulong) (SeniorColCount * SeniorRowCount);
            ulong blockSize1 = objectSize / seniorCellNum;
            ulong blockSize2 = 0;
            ulong block2StartIndex = seniorCellNum;

            if(objectSize % seniorCellNum != 0) {
                if(objectSize % seniorCellNum != 0) {
                    blockSize2 = blockSize1;
                    blockSize1++;
                    block2StartIndex = objectSize % seniorCellNum ;
                }
            }
            List<CellFragment> fragments = cell.CellFragments;
            
            ThreadPool.QueueUserWorkItem(callBack => {
                for (ulong index = 0; index < seniorCellNum; index++) {
                    ObjectSectorCell newCell = SeniorCells[(int)index];
                    newCell.CellFragments.Clear();
                    if(index < block2StartIndex) {
                        newCell.IniSector = cell.IniSector + (index * blockSize1 );
                        newCell.EndSector = newCell.IniSector + blockSize1;
                    }
                    else {
                        newCell.IniSector = cell.IniSector + (index * blockSize1 + block2StartIndex - index );
                        newCell.EndSector = newCell.IniSector + blockSize2;
                    }
                    newCell.Exist = newCell.EndSector - newCell.IniSector != 0;
                    if (!newCell.Exist) {

                    }
                    IEnumerable<CellFragment> objectFragments = null;
                    if (!CDFCSetting.ScanSetting.IsMP4Class) {
                        objectFragments = fragments.Where(p => {
                            return !((p.Fragment.StartAddress > newCell.EndSector * (ulong)SectorSize ||
                            (p.Fragment.StartAddress + p.Fragment.Size) < newCell.IniSector * (ulong)SectorSize));
                        });
                    }
                    else {
                        objectFragments = fragments.Where(p => {
                            return (p.Fragment.StartAddress < newCell.EndSector * (ulong)SectorSize &&
                            p.Fragment.StartAddress >= newCell.IniSector * (ulong)SectorSize);
                        });
                    }
                    if (objectFragments.Count() != 0) {
                        newCell.CellState = CellStatement.None;
                        foreach (var frag in objectFragments) {
                            newCell.CellState |= frag.FragmentStatement;
                        }
                        
                        //if ((newCell.CellState & CellStatement.ti) != 0) {
                        //    //SetSeniorChosenCell(newCell);
                        //}
                        newCell.CellFragments.AddRange(objectFragments);
                    }
                    else {
                        newCell.CellState = CellStatement.None;
                    }
                }
                //if(cell.EndSector - cell.IniSector > seniorCellNum) {
                //    SeniorCells.Last().EndSector = cell.EndSector;
                //}
                UpdateSelectionView();
            });
            
        }

        private void UpdateSeniorView() {

        }

        public void ShowCellFragments(ObjectSectorCell cell) {
            int seniorCellNum = PrimaryColCount * PrimaryRowCount;
            ulong objectSize = cell.EndSector - cell.IniSector;
            //int blockSize = Convert.ToInt32((objectSize / (seniorCellNum) * (seniorCellNum) + seniorCellNum) / seniorCellNum);
            List<CellFragment> fragments = cell.CellFragments;

            if (fragments != null && fragments.Count != 0) {
                FragmentsViewerWindowViewModel fragViewerWindowModel = new FragmentsViewerWindowViewModel(MainWindowViewModel.Scanner.IObjectDevice, MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer) {
                    CellFragments = new ObservableCollection<CellFragment>(fragments)
                };
                FragmentsViewerWindow fragmentViewerWindow = new FragmentsViewerWindow(fragViewerWindowModel);
                fragmentViewerWindow.ShowDialog();
            }
        }

        public void ShowCellSectors(ObjectSectorCell cell) {
            //清除第三级节点显示;
            DetailSectorCells.Clear();
            //设定三级视图的节点显示;
            ThreadPool.QueueUserWorkItem(callBack => {
                if (cell != null) {
                    int cellCount = Convert.ToInt32(cell.EndSector - cell.IniSector);
                    //int cellCount = 0;
                    for (int index = 0; index < cellCount; index++) {
                        SectorItemCell sCell = new SectorItemCell();
                        sCell.SectorAddress = cell.IniSector + (ulong)index;
                        var cellFragment = cell.CellFragments.FirstOrDefault(p =>
                        !(p.Fragment.StartAddress > (sCell.SectorAddress + 1) * (ulong)SectorSize
                        || p.Fragment.StartAddress + p.Fragment.Size < sCell.SectorAddress * (ulong)SectorSize));

                        if (cellFragment != null) {
                            sCell.CellState = cellFragment.FragmentStatement;
                            sCell.CellFragment = cellFragment;
                        }
                        MainWindowViewModel.UpdateInvoker.Invoke(() => {
                            DetailSectorCells.Add(sCell);
                            if (IsRanged) {
                                //RefreshSelectedRange();
                            }
                        });
                    }
                    MainWindowViewModel.UpdateInvoker(() => {
                        UpdateSelectionView();
                    });
                    UpdateSelectionView();
                }

            });
            //设定二级视图选中扇区单元的大小;
            ThreadPool.QueueUserWorkItem(callBack => {
                SeniorTickedCell = cell;
            });
        }
    }

    /// <summary>
    /// 碎片图表查看器的命令绑定;
    /// </summary>
    public partial class FragmentAnalyzerWindowViewModel {
        public Action HiddenAct { get; set; }
        public Action ShowAct { get; set; }
        /// <summary>
        /// 当窗体已关闭时的命令;
        /// </summary>
        private DelegateCommand<CancelEventArgs> closingCommand;
        public DelegateCommand<CancelEventArgs> ClosingCommand {
            get {
                return closingCommand ??
                    (closingCommand = new DelegateCommand<CancelEventArgs>(
                        (e) => {
                            if (IsEnabled) {
                                Visible = Visibility.Hidden;
                                e.Cancel = true;
                            }
                        }
                    ));
            }
        }

        private RelayCommand setAsStartCommand;
        public RelayCommand SetAsStartCommand {
            get {
                return setAsStartCommand ??
                    (setAsStartCommand = new RelayCommand(
                        () =>
                        {
                            if(SelectedDetailSectorCell != null) {
                                SelectionStartSector = SelectedDetailSectorCell.SectorAddress;
                            }
                        },
                        () => {
                            return !CDFCSetting.ScanSetting.IsMP4Class && SelectedDetailSectorCell != null;
                        }
                        ));
            }
        }

        private RelayCommand setAsEndCommand;
        public RelayCommand SetAsEndCommand {
            get {
                return setAsEndCommand ??
                    (setAsEndCommand = new RelayCommand(
                        () => {
                            if (SelectedDetailSectorCell != null) {
                                SelectionEndSector = SelectedDetailSectorCell.SectorAddress;
                            }
                        },
                        () => {
                            return !CDFCSetting.ScanSetting.IsMP4Class && SelectedDetailSectorCell != null;
                        }
                        ));
            }
        }

        private RelayCommand reScanSelectedAreaCommand;
        public RelayCommand ReScanSelectedAreaCommand   {
            get {
                return reScanSelectedAreaCommand ??
                    (reScanSelectedAreaCommand = new RelayCommand(() => {
                        if(MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer
                            || MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.CPAndMultiMedia) {
                            if(MainWindowViewModel.VideoItemListViewerPageViewModel.ActualRows.FirstOrDefault() != null) {
                                if (CDFCMessageBox.Show("这将覆盖当前结果,确定?","提示",MessageBoxButton.YesNo) != MessageBoxResult.Yes) {
                                    return;
                                }
                            }
                            MainWindowViewModel.VideoItemListViewerPageViewModel.Exit();
                            MainWindowViewModel.Scanner?.Dispose();
                            if (MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer) {
                                MainWindowViewModel.PrimaryObjectScanSettingPageViewModel.ObjectScanSetting.IniSector = (ulong)SelectionStartSector;
                                MainWindowViewModel.PrimaryObjectScanSettingPageViewModel.ObjectScanSetting.EndSector = (ulong)SelectionEndSector;
                                MainWindowViewModel.PrimaryObjectScanSettingPageViewModel.SureDoCommand.Execute(true);
                            }
                            else if(MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.CPAndMultiMedia) {
                                MainWindowViewModel.CPAndMPrimarySettingViewModel.ObjectScanSetting.IniSector = (ulong)SelectionStartSector;
                                MainWindowViewModel.CPAndMPrimarySettingViewModel.ObjectScanSetting.EndSector = (ulong)SelectionEndSector;
                                MainWindowViewModel.CPAndMPrimarySettingViewModel.SureDoCommand.Execute(true);
                            }
                            else if(MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.MultiMedia) {
                                MainWindowViewModel.MultiMediaPrimaryObjectScanSettingPageViewModel.ObjectScanSetting.IniSector = (ulong)SelectionStartSector;
                                MainWindowViewModel.MultiMediaPrimaryObjectScanSettingPageViewModel.ObjectScanSetting.EndSector = (ulong)SelectionEndSector;
                                MainWindowViewModel.MultiMediaPrimaryObjectScanSettingPageViewModel.SureDoCommand.Execute(true);
                            }
 
                        }
                    },() => !CDFCSetting.ScanSetting.IsMP4Class && SelectionEndSector != null && SelectionStartSector != null
                    ));
            }
        }

        private RelayCommand setAsMP4HeadCommand;
        public RelayCommand SetAsMP4HeadCommand {
            get {
                return setAsMP4HeadCommand ??
                    (setAsMP4HeadCommand = new RelayCommand(() => {
                        if((SelectedDetailSectorCell.CellFragment.FragmentStatement & CellStatement.Head) != 0) {
                            selectedHeadFragment = SelectedDetailSectorCell.CellFragment.Fragment;
                            SelectionStartSector = SelectedDetailSectorCell.SectorAddress;
                        }
                        else {
                            CDFCMessageBox.Show("当前扇区无文件头部!");
                        }
                    },() => {
                        return CDFCSetting.ScanSetting.IsMP4Class && SelectedDetailSectorCell != null
                        && SelectedDetailSectorCell.CellFragment != null
                        && (SelectedDetailSectorCell.CellFragment.FragmentStatement & CellStatement.Head) != 0;
                    }));
            }
        }

        private RelayCommand setAsMP4TileCommand;
        public RelayCommand SetAsMP4TileCommand {
            get {
                return setAsMP4TileCommand ??
                    (setAsMP4TileCommand = new RelayCommand(() => {
                        selectedTileFragment = SelectedDetailSectorCell.CellFragment.Fragment;
                        SelectionEndSector = SelectedDetailSectorCell.SectorAddress;
                    }, () => {
                        return CDFCSetting.ScanSetting.IsMP4Class && SelectedDetailSectorCell != null
                        && SelectedDetailSectorCell.CellFragment != null
                        && ( SelectedDetailSectorCell.CellFragment.FragmentStatement & CellStatement.Tile ) != 0;
                    }));
            }
        }

        private FileFragment selectedHeadFragment;
        private FileFragment selectedTileFragment;

        private RelayCommand reCompositeMP4FileCommand;
        public RelayCommand RecompositeMP4FileCommand {
            get {
                return reCompositeMP4FileCommand ??
                    (reCompositeMP4FileCommand = new RelayCommand(() => {
                        if(selectedHeadFragment != null && selectedTileFragment != null) {
                            var ftypPos = selectedHeadFragment.StartAddress;
                            var moovPos = selectedTileFragment.StartAddress;
                            var moovSize = selectedTileFragment.Size;

                            int clusterSize = 0;
                            if(MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.Capturer) {
                                clusterSize = MainWindowViewModel.PrimaryObjectScanSettingPageViewModel.ObjectScanSetting.ClusterSize;
                            }
                            else if(MainWindowViewModel.SelectedEntranceType == Enums.EntranceType.CPAndMultiMedia) {
                                clusterSize = MainWindowViewModel.CPAndMPrimarySettingViewModel.ObjectScanSetting.ClusterSize;
                            }
                            else {
                                return;
                            }
                            
                            if(clusterSize == 0) {
                                clusterSize = MP4RecoveryMethods.GetCluster(MainWindowViewModel.Scanner.IObjectDevice.Handle);
                            }
                            if(clusterSize == 0) {
                                CDFCMessageBox.Show("簇大小获取失败!");
                                return;
                            }

                            MP4RecoveryMethods.SetCluster(clusterSize);
                            var video = MP4RecoveryMethods.CreateFile(ftypPos, moovSize, moovPos);
                            
                            if (CDFCMessageBox.Show("重组完成,需要保存文件?",MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                                var saveFileDialog = new VistaSaveFileDialog();
                                if (saveFileDialog.ShowDialog() == true) {
                                    MainWindowViewModel.Scanner.DefaultRecoverer.Init(video);
                                    IsLoading = true;
                                    ThreadPool.QueueUserWorkItem(callBack => {
                                        MainWindowViewModel.Scanner.DefaultRecoverer.SaveAs(saveFileDialog.FileName);
                                        MainWindowViewModel.UpdateInvoker(() => {
                                            IsLoading = false;
                                        });
                                        var path = saveFileDialog.FileName.Substring(0, saveFileDialog.FileName.LastIndexOf("\\"));
                                        Process.Start("explorer", path);
                                    });
                                }
                            }
                            
                            MP4RecoveryMethods.CreateFileExit();
                        }
                    },
                    () => {
                        return CDFCSetting.ScanSetting.IsMP4Class && selectedTileFragment != null 
                        && selectedHeadFragment != null;
                    }));
            }
        }
    }
   
    
}
