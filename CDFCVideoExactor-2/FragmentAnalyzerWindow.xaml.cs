using CDFCConverter.Enums;
using CDFCEntities.Interfaces;
using CDFCVideoExactor.Commands;
using CDFCVideoExactor.Models;
using CDFCVideoExactor.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CDFCVideoExactor {
    /// <summary>
    /// Interaction logic for FragmentAnalyzerWindow.xaml
    /// </summary>
    public partial class FragmentAnalyzerWindow : MetroWindow {
        private FragmentAnalyzerWindowViewModel vm;
        public FragmentAnalyzerWindow() {
            InitializeComponent();
            vm = new FragmentAnalyzerWindowViewModel(new MainWindowViewModel(this.Dispatcher));
            this.DataContext = vm;
            vm.InitializingChanged += Vm_InitializingChanged;
            
        }

        private void Vm_InitializingChanged(object sender, bool e) {
            if (e) {
                this.Cursor = Cursors.Wait;
            }
            else {
                this.Cursor = null;
            }
        }

        public FragmentAnalyzerWindow(FragmentAnalyzerWindowViewModel vm,IObjectDevice objectDevice) {
            InitializeComponent();
            this.vm = vm;
            vm.ShowAct = this.Show;
            vm.HiddenAct = this.Hide;
            this.DataContext = vm;
            this.ObjectDevice = objectDevice;
        }

        public IObjectDevice ObjectDevice { get; private set; }
        /// <summary>
        /// 鼠标左击碎片的响应;(一级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PrimaryItemsControl_MouseLeftButtonDown(object sender, RoutedEventArgs e) {
            Border bd = e.OriginalSource as Border;
            if(bd != null) {
                ObjectSectorCell cell = bd.DataContext as ObjectSectorCell;
                if (cell != null && vm != null) {
                    vm.SetSeniorRange(cell);
                }
                vm.PrimaryTickedCell = cell;
            }
            e.Handled = true;
        }
        /// <summary>
        /// 鼠标其他动作点击单元时的响应;(一级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrimaryItemsControl_MouseDown(object sender, MouseButtonEventArgs e) {
            //若是双击;
            if (e.ClickCount == 2) {
                var bd = e.OriginalSource as Border;
                 if (bd != null) {
                    ObjectSectorCell cell = bd.DataContext as ObjectSectorCell;
                    if(cell == null) {
                        return;
                    }
                    if(vm != null) {
                        vm.ShowCellFragments(cell);
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
        private void SeniorItemsControl_MouseLeftButtonDown(object sender, System.Windows.RoutedEventArgs e) {
            var bd = e.OriginalSource as Border;
            if (bd != null) {
                var cell = bd.DataContext as ObjectSectorCell;
                if(cell == null) {
                    return;
                }
                if(vm != null) {
                    vm.ShowCellSectors(cell);
                }
            }
            
            e.Handled = true;
        }

        /// <summary>
        /// 鼠标右击扇区碎片时的响应;(三级视图)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailItemsControl_MouseDown(object sender, MouseButtonEventArgs e) {
            var rect = e.OriginalSource as Rectangle;
            if (rect != null) {
                vm.SelectedSector = rect.DataContext as SectorItemCell;
                if (vm.SelectedSector.CellFragment != null) {
                    FragmentsAnalyzerCommands.SetAsBorderNodeCommand.CanExecute(this, rect);
                }
            }
            e.Handled = true;
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

        private void Button_Click(object sender, RoutedEventArgs e) {
            ThreadPool.QueueUserWorkItem(callBack => {
                vm.InitCells(6442450944); 
            });
            
        }
        
    }
    

}
