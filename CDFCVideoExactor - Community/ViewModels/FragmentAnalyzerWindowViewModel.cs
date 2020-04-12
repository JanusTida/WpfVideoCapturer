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

namespace CDFCVideoExactor.ViewModels {
    /// <summary>
    /// //碎片图表查看器
    /// </summary>
    public partial class FragmentAnalyzerWindowViewModel : ViewModelBase,IDisposable {
        public static readonly int DefaultRowCount = 54;
        public static readonly int DefaultColCount = 140;
        public static readonly int DefaultObjectRowCount = 25;
        public static readonly int DefaultObjectColCount = 140;
        public readonly MainWindowViewModel MainWindowViewModel;

        public FragmentAnalyzerWindowViewModel(MainWindowViewModel mainWindowViewModel, int primaryRowNum, int primaryColNum, int seniorRowNum, int seniorColNum) {
            this.MainWindowViewModel = mainWindowViewModel;
            this.primaryColNum = primaryColNum;
            this.primaryRowNum = primaryRowNum;
            int primaryCellNum = primaryRowNum * primaryColNum;

            Rows = new List<Row>();
            Fragments = new List<FileFragment>();
            DetailRows = new List<Row>();
            DetailSectorCells = new ObservableCollection<SectorItemCell>();

            primaryChosenCells = new List<ObjectSectorCell>();
            seniorChosenCells = new List<ObjectSectorCell>();

            headerCells = new List<ObjectSectorCell>();
            tileCells = new List<ObjectSectorCell>();

            for (int row = 0; row < primaryRowNum; row++) {
                Row rowVal = new Row();
                for (int col = 0; col < primaryColNum; col++) {
                    rowVal.Cells.Add(new ObjectSectorCell());
                }
                Rows.Add(rowVal);
            }

            for (int row = 0; row < seniorRowNum; row++) {
                Row rowVal = new Row();
                for (int col = 0; col < seniorColNum; col++) {
                    rowVal.Cells.Add(new ObjectSectorCell());
                }
                DetailRows.Add(rowVal);
            }
            PrimaryRows = new ObservableCollection<Row>();
            SeniorRows = new ObservableCollection<Row>();
        }

        /// <summary>
        /// 释放碎片节点分析器对象;
        /// </summary>
        public void Dispose() {
            ThreadPool.QueueUserWorkItem(callBack => {
                //清除所有的选定区间(MP4);
                ClearSelectedCells();
                //清除被文件选中节点;
                ClearChosenCells();

                //清除文件头部节点;
                ClearHeaderCells();
                //清除文件尾部节点;
                ClearTileCells();
                ClearMethod();

                //清除所有被选定的节点(Size);
                PrimarySelectedCell = null;
                SeniorSelectedCell = null;

                IsRanged = false;


                //初始化更新状态;
                curUpdatingRow = 0;
                curDetailUpdatingRow = 0;
                Updated = false;


                //初始化被选定的某个扇区单元(三级视图)
                SelectedSector = null;

                //初始化选定区间;
                BorderCell = null;
                ViceBorderCell = null;
            });

            //清除绑定项中的项目;
            PrimaryRows.Clear();
            SeniorRows.Clear();
            DetailSectorCells.Clear();
            IsEnabled = false;
        }
    }

    /// <summary>
    /// 碎片图表查看器的状态;
    /// </summary>
    public partial class FragmentAnalyzerWindowViewModel  {
        //所有的碎片;
        private List<FileFragment> Fragments { get; set; }
        //当前正在更新的行号(二级视图)
        private int curDetailUpdatingRow;
        //当前正在更新的行号(一级视图)
        private int curUpdatingRow;


        #region 一二级视图的行，属性，方法;
        //第一级设定的行(可见);
        public ObservableCollection<Row> PrimaryRows { get; set; }
        //第二级设定的行(可见);
        public ObservableCollection<Row> SeniorRows { get; set; }

        //第二级视图的所有行;
        public List<Row> DetailRows { get; set; }
        //第一级视图的所有行;
        public List<Row> Rows { get; set; }

        //可见的行是否已经更新完毕;
        public bool Updated { get; set; }

        /// <summary>
        /// 逐行增加可视行;
        /// </summary>
        public void UpdateViewRowByRow() {
            bool updatingCompleted = true;
            if (curUpdatingRow < Rows.Count) {
                PrimaryRows.Add(Rows[curUpdatingRow++]);
                updatingCompleted = false;
            }
            if (curDetailUpdatingRow < DetailRows.Count) {
                SeniorRows.Add(DetailRows[curDetailUpdatingRow++]);
                updatingCompleted = false;
            }
            Updated = updatingCompleted;
        }


        #endregion


        #region MP4选定状态的属性与
        //第一级被选定的节点;
        private ObjectSectorCell primarySelectedCell;
        public ObjectSectorCell PrimarySelectedCell {
            get {
                return primarySelectedCell;
            }
            set {
                if (primarySelectedCell != null) {
                    primarySelectedCell.Width = 10;
                    PrimarySelectedCell.Height = 10;
                }
                primarySelectedCell = value;
                if (primarySelectedCell != null) {
                    primarySelectedCell.Width = 20;
                    primarySelectedCell.Height = 20;
                }

            }
        }
        //第二级被选定的节点;
        private ObjectSectorCell seniorSelectedCell;
        public ObjectSectorCell SeniorSelectedCell {
            get {
                return seniorSelectedCell;
            }
            set {
                if (seniorSelectedCell != null) {
                    seniorSelectedCell.Width = 10;
                    seniorSelectedCell.Height = 10;
                }
                seniorSelectedCell = value;
                if (seniorSelectedCell != null) {
                    seniorSelectedCell.Width = 20;
                    seniorSelectedCell.Height = 20;
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
            RefreshSelectedRange();
            IsRanged = true;
        }
        public void RefreshSelectedRange() {
            ResetSelectedCells();
            ClearSelectedCells();

            var add1 = BorderCell.SectorAddress;
            var add2 = ViceBorderCell.SectorAddress;
            var iniAddress = add1 > add2 ? add2 : add1;
            var endAddress = add1 > add2 ? add1 : add2;
            foreach (var row in Rows) {
                var cells = row.Cells.Where(p => p.IniSector >= iniAddress
                && p.EndSector <= endAddress);
                primarySelectedCells.AddRange(cells);
            }
            foreach (var row in DetailRows) {
                var cells = row.Cells.Where(p => p.IniSector >= iniAddress
                && p.EndSector <= endAddress);
                seniorSelectedCells.AddRange(cells);
            }
            detailSelecetedCells = DetailSectorCells.Where(p => p.SectorAddress >= iniAddress &&
            p.SectorAddress <= endAddress).ToList();

            seniorSelectedCells.ForEach(p => { p.CellState |= CellStatement.Selected; });
            primarySelectedCells.ForEach(p => { p.CellState |= CellStatement.Selected; });
            detailSelecetedCells.ForEach(p => { p.CellState |= CellStatement.Selected; });

        }
        #endregion

        /// <summary>
        /// 增加总的文件存在碎片显示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddFileFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            foreach (var row in Rows) {
                foreach (var cell in row.Cells) {
                    if (!((startAddress / (ulong)SectorSize > cell.EndSector) || ( startAddress + fileFragment.Size) / (ulong)SectorSize  < cell.IniSector)) {
                        cell.CellState |= CellStatement.HasFile;
                        cell.CellFragments.Add(new CellFragment { Fragment = fileFragment, FragmentStatement = CellStatement.HasFile });
                        //reached = true;
                    }
                }
                
            }
            Fragments.Add(fileFragment);
        }


        #region 被文件选中碎片节点的处理(右键)
        ///被选中碎片节点的集合(右键,一级视图);
        private List<ObjectSectorCell> primaryChosenCells;
        private List<ObjectSectorCell> seniorChosenCells;
        /// <summary>
        /// 清除被文件选中的碎片节点显示;
        /// </summary>
        public void ClearChosenCells() {
            primaryChosenCells.ForEach(p => {
                p.CellState &= ~CellStatement.Chosen;
                p.CellFragments?.ForEach(q => {
                    q.FragmentStatement &= ~CellStatement.Chosen;
                });
            });
            //var sure = chosenCells.Exists(p => p.OriCellState == CellStatement.Chosen) ;
            primaryChosenCells.Clear();
            seniorChosenCells.ForEach(p => {
                p.CellState &= ~CellStatement.Chosen;
                p.CellFragments.ForEach(q => {
                    q.FragmentStatement &= ~CellStatement.Chosen;
                });
            });
            seniorChosenCells.Clear();
            lock (DetailSectorCells) {
                foreach (var cell in DetailSectorCells) {
                    if (cell.CellFragment != null) {
                        cell.CellFragment.FragmentStatement &= ~CellStatement.Chosen;
                        cell.CellState &= ~CellStatement.Chosen;
                    }
                }
            }
        }
        public void AddSeniorChosenCell(ObjectSectorCell cell) {
            seniorChosenCells.Add(cell);
        }
        /// <summary>
        /// 增加被选中的碎片节点显示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddChosenFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            //是否触及到了细胞;
            //bool reached = false;
            foreach (var row in Rows) {
                foreach (var cell in row.Cells) {
                    if(!((startAddress / (ulong)SectorSize > cell.EndSector) || (startAddress + fileFragment.Size) / (ulong)SectorSize < cell.IniSector)) {
                        cell.CellState |= CellStatement.Chosen;
                        cell.CellState |= CellStatement.HasFile;
                        //判断该细胞中是否存在同一个碎片;
                        var fragEntity = cell.CellFragments.FirstOrDefault(p => p.Fragment == fileFragment);
                        //若不存在,则将其添加到该细胞碎片列表中;
                        if(fragEntity == null) {
                            cell.CellFragments.Add(new CellFragment { Fragment = fileFragment,FragmentStatement = CellStatement.HasFile|CellStatement.Chosen});
                        }
                        else {
                            fragEntity.FragmentStatement |= CellStatement.Chosen;
                        }
                        
                    //    reached = true;
                        primaryChosenCells.Add(cell);
                        
                    }
                }
                //if (reached) {
                    
                //}
            }
        }

        #endregion

        #region 文件头尾的处理;
        private List<ObjectSectorCell> tileCells;
        /// <summary>
        /// 清除所有文件碎片尾的显示
        /// </summary>
        public void ClearTileCells() {
            tileCells.ForEach(p => p.CellState &= ~CellStatement.Tile);
            tileCells.Clear();
        }
        /// <summary>
        /// 增加文件尾部的碎片表示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddTileFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            bool reached = false;
            foreach (var row in Rows) {
                foreach (var cell in row.Cells) {
                    if (startAddress / (ulong)SectorSize >= cell.IniSector && startAddress / (ulong)SectorSize < cell.EndSector) {
                        cell.CellFragments.Add(new CellFragment { Fragment = fileFragment, FragmentStatement = CellStatement.Tile });
                        cell.CellState |= CellStatement.Tile;
                        reached = true;
                        break;
                    }
                }
                if (reached) {
                    break;
                }
            }
        }

        private List<ObjectSectorCell> headerCells;
        /// <summary>
        /// 清除文件头的碎片表示;
        /// </summary>
        public void ClearHeaderCells() {
            headerCells.ForEach(p => p.CellState &= ~CellStatement.Head);
            headerCells.Clear();
        }
        /// <summary>
        /// 增加文件头的碎片表示;
        /// </summary>
        /// <param name="fileFragment"></param>
        public void AddHeaderFragment(FileFragment fileFragment) {
            ulong startAddress = fileFragment.StartAddress;
            bool reached = false;
            foreach (var row in Rows) {
                foreach (var cell in row.Cells) {
                    if (startAddress / (ulong)SectorSize >= cell.IniSector && startAddress / (ulong)SectorSize < cell.EndSector) {
                        cell.CellFragments.Add(new CellFragment { Fragment = fileFragment, FragmentStatement = CellStatement.Head });
                        cell.CellState |= CellStatement.Head;

                        reached = true;
                        break;
                    }
                }
                if (reached) {
                    break;
                }
            }
        }
        #endregion

        private void ClearMethod() {
            IniRows(Rows);
            IniRows(DetailRows);
        }
        //清除指定行的状态;
        private void IniRows(List<Row> rows) {
            rows.ForEach(p => {
                foreach (var q in p.Cells) {
                    q.EndSector = 0;
                    q.CellFragments.Clear();
                    q.CellState = CellStatement.None;
                    q.IniSector = 0;
                    q.Height = 10;
                    q.Width = 10;
                }
            });
        }

        public ObservableCollection<SectorItemCell> DetailSectorCells { get; set; }

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

        private int primaryRowNum;
        private int primaryColNum;
        public int SectorSize { get; set; }
        public void GetScale(ulong sizeInSector, int sectorSize) {
            int primaryCellNum = primaryRowNum * primaryColNum;
            this.SectorSize = sectorSize;
            if (sizeInSector % ((ulong)Rows.Count) == 0) {
                BlockSize = Convert.ToInt32(sizeInSector / (ulong)primaryCellNum);
            }
            else {
                try { 
                    BlockSize = Convert.ToInt32(
                            (sizeInSector / ((ulong)primaryCellNum) * (ulong)primaryCellNum + (ulong)primaryCellNum) / (ulong)primaryCellNum
                        );
                }
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("FragmentAnalyzerWindowViewModel->GetScale错误:" + ex.Message);
                }
            }
            for (int row = 0; row < Rows.Count; row++) {
                for (int col = 0; col < Rows[row].Cells.Count; col++) {
                    Rows[row].Cells[col].IniSector = (ulong)blockSize * ((ulong)primaryColNum * (ulong)row + (ulong)col);
                    Rows[row].Cells[col].EndSector = (ulong)blockSize * ((ulong)primaryColNum * (ulong)row + (ulong)(col + 1)) - 1;
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
                    HiddenAct?.Invoke();
                }
                else {
                    ShowAct?.Invoke();
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
                isLoading = true;
                NotifyPropertyChanging(nameof(IsLoading));
            }
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
                            if (isEnabled) {
                                Visible = Visibility.Hidden;
                                e.Cancel = true;
                            }
                        }
                    ));
            }
        }
    }
   
    
}
