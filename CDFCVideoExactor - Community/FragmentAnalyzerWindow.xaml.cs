using CDFCConverter.Enums;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for FragmentAnalyzerWindow.xaml
    /// </summary>
    public partial class FragmentAnalyzerWindow : MetroWindow {
        private FragmentAnalyzerWindowViewModel vm;
        private const int rowCount = 54;
        private const int colCount = 140;
        private const int objectRowCount = 25;
        private const int objectColCount = 140;
        public FragmentAnalyzerWindow(FragmentAnalyzerWindowViewModel vm) {
            InitializeComponent();
            SetCommandBindings();
            this.vm = vm;
            vm.ShowAct = this.Show;
            vm.HiddenAct = this.Hide;
            this.DataContext = vm;
        }
        /// <summary>
        /// 鼠标左击碎片的响应;(一级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemsControl_MouseLeftButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            Rectangle rect = e.OriginalSource as Rectangle;
            //清除第三级的碎片节点显示;
            vm.DetailSectorCells.Clear();
            //清除第二级的放大节点显示;
            vm.SeniorSelectedCell = null;
            if (rect != null) {
                ulong seniorCellNum = objectColCount * objectRowCount;
                ObjectSectorCell cell = rect.DataContext as ObjectSectorCell;

                ulong objectSize = cell.EndSector - cell.IniSector;
                int blockSize = Convert.ToInt32((objectSize / (seniorCellNum) * (seniorCellNum) + seniorCellNum) / seniorCellNum);

                List<CellFragment> fragments = cell.CellFragments;
                //线程池调用防止卡顿,设定多级视图的状态;
                ThreadPool.QueueUserWorkItem(callBack => {
                    //设定二级列表的碎片存在分布状况;
                    for (int row = 0; row < objectRowCount; row++) {
                        for (int col = 0; col < objectColCount; col++) {
                            ObjectSectorCell newCell = vm.DetailRows[row].Cells[col];
                            newCell.CellFragments.Clear();
                            newCell.IniSector = cell.IniSector + (ulong)((row * objectColCount + col) * blockSize);
                            newCell.EndSector = cell.IniSector + (ulong)((row * objectColCount + col + 1) * blockSize);
                            var objectFragments = fragments.Where(p => {
                                return !((p.Fragment.StartAddress / (ulong)vm.SectorSize) > newCell.EndSector ||
                                (p.Fragment.StartAddress + p.Fragment.Size) / (ulong)vm.SectorSize < newCell.IniSector);
                            });
                            if (objectFragments.Count() != 0) {
                                //判断该段显示状态;
                                //若仅存在头;
                                //if (objectFragments.All(p => p.FragmentStatement == CellStatement.Head)) {
                                //    newCell.CellState = CellStatement.Head;
                                //    newCell.OriCellState = CellStatement.Head;
                                //}
                                ////若仅存在尾;
                                //else if (objectFragments.All(p => p.FragmentStatement == CellStatement.Tile)) {
                                //    newCell.CellState = CellStatement.Tile;
                                //    newCell.OriCellState = CellStatement.Tile;
                                //}
                                ////若仅存在选中;
                                //else if (objectFragments.All(p => p.FragmentStatement == CellStatement.Chosen)) {
                                //    newCell.CellState = CellStatement.Chosen;
                                //}
                                ////否则将只显示文件存在;
                                //else {

                                newCell.CellState = CellStatement.None;
                                foreach (var frag in objectFragments) {
                                    newCell.CellState |= frag.FragmentStatement;
                                }
                                //}
                                if ((newCell.CellState & CellStatement.Chosen) != 0) {
                                    vm.AddSeniorChosenCell(newCell);
                                }
                                newCell.CellFragments.AddRange(objectFragments);
                            }
                            else {
                                newCell.CellState = CellStatement.None;
                            }
                            //                        rowVal.Cells.Add(newCell);
                        }
                        //                 DetailRows.Add(rowVal);
                    }
                    //若用户选定了截取范围(MP4)
                    if (vm.IsRanged) {
                        vm.RefreshSelectedRange();
                    }
                });

                vm.PrimarySelectedCell = cell;
            }
            e.Handled = true;
        }
        /// <summary>
        /// 鼠标其他动作点击单元时的响应;(一级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            //若是双击;
            if (e.ClickCount == 2) {
                Rectangle rect = sender as Rectangle;
                 if (rect != null) {
                    ulong seniorCellNum = objectColCount * objectRowCount;
                    ObjectSectorCell cell = rect.DataContext as ObjectSectorCell;
                    ulong objectSize = cell.EndSector - cell.IniSector;
                    int blockSize = Convert.ToInt32((objectSize / (seniorCellNum) * (seniorCellNum) + seniorCellNum) / seniorCellNum);
                    List<CellFragment> fragments = cell.CellFragments;

                    if (fragments != null && fragments.Count != 0) {
                        FragmentsViewerWindowViewModel fragViewerWindowModel = new FragmentsViewerWindowViewModel(vm.MainWindowViewModel) {
                            CellFragments = new System.Collections.ObjectModel.ObservableCollection<CellFragment>(fragments)
                        };
                        FragmentsViewerWindow fragmentViewerWindow = new FragmentsViewerWindow(fragViewerWindowModel);
                        fragmentViewerWindow.ShowDialog();
                    }
                }

                e.Handled = true;
            }

        }

        /// <summary>
        /// 鼠标单击二级碎片的响应;(二级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void midItem_MouseLeftButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            var rect = e.OriginalSource as Rectangle;
            if (rect != null) {
                var cell = rect.DataContext as ObjectSectorCell;
                
                //在视图模型中设定选定节点;

                //清除第三级节点显示;
                vm.DetailSectorCells.Clear();
                //设定三级视图的节点显示;
                ThreadPool.QueueUserWorkItem(callBack => {
                    if (cell != null) {
                        int cellCount = Convert.ToInt32(cell.EndSector - cell.IniSector);
                        for (int index = 0; index < cellCount; index++) {
                            SectorItemCell sCell = new SectorItemCell();
                            sCell.SectorAddress = cell.IniSector + (ulong)index;
                            var cellFragment = cell.CellFragments.FirstOrDefault(p => 
                            p.Fragment.StartAddress <= sCell.SectorAddress * (ulong)vm.SectorSize
                            &&p.Fragment.StartAddress + p.Fragment.Size >=( sCell.SectorAddress+1)*(ulong)vm.SectorSize);
                            if (cellFragment != null) {
                                sCell.CellState = cellFragment.FragmentStatement;
                                sCell.CellFragment = cellFragment;
                            }
                            this.Dispatcher.Invoke(() => {
                                vm.DetailSectorCells.Add(sCell);
                                if (vm.IsRanged) {
                                    vm.RefreshSelectedRange();
                                }
                            });
                        }
                    }

                });
                //设定二级视图选中扇区单元的大小;
                ThreadPool.QueueUserWorkItem(callBack => {
                    vm.SeniorSelectedCell = cell;
                });
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// 鼠标右击扇区碎片时的响应;(三级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailItemsControl_MouseRightButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            var rect = e.OriginalSource as Rectangle;
            if (rect != null) {
                vm.SelectedSector = rect.DataContext as SectorItemCell;
                if (vm.SelectedSector.CellFragment != null) {
                    FragmentsAnalyzerCommands.SetAsBorderNodeCommand.CanExecute(this, rect);
                }

            }

            //e.Handled = true;
        }

        private void FragmentAnalyzerWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                if (!Convert.ToBoolean(e.NewValue)) {
                    this.Close();
                }
            }
            catch(Exception ex) {
                EventLogger.Logger.WriteLine("FragmentAnalyzerWindow->FragmentAnalyzerWindow_IsEnabledChanged出错" + ex.Message);
            }
        }

        //private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        //    if(vm != null && vm.IsEnabled) {
                
        //        vm.Visible = Visibility.Hidden;
        //        e.Cancel = true;
        //    }
        //}
    }
    /// <summary>
    /// 设定命令绑定项;
    /// </summary>
    public partial class FragmentAnalyzerWindow {
        private void SetCommandBindings() {
            //置为临界节点的命令绑定;
            var miSetAsBorderNodeCommandBinding = new CommandBinding(
                FragmentsAnalyzerCommands.SetAsBorderNodeCommand,
                MiSetAsBorderNodeCommandBinding_Executed,
                MiSetAsBorderNodeCommandBinding_CanExecute
                );
            this.CommandBindings.Add(miSetAsBorderNodeCommandBinding);
        }

        private void MiSetAsBorderNodeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e) {
            vm.ViceBorderCell = vm.BorderCell;
            vm.BorderCell = vm.SelectedSector;
            if (vm.ViceBorderCell != null) {
                vm.SetSelectedRange();
            }
            e.Handled = true;
        }

        private void MiSetAsBorderNodeCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
            if (CDFCSetting.ScanSetting.IsMP4Class && vm.SelectedSector != null && vm.SelectedSector.CellFragment != null) {
                if ((vm.SelectedSector.CellFragment.FragmentStatement & CellStatement.Head) != 0
                    || (vm.SelectedSector.CellFragment.FragmentStatement & CellStatement.Tile) != 0) {
                    if (vm.BorderCell == null) {
                        e.CanExecute = true;
                    }
                    else if (vm.SelectedSector.SectorAddress != vm.BorderCell.SectorAddress) {
                        if (vm.ViceBorderCell == null) {
                            e.CanExecute = true;
                        }
                        else if (vm.SelectedSector.SectorAddress != vm.ViceBorderCell.SectorAddress) {
                            e.CanExecute = true;
                        }
                    }
                }
            }

            e.Handled = true;
        }
    }

}
