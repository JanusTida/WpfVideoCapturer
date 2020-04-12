﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFHexaEditor.Control.Abstracts;
using WPFHexaEditor.Control.IO;
using WPFHexaEditor.Core;
using WPFHexaEditor.Core.Bytes;
using WPFHexaEditor.Core.MethodExtention;
using WPFHexaEditor.Core.ROMTable;

namespace WPFHexaEditor.Control {
    /// <summary>
    /// 2016 - Derek Tremblay (derektremblay666@gmail.com)
    /// WPF Hexadecimal editor control
    /// </summary>
    public partial class HexaEditor : UserControl
    {
        private const double _lineInfoHeight = 22;
        private ByteProvider _provider = null;
        private double _scrollLargeChange = 100;
        private List<long> _markedPositionList = new List<long>();
        private long _rightClickBytePosition = -1;
        private TBLStream _TBLCharacterTable = null;
        
        //Event
        public event EventHandler SelectionStartChanged;
        public event EventHandler SelectionStopChanged;
        public event EventHandler SelectionLengthChanged;
        public event EventHandler DataCopied;
        public void OpenHandleStream(ObjectDeviceStream stream) {
            if (stream.CanRead) {
                Close();
                _provider = new ByteProvider(stream);
                _provider.ReadOnlyChanged += Provider_ReadOnlyChanged;
                _provider.DataCopiedToClipboard += Provider_DataCopied;
                _provider.ChangesSubmited += ProviderStream_ChangesSubmited;
                _provider.LongProcessProgressChanged += Provider_LongProcessProgressChanged;
                _provider.LongProcessProgressStarted += Provider_LongProcessProgressStarted;
                _provider.LongProcessProgressCompleted += Provider_LongProcessProgressCompleted;

                UpdateVerticalScroll();
                UpdateHexHeader();

                RefreshView(true);

                UnSelectAll();

                UpdateSelectionColorMode(FirstColor.HexByteData);
            }
            else {
                throw new Exception("Can't read MemoryStream");
            }
        }
        public HexaEditor()
        {
            InitializeComponent();
            
            RefreshView(true);

            UpdateHexHeader();

            StatusBarGrid.DataContext = this;
            
            this.DataContextChanged += HexaEditor_DataContextChanged;

        }
        
        private HexEditorViewModel vm;
        private void HexaEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var oldVm = e.OldValue as HexEditorViewModel;
            var newVm = e.NewValue as HexEditorViewModel;
            if (oldVm != null) {
                oldVm.FindNextRequired -= FindNextString;
                //oldVm.SubmitChangesRequired -= SubmitChanges;
            }
            if (newVm != null) {
                newVm.FindNextRequired += FindNextString;
                //newVm.SubmitChangesRequired += SubmitChanges;
            }
            vm = newVm;
        }
        
        private void FindNextString(object sender, string text) {
            FindNext(text);
        }

        private void SubmitChanges(object sender,object para) {
            SubmitChanges();
        }

        #region Miscellaneous property/methods
        public double ScrollLargeChange {
            get
            {
                return _scrollLargeChange;
            }
            set
            {
                _scrollLargeChange = value;

                UpdateVerticalScroll();
            }
        }

        #endregion Miscellaneous property/methods

        #region Characters tables property/methods
        /// <summary>
        /// Type of caracter table are used un hexacontrol. 
        /// For now, somes character table can be readonly but will change in future
        /// </summary>
        public  CharacterTable TypeOfCharacterTable
        {
            get { return ( CharacterTable)GetValue(TypeOfCharacterTableProperty); }
            set { SetValue(TypeOfCharacterTableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TypeOfCharacterTable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeOfCharacterTableProperty =
            DependencyProperty.Register("TypeOfCharacterTable", typeof( CharacterTable), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(CharacterTable.ASCII,
                    new PropertyChangedCallback(TypeOfCharacterTable_PropertyChanged)));

        private static void TypeOfCharacterTable_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            ctrl.RefreshView();              
        }

        /// <summary>
        /// Load TBL Character table file in control. (Used for ROM reverse engineering)
        /// Change CharacterTable property for use.
        /// </summary>
        public void LoadTBLFile(string fileName)
        {
            if (File.Exists(fileName)) {
                _TBLCharacterTable = new TBLStream();
                _TBLCharacterTable.Load(fileName);

                TBLLabel.Visibility = Visibility.Visible;
                TBLLabel.ToolTip = $"TBL file : {fileName}";

                RefreshView();
            }
        }
        #endregion Characters tables

        #region ReadOnly property/event
        /// <summary>
        /// Put the control on readonly mode.
        /// </summary>
        public bool ReadOnlyMode
        {
            get { return (bool)GetValue(ReadOnlyModeProperty); }
            set { SetValue(ReadOnlyModeProperty, value); }
        }

        public static readonly DependencyProperty ReadOnlyModeProperty =
            DependencyProperty.Register("ReadOnlyMode", typeof(bool), typeof(HexaEditor),
                new FrameworkPropertyMetadata(false, 
                    new PropertyChangedCallback(ReadOnlyMode_PropertyChanged)));

        private static void ReadOnlyMode_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.RefreshView(false);
                
                //TODO: ADD VISIBILITY CONVERTER FOR BINDING READONLY PROPERTY
                if (ctrl.ReadOnlyMode)
                    ctrl.ReadOnlyLabel.Visibility = Visibility.Visible;
                else
                    ctrl.ReadOnlyLabel.Visibility = Visibility.Collapsed;
            }

        }
        
        private void Provider_ReadOnlyChanged(object sender, EventArgs e)
        {
            if (ByteProvider.CheckIsOpen(_provider))
                ReadOnlyMode = _provider.ReadOnlyMode;
        }
        #endregion ReadOnly property/event
        
        #region Add modify delete bytes methods/event
        private void Control_ByteModified(object sender, EventArgs e)
        {
            HexByteControl ctrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                _provider.AddByteModified(sbCtrl.Byte, sbCtrl.BytePositionInFile);
                SetScrollMarker(sbCtrl.BytePositionInFile, ScrollMarker.ByteModified);
            }
            else if (ctrl != null)
            {
                _provider.AddByteModified(ctrl.Byte, ctrl.BytePositionInFile);
                SetScrollMarker(ctrl.BytePositionInFile, ScrollMarker.ByteModified);
            }

            UpdateStatusBar();
        }
                                
        /// <summary>
        /// Delete selection, add scroll marker and update control
        /// </summary>
        public void DeleteSelection()
        {
            if (!CanDelete()) return;

            if (ByteProvider.CheckIsOpen(_provider))
            {
                long position = -1;

                if (SelectionStart > SelectionStop)
                    position = SelectionStop;
                else
                    position = SelectionStart;

                _provider.AddByteDeleted(position, SelectionLength);

                SetScrollMarker(position, ScrollMarker.ByteDeleted);

                //UpdateByteModified();
                UpdateSelection();
                UpdateStatusBar();
            }
        }
        #endregion Add modify delete bytes methods/event

        #region Lines methods
        /// <summary>
        /// Obtain the max line for verticalscrollbar
        /// </summary>
        public long GetMaxLine()
        {
            if (ByteProvider.CheckIsOpen(_provider))
                return _provider.Length / BytePerLine;
            else
                return 0;
        }

        /// <summary>
        /// Get the number of row visible in control 
        /// </summary>
        public long GetMaxVisibleLine()
        {
            return (long)(StringDataStackPanel.ActualHeight / _lineInfoHeight); // + 1; //TEST
        }

        #endregion Lines methods

        #region SectorSize/SectorLines Property/Method
        public static readonly DependencyProperty SectorSizeProperty =
            DependencyProperty.Register("SectorSize", typeof(int), typeof(HexaEditor),
                new PropertyMetadata(512,SectorSize_PropertyChanged));

        public int SectorSize {
            get {
                return (int)GetValue(SectorSizeProperty);
            }
            set {
                SetValue(SectorSizeProperty, value);
            }
        }
        public static void SectorSize_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var hexEditor = d as HexaEditor;
            if(hexEditor != null) {
                hexEditor.UpdateSectorLines();
            }
        }
        private void UpdateSectorLines() {
            SectorLineContainer.Children.Clear();

            var sectorHeight = (int) _lineInfoHeight * SectorSize / BytePerLine;
            int linesCount =  (int)this.ActualHeight / sectorHeight + 1;
            for (int i = 0; i < linesCount; i++) {
                var bd = new Border();
                bd.Height = 1;
                bd.Background = new SolidColorBrush(Colors.Gainsboro);
                bd.Margin = new Thickness(0, sectorHeight * i, 0, 0);
                bd.VerticalAlignment = VerticalAlignment.Top;
                SectorLineContainer.Children.Add(bd);
            }
            var firstPosition = GetFirstVisibleBytePosition();
            firstPosition = firstPosition == -1 ? 0 : firstPosition;
            var firstLine = firstPosition / BytePerLine % ( SectorSize / BytePerLine);
            SectorLineContainer.Margin = new Thickness(0,_lineInfoHeight * ( SectorSize / BytePerLine - firstLine),0,0);
        }

        #endregion

        #region Selection Property/Methods/Event
        /// <summary>
        /// Get the selected line of focus control
        /// </summary>
        public long SelectionLine
        {
            get { return (long)GetValue(SelectionLineProperty); }
            internal set { SetValue(SelectionLineProperty, value); }
        }

        public static readonly DependencyProperty SelectionLineProperty =
            DependencyProperty.Register("SelectionLine", typeof(long), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(0L));
                
        private void LineInfoLabel_MouseMove(object sender, MouseEventArgs e)
        {
            Label line = sender as Label;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                selectionStop = ByteConverters.DecimalLiteralToLong(line.Content.ToString()) + BytePerLine - 1;
            }
        }

        /// <summary>
        /// Update the dataviewer stackpanel and strin dataviewer stackpanel.
        /// </summary>
        private void UpdateStringDataViewer(bool ControlResize) {
            if (ByteProvider.CheckIsOpen(_provider)) {
                if (ControlResize) {
                    StringDataStackPanel.Children.Clear();
                    foreach (Label infolabel in LinesInfoStackPanel.Children) {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;

                        long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < BytePerLine; i++) {

                            _provider.Position = position + i;

                            if (_provider.Position >= _provider.Length)
                                break;

                            StringByteControl sbCtrl = new StringByteControl();

                            sbCtrl.BytePositionInFile = _provider.Position;
                            sbCtrl.StringByteModified += Control_ByteModified;
                            sbCtrl.ReadOnlyMode = ReadOnlyMode;
                            sbCtrl.MoveNext += Control_MoveNext;
                            sbCtrl.MovePrevious += Control_MovePrevious;
                            sbCtrl.MouseSelection += Control_MouseSelection;
                            sbCtrl.Click += Control_Click;
                            sbCtrl.RightClick += Control_RightClick;
                            sbCtrl.MoveUp += Control_MoveUp;
                            sbCtrl.MoveDown += Control_MoveDown;
                            sbCtrl.MoveLeft += Control_MoveLeft;
                            sbCtrl.MoveRight += Control_MoveRight;
                            sbCtrl.ByteDeleted += Control_ByteDeleted;
                            sbCtrl.EscapeKey += Control_EscapeKey;

                            sbCtrl.InternalChange = true;
                            sbCtrl.TBLCharacterTable = _TBLCharacterTable;
                            sbCtrl.TypeOfCharacterTable = TypeOfCharacterTable;
                            sbCtrl.Byte = (byte)_provider.ReadByte();

                            sbCtrl.ByteNext = (byte)_provider.ReadByte();

                            _provider.Position--;

                            sbCtrl.InternalChange = false;

                            dataLineStack.Children.Add(sbCtrl);
                        }

                        StringDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else {
                    int stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children) {
                        long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                        if (StringDataStackPanel.Children.Count > 0) {
                            #region 
                            foreach (StringByteControl sbCtrl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children) {
                                _provider.Position = position++;

                                sbCtrl.Action = ByteAction.Nothing;
                                sbCtrl.ReadOnlyMode = ReadOnlyMode;

                                sbCtrl.InternalChange = true;
                                sbCtrl.TBLCharacterTable = _TBLCharacterTable;
                                sbCtrl.TypeOfCharacterTable = TypeOfCharacterTable;
                                if (_provider.Position >= _provider.Length) {
                                    sbCtrl.Byte = null;
                                    sbCtrl.ByteNext = null;
                                    sbCtrl.BytePositionInFile = -1;
                                }
                                else {
                                    sbCtrl.Byte = (byte)_provider.ReadByte();
                                    sbCtrl.BytePositionInFile = _provider.Position - 1;

                                    sbCtrl.ByteNext = (byte)_provider.ReadByte();
                                    _provider.Position--;
                                }
                                sbCtrl.InternalChange = false;
                            }
                            #endregion
                        }


                        stackIndex++;

                        //Prevent index out off range exception when resize at EOF
                        if (stackIndex == HexDataStackPanel.Children.Count && VerticalScrollBar.Value == VerticalScrollBar.Maximum)
                            stackIndex--;
                    }
                }
            }
            else {
                StringDataStackPanel.Children.Clear();
            }
        }
        private void LineInfoLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label line = sender as Label;

            selectionStart = ByteConverters.DecimalLiteralToLong(line.Content.ToString());
            selectionStop = SelectionStart + BytePerLine - 1;
        }

        private void Control_MovePageDown(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long byteToMove = (BytePerLine * GetMaxVisibleLine());
            long test = SelectionStart + byteToMove;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test < _provider.Length)
                    SelectionStart += byteToMove;
                else
                    selectionStart = _provider.Length;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    selectionStart = SelectionStop;
                else
                    selectionStop = SelectionStart;

                if (test < _provider.Length)
                {
                    SelectionStart += byteToMove;
                    SelectionStop += byteToMove;
                }
            }

            if (SelectionStart > GetLastVisibleBytePosition())
                VerticalScrollBar.Value++;

            if (hbCtrl != null || sbCtrl != null)
            {
                VerticalScrollBar.Value += GetMaxVisibleLine() - 1;
                SetFocusHexDataPanel(SelectionStart);
            }
        }

        private void Control_ByteDeleted(object sender, EventArgs e)
        {
            HexByteControl ctrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            DeleteSelection();
        }

        private void Control_EscapeKey(object sender, EventArgs e)
        {
            UnSelectAll();
            UnHighLightAll();
        }

        private void Control_MovePageUp(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long byteToMove = (BytePerLine * GetMaxVisibleLine());
            long test = SelectionStart - byteToMove;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                    SelectionStart -= byteToMove;
                else
                    selectionStart = 0;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    selectionStart = SelectionStop;
                else
                    selectionStop = SelectionStart;

                if (test > -1)
                {
                    SelectionStart -= byteToMove;
                    SelectionStop -= byteToMove;
                }
            }

            if (SelectionStart < GetFirstVisibleBytePosition())
                VerticalScrollBar.Value--;

            if (hbCtrl != null || sbCtrl != null)
            {
                VerticalScrollBar.Value -= GetMaxVisibleLine() - 1;
                SetFocusHexDataPanel(SelectionStart);
            }                
        }

        private void Control_MoveDown(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart + BytePerLine;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test < _provider.Length)
                    SelectionStart += BytePerLine;
                else
                    selectionStart = _provider.Length;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    selectionStart = SelectionStop;
                else
                    selectionStop = SelectionStart;

                if (test < _provider.Length)
                {
                    SelectionStart += BytePerLine;
                    SelectionStop += BytePerLine;
                }
            }

            if (SelectionStart > GetLastVisibleBytePosition())
                VerticalScrollBar.Value++;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }
        
        private void Control_MoveUp(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart - BytePerLine;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                    SelectionStart -= BytePerLine;
                else
                    selectionStart = 0;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    selectionStart = SelectionStop;
                else
                    selectionStop = SelectionStart;

                if (test > -1)
                {
                    SelectionStart -= BytePerLine;
                    SelectionStop -= BytePerLine;
                }
            }

            if (SelectionStart < GetFirstVisibleBytePosition())
                VerticalScrollBar.Value--;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }

        private void Control_Click(object sender, EventArgs e)
        {
            StringByteControl sbCtrl = sender as StringByteControl;
            HexByteControl ctrl = sender as HexByteControl;

            if (ctrl != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    selectionStop = ctrl.BytePositionInFile;
                }
                else
                {
                    focusPosition = ctrl.BytePositionInFile;
                }
                UpdateSelectionColorMode(FirstColor.HexByteData);
                UpdateFocusState();
            }

            if (sbCtrl != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    selectionStop = sbCtrl.BytePositionInFile;
                }
                else
                {
                    focusPosition = sbCtrl.BytePositionInFile; 
                }

                UpdateSelectionColorMode(FirstColor.StringByteData);
                UpdateFocusState();
            }
        }
        
        private void Control_MouseSelection(object sender, EventArgs e)
        {
            //Prevent false mouse selection on file open
            if (SelectionStart == -1 && focusPosition == -1)
                return;

            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (hbCtrl != null) {
                UpdateSelectionColorMode(FirstColor.HexByteData);

                if (hbCtrl.BytePositionInFile != -1) {
                    selectionStart = focusPosition;
                    selectionStop = hbCtrl.BytePositionInFile;
                }
                else {
                    selectionStart = focusPosition;
                    selectionStop = GetLastVisibleBytePosition();
                }
            }

            if (sbCtrl != null) {
                UpdateSelectionColorMode(FirstColor.StringByteData);

                if (sbCtrl.BytePositionInFile != -1) {
                    selectionStart = focusPosition;
                    selectionStop = sbCtrl.BytePositionInFile;
                }
                else {
                    selectionStart = focusPosition;
                    selectionStop = GetLastVisibleBytePosition();
                }
            }

            UpdateSelection();
        }
        
        private void BottomRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                VerticalScrollBar.Value += 5;            
        }

        private void TopRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                VerticalScrollBar.Value -= 5;            
        }

        /// <summary>
        /// Un highlight all byte as highlighted with find all methods
        /// </summary>
        public void UnHighLightAll()
        {
            _markedPositionList.Clear();
            UpdateHighLightByte();
            ClearScrollMarker(ScrollMarker.SearchHighLight);
        }

        /// <summary>
        /// Set the start byte position of selection
        /// </summary>
        public long SelectionStart
        {
            get { return (long)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }

        //Add the private property in case that setting the value to the DependencyProperty makes the Binding useless.
        //The same to "SelectionStop".
        private long selectionStart {
            get {
                return SelectionStart;
            }
            set {
                SetCurrentValue(SelectionStartProperty, value);
            }
        }

        private long _focusPosition = -1;
        private long focusPosition {
            get {
                return _focusPosition;
            }
            set {
                _focusPosition = value;
                vm?.NotifyPosition(value);
            }
        }

        
        public static readonly DependencyProperty SelectionStartProperty =
            DependencyProperty.Register("SelectionStart", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStart_ChangedCallBack),
                    new CoerceValueCallback(SelectionStart_CoerceValueCallBack)));

        //public static readonly DependencyProperty SelectionStartProperty =
        //    DependencyProperty.Register("SelectionStart", typeof(long), typeof(HexaEditor),
        //        new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStart_ChangedCallBack)));

        // Using a DependencyProperty as the backing store for Stream.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty StreamProperty =
        //    DependencyProperty.Register("Stream", typeof(Stream), typeof(HexaEditor),
        //        new FrameworkPropertyMetadata(null,
        //            new PropertyChangedCallback(Stream_PropertyChanged)));


        private static object SelectionStart_CoerceValueCallBack(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            long value = (long)baseValue;

            if (value < -1)
                return -1L;

            if (!ByteProvider.CheckIsOpen(ctrl._provider))
                return -1L;
            else
                return baseValue;
        }

        private static void SelectionStart_ChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateSelection();
                ctrl.UpdateSelectionLine();
                ctrl.SetScrollMarker(0, ScrollMarker.SelectionStart);

                ctrl.SelectionStartChanged?.Invoke(ctrl, new EventArgs());

                ctrl.SelectionLengthChanged?.Invoke(ctrl, new EventArgs());
            }
        }
              

        /// <summary>
        /// Set the start byte position of selection
        /// </summary>
        public long SelectionStop
        {
            get { return (long)GetValue(SelectionStopProperty); }
            set { SetValue(SelectionStopProperty, value); }
        }
        
        private long selectionStop {
            get {
                return SelectionStop;
            }
            set {
                SetCurrentValue(SelectionStopProperty, value);
            }
        }

        public static readonly DependencyProperty SelectionStopProperty =
            DependencyProperty.Register("SelectionStop", typeof(long), typeof(HexaEditor),
                new FrameworkPropertyMetadata(-1L, new PropertyChangedCallback(SelectionStop_ChangedCallBack),
                    new CoerceValueCallback(SelectionStop_CoerceValueCallBack)));

        private static object SelectionStop_CoerceValueCallBack(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            long value = (long)baseValue;

            //Debug.Print($"SelectionStop : {value.ToString()}");

            if (value < -1)
                return -1L;

            if (!ByteProvider.CheckIsOpen(ctrl._provider))
                return -1L;

            if (value >= ctrl._provider.Length)
                return ctrl._provider.Length;
            
            return baseValue;            
        }

        private static void SelectionStop_ChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.UpdateSelection();
                ctrl.UpdateSelectionLine();

                ctrl.SelectionStopChanged?.Invoke(ctrl, new EventArgs());

                ctrl.SelectionLengthChanged?.Invoke(ctrl, new EventArgs());
            }
        }

        /// <summary>
        /// Reset selection to -1
        /// </summary>
        public void UnSelectAll()
        {
            selectionStart = -1;
            selectionStop = -1;
        }

        /// <summary>
        /// Select the entire file
        /// If file are closed the selection will be set to -1
        /// </summary>
        public void SelectAll()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                selectionStart = 0;
                selectionStop = _provider.Length;
            }
            else
            {
                selectionStart = -1;
                selectionStop = -1;
            }

            UpdateSelection();
        }
                

        /// <summary>
        /// Get the lenght of byte are selected (base 1)
        /// </summary>
        public long SelectionLength
        {
            get
            {
                if (SelectionStop == -1 || SelectionStop == -1)
                    return 0;
                else if (SelectionStart == SelectionStop)
                    return 1;
                else if (SelectionStart > SelectionStop)
                    return SelectionStart - SelectionStop + 1;
                else
                    return SelectionStop - SelectionStart + 1;
            }
        }

        /// <summary>
        /// Get byte array from current selection
        /// </summary>
        public byte[] SelectionByteArray
        {
            get
            {
                MemoryStream ms = new MemoryStream();

                CopyToStream(ms, true);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get string from current selection
        /// </summary>
        public string SelectionString
        {
            get
            {
                MemoryStream ms = new MemoryStream();

                CopyToStream(ms, true);

                return ByteConverters.BytesToString(ms.ToArray());
            }
        }

        /// <summary>
        /// Get Hexadecimal from current selection
        /// </summary>
        public string SelectionHexa
        {
            get
            {
                MemoryStream ms = new MemoryStream();

                CopyToStream(ms, true);

                return ByteConverters.ByteToHex(ms.ToArray());
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) //UP            
                VerticalScrollBar.Value--;
            
            if (e.Delta < 0) //Down            
                VerticalScrollBar.Value++;
            
        }

        private void Control_MoveRight(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart + 1;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test <= _provider.Length)
                    SelectionStart++;
                else
                    selectionStart = _provider.Length;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    selectionStart = SelectionStop;
                else
                    selectionStop = SelectionStart;

                if (test < _provider.Length)
                {
                    SelectionStart++;
                    SelectionStop++;
                }
            }

            //Validation and refresh
            if (SelectionStart >= _provider.Length)
                selectionStart = _provider.Length;

            if (SelectionStart > GetLastVisibleBytePosition())
                VerticalScrollBar.Value++;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }

        private void Control_MoveLeft(object sender, EventArgs e)
        {
            HexByteControl hbCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            long test = SelectionStart - 1;

            //TODO : Validation
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (test > -1)
                    SelectionStart--;
                else
                    selectionStart = 0;
            }
            else
            {
                if (SelectionStart > SelectionStop)
                    selectionStart = SelectionStop;
                else
                    selectionStop = SelectionStart;

                if (test > -1)
                {
                    SelectionStart--;
                    SelectionStop--;
                }
            }

            //Validation and refresh
            if (SelectionStart < 0)
                selectionStart = 0;

            if (SelectionStart < GetFirstVisibleBytePosition())
                VerticalScrollBar.Value--;

            if (hbCtrl != null)
                SetFocusHexDataPanel(SelectionStart);

            if (sbCtrl != null)
                SetFocusStringDataPanel(SelectionStart);
        }


        private void Control_MovePrevious(object sender, EventArgs e)
        {
            HexByteControl hexByteCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                sbCtrl.IsSelected = false;
                SetFocusStringDataPanel(sbCtrl.BytePositionInFile - 1);
            }

            if (hexByteCtrl != null)
            {
                hexByteCtrl.IsSelected = false;
                SetFocusHexDataPanel(hexByteCtrl.BytePositionInFile - 1);
            }

            if (hexByteCtrl != null || sbCtrl != null)
            {
                SelectionStart--;
                SelectionStop--;
                //UpdateByteModified();
            }
        }

        private void Control_MoveNext(object sender, EventArgs e)
        {
            HexByteControl hexByteCtrl = sender as HexByteControl;
            StringByteControl sbCtrl = sender as StringByteControl;

            if (sbCtrl != null)
            {
                sbCtrl.IsSelected = false;
                SetFocusStringDataPanel(sbCtrl.BytePositionInFile + 1);
            }

            if (hexByteCtrl != null)
            {
                hexByteCtrl.IsSelected = false;
                SetFocusHexDataPanel(hexByteCtrl.BytePositionInFile + 1);
            }

            if (hexByteCtrl != null || sbCtrl != null)
            {
                SelectionStart++;
                SelectionStop++;
                //UpdateByteModified();
            }
        }
        #endregion Selection Property/Methods

        #region Copy/Paste/Cut Methods
        /// <summary>
        /// Return true if Copy method could be invoked.
        /// </summary>
        public bool CanCopy()
        {
            if (SelectionLength < 1 || !ByteProvider.CheckIsOpen(_provider))
                return false;
            
            return true;
        }

        /// <summary>
        /// Return true if delete method could be invoked.
        /// </summary>
        public bool CanDelete()
        {
            return CanCopy() && !ReadOnlyMode;
        }
        
        /// <summary>
        /// Copy to clipboard with default CopyPasteMode.ASCIIString
        /// </summary>
        public void CopyToClipboard()
        {            
            CopyToClipboard(CopyPasteMode.ASCIIString);
        }

        /// <summary>
        /// Copy to clipboard the current selection with actual change in control
        /// </summary>        
        public void CopyToClipboard(CopyPasteMode copypastemode)
        {
            CopyToClipboard(copypastemode, SelectionStart, SelectionStop, true);
        }

        /// <summary>
        /// Copy to clipboard
        /// </summary>        
        public void CopyToClipboard(CopyPasteMode copypastemode, long selectionStart, long selectionStop, bool copyChange)
        {
            if (!CanCopy()) return;

            if (ByteProvider.CheckIsOpen(_provider))
                _provider.CopyToClipboard(copypastemode, SelectionStart, SelectionStop, copyChange);
        }

        /// <summary>
        /// Copy selection to a stream
        /// </summary>      
        /// <param name="output">Output stream is not closed after copy</param>
        public void CopyToStream(Stream output, bool copyChange)
        {
            CopyToStream(output, SelectionStart, SelectionStop, copyChange);
        }

        /// <summary>
        /// Copy selection to a stream
        /// </summary>      
        /// <param name="output">Output stream is not closed after copy</param>
        public void CopyToStream(Stream output, long selectionStart, long selectionStop, bool copyChange)
        {
            if (!CanCopy()) return;

            if (ByteProvider.CheckIsOpen(_provider))
                _provider.CopyToStream(output, selectionStart, selectionStop, copyChange);
        }

        /// <summary>
        /// Occurs when data is copied in byteprovider instance
        /// </summary>
        private void Provider_DataCopied(object sender, EventArgs e)
        {
            DataCopied?.Invoke(sender, e);
        }

        #endregion Copy/Paste/Cut Methods

        #region Position Propertyies;
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(long), typeof(HexaEditor),
            new FrameworkPropertyMetadata(0L, new PropertyChangedCallback(Position_PropertyChanged), new CoerceValueCallback(Position_CoerceChanged)));

        private static void Position_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var hex = d as HexaEditor;
            hex?.SetPosition((long)e.NewValue);
        }

        private static object Position_CoerceChanged(DependencyObject d, object baseValue) {
            HexaEditor ctrl = d as HexaEditor;
            if (object.Equals(ctrl.Position, baseValue)) {
                ctrl?.SetPosition((long)baseValue);
            }
            return baseValue;
        }
        public long Position {
            get { return (long)this.GetValue(PositionProperty); }
            set {
                this.SetValue(PositionProperty, value);
            }
        }
        
        #endregion

        #region Set position methods

        /// <summary>
        /// Set position of cursor
        /// </summary>
        public void SetPosition(long position, long byteLength)
        {
            //TODO : selected hexbytecontrol
            if(byteLength != 0) {
                selectionStart = position;
                selectionStop = position + byteLength - 1;
            }
            if (ByteProvider.CheckIsOpen(_provider))            
                VerticalScrollBar.Value = GetLineNumber(position);            
            else
                VerticalScrollBar.Value = 0;

            if(byteLength == 0) {
                focusPosition = position;
                UpdateFocusState();
            }
            
            //RefreshView(true);
        }
        
        /// <summary>
        /// Get the line number of position in parameter
        /// </summary>
        public double GetLineNumber(long position)
        {
            return position / BytePerLine;
        }
        
        /// <summary>
        /// Set position in control at position in parameter
        /// </summary>
        public void SetPosition(long position)
        {
            SetPosition(position, 0);
        }
        
        /// <summary>
        /// Set position in control at position in parameter
        /// </summary>
        public void SetPosition(string HexLiteralPosition)
        {
            try
            {
                SetPosition(ByteConverters.DecimalLiteralToLong(HexLiteralPosition));
            }
            catch
            {
                throw new InvalidCastException("Invalid hexa string");
            }
        }

        /// <summary>
        /// Set position in control at position in parameter with specified selected lenght
        /// </summary>
        public void SetPosition(string HexLiteralPosition, long byteLenght)
        {
            try
            {
                SetPosition(ByteConverters.DecimalLiteralToLong(HexLiteralPosition), byteLenght);
            }
            catch
            {
                throw new InvalidCastException("Invalid hexa string");
            }
        }
        #endregion Set position methods

        #region Visibility property
        /// <summary>
        /// Set or Get value for change visibility of hexadecimal panel
        /// </summary>
        public Visibility HexDataVisibility
        {
            get { return (Visibility)GetValue(HexDataVisibilityProperty); }
            set { SetValue(HexDataVisibilityProperty, value); }
        }

        public static readonly DependencyProperty HexDataVisibilityProperty =
            DependencyProperty.Register("HexDataVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible, 
                    new PropertyChangedCallback(HexDataVisibility_PropertyChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static object Visibility_CoerceValue(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)baseValue;

            if (value == Visibility.Hidden)
                return Visibility.Collapsed;

            return value;
        }

        private static void HexDataVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.HexDataStackPanel.Visibility = Visibility.Visible;

                    if (ctrl.HeaderVisibility == Visibility.Visible)
                        ctrl.HexHeaderStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.HexDataStackPanel.Visibility = Visibility.Collapsed;
                    ctrl.HexHeaderStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of hexadecimal header
        /// </summary>
        public Visibility HeaderVisibility
        {
            get { return (Visibility)GetValue(HeaderVisibilityProperty); }
            set { SetValue(HeaderVisibilityProperty, value); }
        }
                
        public static readonly DependencyProperty HeaderVisibilityProperty =
            DependencyProperty.Register("HeaderVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(HeaderVisibility_PropertyChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));
                
        private static void HeaderVisibility_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    if (ctrl.HexDataVisibility == Visibility.Visible)
                        ctrl.HexHeaderStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:                    
                    ctrl.HexHeaderStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of string panel
        /// </summary>
        public Visibility StringDataVisibility
        {
            get { return (Visibility)GetValue(StringDataVisibilityProperty); }
            set { SetValue(StringDataVisibilityProperty, value); }
        }

        public static readonly DependencyProperty StringDataVisibilityProperty =
            DependencyProperty.Register("StringDataVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(StringDataVisibility_ValidateValue),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void StringDataVisibility_ValidateValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.StringDataStackPanel.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.StringDataStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Set or Get value for change visibility of vertical scroll bar
        /// </summary>
        public Visibility VerticalScrollBarVisibility
        {
            get { return (Visibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(VerticalScrollBarVisibility_ValueChanged),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void VerticalScrollBarVisibility_ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.VerticalScrollBar.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.VerticalScrollBar.Visibility = Visibility.Collapsed;
                    break;
            }
            
        }

        /// <summary>
        /// Set or Get value for change visibility of status bar
        /// </summary>
        public Visibility StatusBarVisibility
        {
            get { return (Visibility)GetValue(StatusBarVisibilityProperty); }
            set { SetValue(StatusBarVisibilityProperty, value); }
        }
                
        public static readonly DependencyProperty StatusBarVisibilityProperty =
            DependencyProperty.Register("StatusBarVisibility", typeof(Visibility), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(Visibility.Visible,
                    new PropertyChangedCallback(StatusBarVisibility_ValueChange),
                    new CoerceValueCallback(Visibility_CoerceValue)));

        private static void StatusBarVisibility_ValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;
            Visibility value = (Visibility)e.NewValue;

            switch (value)
            {
                case Visibility.Visible:
                    ctrl.StatusBarGrid.Visibility = Visibility.Visible;
                    break;
                case Visibility.Collapsed:
                    ctrl.StatusBarGrid.Visibility = Visibility.Collapsed;
                    break;
            }

            ctrl.RefreshView(false);
        }
        #endregion Visibility standard property

        #region Undo / Redo
        /// <summary>
        /// Clear undo and change
        /// </summary>
        public void ClearAllChange()
        {
            if (ByteProvider.CheckIsOpen(_provider))
                _provider.ClearUndoChange();
        }

        /// <summary>
        /// Make undo of last the last bytemodified
        /// </summary>
        public void Undo(int repeat = 1)
        {
            UnSelectAll();

            if (ByteProvider.CheckIsOpen(_provider))
            {
                for (int i = 0; i < repeat; i++)
                    _provider.Undo();
                               
                RefreshView(false, false);
            }
        }

        public long UndoCount
        {
            get
            {
                if (ByteProvider.CheckIsOpen(_provider))
                    return _provider.UndoCount;
                else
                    return 0;
            }
        }

        public List<ByteModified> UndosList
        {
            get
            {
                if (ByteProvider.CheckIsOpen(_provider))
                    return _provider.UndosList;
                else
                    return null;
            }
        }

        #endregion Undo / Redo

        #region Open, Close, Save... Methods/Property

        private void Provider_ChangesSubmited(object sender, EventArgs e)
        {
            //Refresh filename
            //var filename = FileName;
            Close();
            //FileName = filename;
        }

        private void ProviderStream_ChangesSubmited(object sender, EventArgs e)
        {
            //Refresh stream
            if (ByteProvider.CheckIsOpen(_provider))
            {
                MemoryStream stream = new MemoryStream();
                _provider.Stream.CopyTo(stream);
                Close();
                OpenStream(stream);
            }
        }

        /// <summary>
        /// Set or Get the file with the control will show hex
        /// </summary>
        //public string FileName
        //{
        //    get { return (string)GetValue(FileNameProperty); }
        //    set { SetValue(FileNameProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for FileName.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty FileNameProperty =
        //    DependencyProperty.Register("FileName", typeof(string), typeof(HexaEditor), 
        //        new FrameworkPropertyMetadata("", 
        //            new PropertyChangedCallback(FileName_PropertyChanged)));

        //private static void FileName_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    HexaEditor ctrl = d as HexaEditor;

        //    ctrl.Close();
        //    ctrl.OpenFile((string)e.NewValue);
        //}

        /// <summary>
        /// Set the MemoryStream are used by ByteProvider
        /// </summary>
        public Stream Stream
        {
            get { return (Stream)GetValue(StreamProperty); }
            set { SetValue(StreamProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stream.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StreamProperty =
            DependencyProperty.Register("Stream", typeof(Stream), typeof(HexaEditor),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(Stream_PropertyChanged)));

        private static void Stream_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            ctrl.Close();
            if(e.NewValue != null) {
                ctrl.OpenStream((Stream)e.NewValue);
            }
        }

        /// <summary>
        /// Close file and clear control
        /// ReadOnlyMode is reset to false
        /// </summary>
        public void Close()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                _provider.Close(false);
                
                ReadOnlyMode = false;
                VerticalScrollBar.Value = 0;
                //_TBLCharacterTable = null;
            }

            UnHighLightAll();
            ClearAllChange();
            ClearAllScrollMarker();
            UnSelectAll();
            RefreshView();
        }


        /// <summary>
        /// Save to the current stream
        /// TODO: Add save as another stream...
        /// </summary>
        public void SubmitChanges()
        {
            if (ByteProvider.CheckIsOpen(_provider))
                if (!_provider.ReadOnlyMode)
                    _provider.SubmitChanges();
        }

        /// <summary>
        /// Open file name
        /// </summary>
        /// <param name="filename"></param>
        //private void OpenFile(string filename)
        //{
        //    if (File.Exists(filename))
        //    {
        //        Close();

        //        _provider = new ByteProvider(filename);
        //        _provider.ReadOnlyChanged += Provider_ReadOnlyChanged;
        //        _provider.DataCopiedToClipboard += Provider_DataCopied;
        //        _provider.ChangesSubmited += Provider_ChangesSubmited;
        //        _provider.LongProcessProgressChanged += Provider_LongProcessProgressChanged;
        //        _provider.LongProcessProgressStarted += Provider_LongProcessProgressStarted;
        //        _provider.LongProcessProgressCompleted += Provider_LongProcessProgressCompleted;

        //        UpdateVerticalScroll();
        //        UpdateHexHeader();

        //        //Load file with ASCII character table;
        //        var previousTable = TypeOfCharacterTable;
        //        TypeOfCharacterTable = CharacterTable.ASCII;

        //        RefreshView(true);

        //        //Replace previous character table
        //        TypeOfCharacterTable = previousTable;



        //        UnSelectAll();

        //        UpdateSelectionColorMode(FirstColor.HexByteData);
        //    }
        //    else
        //    {
        //        throw new FileNotFoundException();
        //    }
        //}

        
        /// <summary>
        /// Open file name
        /// </summary>
        /// <param name="filename"></param>
        private void OpenStream(Stream stream)
        {
            if (stream != null && stream.CanRead)
            {
                Close();
                _provider = new ByteProvider(stream);
                _provider.ReadOnlyChanged += Provider_ReadOnlyChanged;
                _provider.DataCopiedToClipboard += Provider_DataCopied;
                _provider.ChangesSubmited += ProviderStream_ChangesSubmited;
                _provider.LongProcessProgressChanged += Provider_LongProcessProgressChanged;
                _provider.LongProcessProgressStarted += Provider_LongProcessProgressStarted;
                _provider.LongProcessProgressCompleted += Provider_LongProcessProgressCompleted;
                
                UpdateVerticalScroll();
                //UpdateHexHeader();

                RefreshView(true);

                UnSelectAll();

                UpdateSelectionColorMode(FirstColor.HexByteData);
            }
            else
            {
                throw new Exception("Can't read Stream");
            }
        }

        private void Provider_LongProcessProgressCompleted(object sender, EventArgs e)
        {
            LongProgressProgressBar.Visibility = Visibility.Collapsed;
            CancelLongProcessButton.Visibility = Visibility.Collapsed;
        }

        private void Provider_LongProcessProgressStarted(object sender, EventArgs e)
        {
            LongProgressProgressBar.Visibility = Visibility.Visible;
            CancelLongProcessButton.Visibility = Visibility.Visible;
        }

        private void Provider_LongProcessProgressChanged(object sender, EventArgs e)
        {
            //Update progress bar
            LongProgressProgressBar.Value = (double)sender;
            Application.Current.DoEvents();
        }


        private void CancelLongProcessButton_Click(object sender, RoutedEventArgs e)
        {            
            //TODO: Add messagebox confirmation

            if (ByteProvider.CheckIsOpen(_provider))
                _provider.IsOnLongProcess = false;
        }

        /// <summary>
        /// Check if byteprovider is on long progress and update control
        /// </summary>
        private void CheckProviderIsOnProgress()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                if (!_provider.IsOnLongProcess)
                {
                    CancelLongProcessButton.Visibility = Visibility.Collapsed;
                    LongProgressProgressBar.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                CancelLongProcessButton.Visibility = Visibility.Collapsed;
                LongProgressProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Update/Refresh view methods/event
        /// <summary>
        /// Get or set the number of byte are show in control
        /// </summary>
        public int BytePerLine
        {
            get { return (int)GetValue(BytePerLineProperty); }
            set { SetValue(BytePerLineProperty, value); }
        }

        public static readonly DependencyProperty BytePerLineProperty =
            DependencyProperty.Register("BytePerLine", typeof(int), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(16, new PropertyChangedCallback(BytePerLine_PropertyChanged),
                    new CoerceValueCallback(BytePerLine_CoerceValue)));

        private static object BytePerLine_CoerceValue(DependencyObject d, object baseValue)
        {
            HexaEditor ctrl = d as HexaEditor;
            
            var value = (int)baseValue;

            if (value < 8)
                return 8;
            else if (value > 32)
                return 32;
            else
                return baseValue;
        }

        private static void BytePerLine_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexaEditor ctrl = d as HexaEditor;

            if (e.NewValue != e.OldValue)
            {
                ctrl.RefreshView(true);
                ctrl.UpdateHexHeader();
            }
            
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            if (e.NewSize.Height > e.PreviousSize.Height || 
                e.NewSize.Height < e.PreviousSize.Height)
                RefreshView(true);
        }
        
        private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {            
            RefreshView(false);
        }

        /// <summary>
        /// Update vertical scrollbar with file info
        /// </summary>
        public void UpdateVerticalScroll()
        {
            VerticalScrollBar.Visibility = Visibility.Collapsed;

            if (ByteProvider.CheckIsOpen(_provider))
            {
                //TODO : check if need to show
                VerticalScrollBar.Visibility = Visibility.Visible;

                VerticalScrollBar.SmallChange = 1;
                VerticalScrollBar.LargeChange = ScrollLargeChange;
                VerticalScrollBar.Maximum = GetMaxLine() - GetMaxVisibleLine() + 1;
            }
        }

        /// <summary>
        /// Update de SelectionLine property
        /// </summary>
        private void UpdateSelectionLine()
        {
            if (ByteProvider.CheckIsOpen(_provider))
                SelectionLine = (SelectionStart / BytePerLine) + 1;
            else
                SelectionLine = 0;            
        }

        /// <summary>
        /// Refresh currentview of hexeditor
        /// </summary>
        /// <param name="ControlResize"></param>
        public void RefreshView(bool ControlResize = false, bool RefreshData = true)
        {
            UpdateLinesInfo();
            ////UpdateVerticalScroll();
            ////UpdateHexHeader();

            if (RefreshData)
            {
                UpdateDataAndStringViewer(ControlResize);
                //UpdateStringDataViewer(ControlResize);
                //UpdateDataViewer(ControlResize);
            }

            //UpdateByteModified();
            //UpdateSelection();
            UpdateSelection2();

            //UpdateHighLightByte();
            UpdateHighLightByte2();

            UpdateFocusState();

            UpdateStatusBar();
            
            CheckProviderIsOnProgress();

            UpdateSectorLines();

            if (ControlResize)
                UpdateScrollMarkerPosition();

            
        }
        
        /// <summary>
        /// Update the selection of byte in hexadecimal panel
        /// </summary>
        private void UpdateSelectionColorMode(FirstColor coloring)
        {
            switch (coloring)
            {
                case FirstColor.HexByteData:
                    TraverseDataControls(byteControl => byteControl.HexByteFirstSelected = true);
                    TraverseStringControls(sbControl => sbControl.StringByteFirstSelected = false);
                    break;
                case FirstColor.StringByteData:
                    TraverseDataControls(control => control.HexByteFirstSelected = false);
                    TraverseStringControls(control => control.StringByteFirstSelected = true);
                    break;
            }
        }

        private void TraverseDataControls(Action<HexByteControl> act) {
            foreach (StackPanel hexDataStack in HexDataStackPanel.Children) {
                //HexByte panel
                foreach (HexByteControl byteControl in hexDataStack.Children) {
                    act(byteControl);
                }
            }
        }
        private void TraverseStringControls(Action<StringByteControl> act) {
            foreach (StackPanel stringDataStack in StringDataStackPanel.Children) {
                //Stringbyte panel
                foreach (StringByteControl sbControl in stringDataStack.Children) {
                    act(sbControl);
                }
            }
        }

        /// <summary>
        /// Update byte are modified
        /// </summary>
        private void UpdateByteModified()
        {
            int stackIndex = 0;
            ByteModified byteModifiedCopy = null;

            if (ByteProvider.CheckIsOpen(_provider))
                foreach (ByteModified byteModified in _provider.ByteModifieds(ByteAction.All))
                {
                    stackIndex = 0;
                    byteModifiedCopy = byteModified.GetCopy();

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                        {
                            if (byteModifiedCopy.BytePositionInFile == byteControl.BytePositionInFile)
                            {
                                byteControl.InternalChange = true;
                                byteControl.Byte = byteModifiedCopy.Byte;

                                switch (byteModifiedCopy.Action)
                                {
                                    case ByteAction.Modified:
                                        byteControl.Action = ByteAction.Modified;
                                        break;
                                    case ByteAction.Deleted:
                                        byteControl.Action = ByteAction.Deleted;
                                        break;
                                }
                                byteControl.InternalChange = false;
                            }
                        }

                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        {
                            if (byteModifiedCopy.BytePositionInFile == byteControl.BytePositionInFile)
                            {
                                byteControl.InternalChange = true;
                                byteControl.Byte = byteModifiedCopy.Byte;

                                switch (byteModifiedCopy.Action)
                                {
                                    case ByteAction.Modified:
                                        byteControl.Action = ByteAction.Modified;
                                        break;
                                    case ByteAction.Deleted:
                                        byteControl.Action = ByteAction.Deleted;
                                        break;
                                }
                                byteControl.InternalChange = false;
                            }
                        }

                        stackIndex++;

                        //Prevent index out off range exception when resize at EOF
                        if (stackIndex == HexDataStackPanel.Children.Count && VerticalScrollBar.Value == VerticalScrollBar.Maximum)
                            stackIndex--;
                    }
                }
        }

        /// <summary>
        /// Update the selection of byte
        /// </summary>
        private void UpdateSelection()
        {
            int stackIndex = 0;
            foreach (Label infolabel in LinesInfoStackPanel.Children)
            {
                if (SelectionStart <= SelectionStop)
                {
                    //Stringbyte panel
                    if (StringDataStackPanel.Children.Count > 0)
                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                            if (byteControl.BytePositionInFile >= SelectionStart &&
                                byteControl.BytePositionInFile <= SelectionStop &&
                                byteControl.BytePositionInFile > -1)
                                byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                            else
                                byteControl.IsSelected = false;

                    //HexByte panel
                    if (HexDataStackPanel.Children.Count > 0)
                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                            if (byteControl.BytePositionInFile >= SelectionStart &&
                                byteControl.BytePositionInFile <= SelectionStop &&
                                byteControl.BytePositionInFile > -1)
                                byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                            else
                                byteControl.IsSelected = false;
                }
                else
                {
                    //Stringbyte panel
                    if (StringDataStackPanel.Children.Count > 0)
                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                            if (byteControl.BytePositionInFile >= SelectionStop &&
                                byteControl.BytePositionInFile <= SelectionStart &&
                                byteControl.BytePositionInFile > -1)
                                byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                            else
                                byteControl.IsSelected = false;

                    //HexByte panel
                    if (HexDataStackPanel.Children.Count > 0)
                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        if (byteControl.BytePositionInFile >= SelectionStop &&
                            byteControl.BytePositionInFile <= SelectionStart &&
                            byteControl.BytePositionInFile > -1)
                            byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                        else
                            byteControl.IsSelected = false;
                }
                stackIndex++;

                //Prevent index out off range exception when resize at EOF
                if (stackIndex == HexDataStackPanel.Children.Count && VerticalScrollBar.Value == VerticalScrollBar.Maximum)
                    stackIndex--;
            }
        }

        /// <summary>
        /// "Simplyfied" Update the selection of byte
        /// </summary>
        private void UpdateSelection2() {
            long minSelect = SelectionStart <= SelectionStop ? SelectionStart : SelectionStop;
            long maxSelect = SelectionStart <= SelectionStop ? SelectionStop : SelectionStart;

            foreach (StackPanel stringDataStack in StringDataStackPanel.Children) {
                //Stringbyte panel
                foreach (StringByteControl byteControl in stringDataStack.Children) {
                    if (byteControl.BytePositionInFile >= minSelect &&
                        byteControl.BytePositionInFile <= maxSelect &&
                        byteControl.BytePositionInFile != -1) {

                        byteControl.IsSelected = byteControl.Action == ByteAction.Deleted ? false : true;
                    }
                    else {
                        byteControl.IsSelected = false;
                    }
                }
            }

            foreach (StackPanel hexDataStack in HexDataStackPanel.Children) {
                //HexByte panel
                foreach (HexByteControl sbControl in hexDataStack.Children) {
                    if (sbControl.BytePositionInFile >= minSelect &&
                        sbControl.BytePositionInFile <= maxSelect &&
                        sbControl.BytePositionInFile != -1) {

                        sbControl.IsSelected = sbControl.Action == ByteAction.Deleted ? false : true;
                    }
                    else {
                        sbControl.IsSelected = false;
                    }
                }
            }


        }

        /// <summary>
        /// Update bytes as marked on findall()
        /// </summary>
        private void UpdateHighLightByte()
        {
            int stackIndex = 0;
            bool find = false;

            if (_markedPositionList.Count > 0) 
            {
                //var ByteList = from hlb in _markedPositionList
                //         where hlb >= GetFirstVisibleBytePosition() + BytePerLine && hlb <= GetLastVisibleBytePosition() + BytePerLine
                //         select hlb;

                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    //Stringbyte panel
                    foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        find = false;
                        
                        foreach (long position in _markedPositionList)
                            if (position == byteControl.BytePositionInFile)
                            {
                                find = true;
                                break;
                            }

                        byteControl.IsHighLight = find;
                    }

                    //HexByte panel
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        find = false;

                        foreach (long position in _markedPositionList)
                            if (position == byteControl.BytePositionInFile)
                            {
                                find = true;
                                break;
                            }

                        byteControl.IsHighLight = find;
                    }

                    stackIndex++;

                    //Prevent index out off range exception when resize at EOF
                    if (stackIndex == HexDataStackPanel.Children.Count && VerticalScrollBar.Value == VerticalScrollBar.Maximum)
                        stackIndex--;
                }
            }
            else //Un highlight all
            {
                stackIndex = 0;

                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    //Stringbyte panel
                    if (StringDataStackPanel.Children.Count > 0)
                        foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                            byteControl.IsHighLight = false;

                    //HexByte panel
                    if (HexDataStackPanel.Children.Count > 0)
                        foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                            byteControl.IsHighLight = false;

                    stackIndex++;

                    //Prevent index out off range exception when resize at EOF
                    if (stackIndex == HexDataStackPanel.Children.Count && VerticalScrollBar.Value == VerticalScrollBar.Maximum)
                        stackIndex--;
                }
            }
        }

        /// <summary>
        /// Update bytes as marked on findall()
        /// </summary>
        private void UpdateHighLightByte2() {
            if (_markedPositionList.Count > 0) {
                //var ByteList = from hlb in _markedPositionList
                //         where hlb >= GetFirstVisibleBytePosition() + BytePerLine && hlb <= GetLastVisibleBytePosition() + BytePerLine
                //         select hlb;

                foreach (StackPanel stringDataStack in StringDataStackPanel.Children) {
                    //Stringbyte panel
                    foreach (StringByteControl byteControl in stringDataStack.Children) {
                        byteControl.IsHighLight = _markedPositionList.Exists(p => p == byteControl.BytePositionInFile);
                    }
                }
                
                foreach (StackPanel hexDataStack in HexDataStackPanel.Children) {
                    //HexByte panel
                    foreach (HexByteControl sbControl in hexDataStack.Children) {
                        sbControl.IsHighLight = _markedPositionList.Exists(p => p == sbControl.BytePositionInFile);
                    }
                }
            }
            else //Un highlight all
            {
                

                foreach (StackPanel stringDataStack in StringDataStackPanel.Children) {
                    //Stringbyte panel
                    foreach (StringByteControl byteControl in stringDataStack.Children) {
                        byteControl.IsHighLight = false;
                    }
                }
                
                foreach (StackPanel hexDataStack in HexDataStackPanel.Children) {
                    //HexByte panel
                    foreach (HexByteControl sbControl in hexDataStack.Children) {
                        sbControl.IsHighLight = false;
                    }
                }
            }
        }

        private void UpdateFocusState() {
            if (focusPosition != -1) {
                foreach (StackPanel stringDataStack in StringDataStackPanel.Children) {
                    //Stringbyte panel
                    foreach (StringByteControl sbControl in stringDataStack.Children) {
                        if (sbControl.BytePositionInFile == focusPosition) {
                            sbControl.IsFocus = true;
                        }
                        else {
                            sbControl.IsFocus = false;
                        }
                    }
                }

                foreach (StackPanel hexDataStack in HexDataStackPanel.Children) {
                    //HexByte panel
                    foreach (HexByteControl hbControl in hexDataStack.Children) {
                        if (hbControl.BytePositionInFile == focusPosition) {
                            hbControl.IsFocus = true;
                        }
                        else {
                            hbControl.IsFocus = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the dataviewer stackpanel
        /// </summary>
        private void UpdateDataViewer(bool ControlResize)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                if (ControlResize)
                {
                    HexDataStackPanel.Children.Clear();

                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        StackPanel dataLineStack = new StackPanel();
                        dataLineStack.Height = _lineInfoHeight;
                        dataLineStack.Orientation = Orientation.Horizontal;

                        long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                        for (int i = 0; i < BytePerLine; i++)
                        {
                            _provider.Position = position + i; //+ correction;

                            if (_provider.Position >= _provider.Length)
                                break;

                            HexByteControl byteControl = new HexByteControl();

                            byteControl.BytePositionInFile = _provider.Position;
                            byteControl.ReadOnlyMode = ReadOnlyMode;
                            byteControl.MouseSelection += Control_MouseSelection;
                            byteControl.Click += Control_Click;
                            byteControl.RightClick += Control_RightClick;
                            byteControl.MoveNext += Control_MoveNext;
                            byteControl.MovePrevious += Control_MovePrevious;
                            byteControl.ByteModified += Control_ByteModified;
                            byteControl.MoveUp += Control_MoveUp;
                            byteControl.MoveDown += Control_MoveDown;
                            byteControl.MoveLeft += Control_MoveLeft;
                            byteControl.MoveRight += Control_MoveRight;
                            byteControl.MovePageUp += Control_MovePageUp;
                            byteControl.MovePageDown += Control_MovePageDown;
                            byteControl.ByteDeleted += Control_ByteDeleted;
                            byteControl.EscapeKey += Control_EscapeKey;

                            byteControl.InternalChange = true;
                            byteControl.Byte = (byte)_provider.ReadByte();
                            byteControl.InternalChange = false;

                            dataLineStack.Children.Add(byteControl);
                        }

                        HexDataStackPanel.Children.Add(dataLineStack);
                    }
                }
                else
                {
                    int stackIndex = 0;
                    foreach (Label infolabel in LinesInfoStackPanel.Children)
                    {
                        long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                        if (HexDataStackPanel.Children.Count > 0)
                            foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                            {
                                _provider.Position = position++;

                                byteControl.ReadOnlyMode = ReadOnlyMode;
                                byteControl.Action = ByteAction.Nothing;

                                byteControl.InternalChange = true;
                                if (_provider.Position >= _provider.Length)
                                {
                                    byteControl.BytePositionInFile = -1;
                                    byteControl.Byte = null;
                                }
                                else
                                {
                                    byteControl.ReadOnlyMode = ReadOnlyMode;
                                    byteControl.BytePositionInFile = _provider.Position;
                                    byteControl.Byte = (byte)_provider.ReadByte();
                                }
                                byteControl.InternalChange = false;
                            }

                        stackIndex++;

                        //Prevent index out off range exception when resize at EOF
                        if (stackIndex == HexDataStackPanel.Children.Count && VerticalScrollBar.Value == VerticalScrollBar.Maximum)
                            stackIndex--;
                    }
                }
            }
            else
            {
                HexDataStackPanel.Children.Clear();
            }
        }
        
        /// <summary>
        /// Save the buffer as a filed,To save the time when updating.
        /// </summary>
        private byte[] buffer;
        /// <summary>
        /// Update the data and string stackpanels;
        /// </summary>
        /// 
        private void UpdateDataAndStringViewer(bool ControlResize) {
            if (ByteProvider.CheckIsOpen(_provider)) {
                if (ControlResize) {
                    #region 
                    //StringDataStackPanel.Children.Clear();

                    //var enumrator = LinesInfoStackPanel.Children.GetEnumerator();
                    //enumrator.MoveNext();
                    //var firstInfoLabel = enumrator.Current as Label;
                    //var startPosition = ByteConverters.DecimalLiteralToLong(firstInfoLabel.Content.ToString());

                    Action<int> buildDataLinesAct = e => {
                         
                        for (int lineIndex = StringDataStackPanel.Children.Count; lineIndex < e; lineIndex++) {
                            #region
                            StackPanel dataLineStack = new StackPanel();
                            dataLineStack.Height = _lineInfoHeight;
                            dataLineStack.Orientation = Orientation.Horizontal;

                            for (int i = 0; i < BytePerLine; i++) {
                                StringByteControl sbCtrl = new StringByteControl();

                                //sbCtrl.StringByteModified += Control_ByteModified;
                                sbCtrl.ReadOnlyMode = ReadOnlyMode;
                                sbCtrl.MoveNext += Control_MoveNext;
                                sbCtrl.MovePrevious += Control_MovePrevious;
                                sbCtrl.MouseSelection += Control_MouseSelection;
                                sbCtrl.Click += Control_Click;
                                sbCtrl.RightClick += Control_RightClick;
                                sbCtrl.MoveUp += Control_MoveUp;
                                sbCtrl.MoveDown += Control_MoveDown;
                                sbCtrl.MoveLeft += Control_MoveLeft;
                                sbCtrl.MoveRight += Control_MoveRight;
                                sbCtrl.ByteDeleted += Control_ByteDeleted;
                                sbCtrl.EscapeKey += Control_EscapeKey;

                                sbCtrl.InternalChange = true;
                                sbCtrl.TBLCharacterTable = _TBLCharacterTable;
                                sbCtrl.TypeOfCharacterTable = TypeOfCharacterTable;

                                sbCtrl.Byte = null;
                                sbCtrl.ByteNext = null;
                                sbCtrl.BytePositionInFile = -1;

                                sbCtrl.InternalChange = false;

                                dataLineStack.Children.Add(sbCtrl);
                            }
                            StringDataStackPanel.Children.Add(dataLineStack);
                            #endregion

                            #region
                            StackPanel hexaDataLineStack = new StackPanel();
                            hexaDataLineStack.Height = _lineInfoHeight;
                            hexaDataLineStack.Orientation = Orientation.Horizontal;

                            for (int i = 0; i < BytePerLine; i++) {
                                HexByteControl byteControl = new HexByteControl();
                                
                                byteControl.ReadOnlyMode = ReadOnlyMode;
                                byteControl.MouseSelection += Control_MouseSelection;
                                byteControl.Click += Control_Click;
                                byteControl.RightClick += Control_RightClick;
                                byteControl.MoveNext += Control_MoveNext;
                                byteControl.MovePrevious += Control_MovePrevious;
                                //byteControl.ByteModified += Control_ByteModified;
                                byteControl.MoveUp += Control_MoveUp;
                                byteControl.MoveDown += Control_MoveDown;
                                byteControl.MoveLeft += Control_MoveLeft;
                                byteControl.MoveRight += Control_MoveRight;
                                byteControl.MovePageUp += Control_MovePageUp;
                                byteControl.MovePageDown += Control_MovePageDown;
                                byteControl.ByteDeleted += Control_ByteDeleted;
                                byteControl.EscapeKey += Control_EscapeKey;

                                byteControl.InternalChange = true;
                                byteControl.Byte = null;
                                byteControl.BytePositionInFile = -1;
                                byteControl.InternalChange = false;

                                hexaDataLineStack.Children.Add(byteControl);
                            }

                            HexDataStackPanel.Children.Add(hexaDataLineStack);
                            #endregion
                        }

                    };

                    if (buffer == null) {
                        var fullSizeReadyToRead = GetMaxVisibleLine() * BytePerLine + 1;
                        buffer = new byte[fullSizeReadyToRead];
                        buildDataLinesAct((int)GetMaxVisibleLine());
                    }
                    else {
                        if (buffer.Length < GetMaxVisibleLine() * BytePerLine + 1) {
                            var fullSizeReadyToRead = GetMaxVisibleLine() * BytePerLine + 1;
                            buildDataLinesAct((int)GetMaxVisibleLine());
                            buffer = new byte[fullSizeReadyToRead];
                        }
                    }
                    #endregion
                    //_provider.Position = startPosition;
                    //var readSize = _provider.Read(buffer, 0, sizeReadyToRead);
                    #region
                    //foreach (Label infolabel in LinesInfoStackPanel.Children) {
                    //    StackPanel dataLineStack = new StackPanel();
                    //    dataLineStack.Height = _lineInfoHeight;
                    //    dataLineStack.Orientation = Orientation.Horizontal;

                    //    long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                    //    for (int i = 0; i < BytePerLine; i++) {
                    //        _provider.Position = position + i;

                    //        if (_provider.Position >= _provider.Length)
                    //            break;

                    //        StringByteControl sbCtrl = new StringByteControl();

                    //        sbCtrl.BytePositionInFile = _provider.Position;
                    //        //sbCtrl.StringByteModified += Control_ByteModified;
                    //        sbCtrl.ReadOnlyMode = ReadOnlyMode;
                    //        sbCtrl.MoveNext += Control_MoveNext;
                    //        sbCtrl.MovePrevious += Control_MovePrevious;
                    //        sbCtrl.MouseSelection += Control_MouseSelection;
                    //        sbCtrl.Click += Control_Click;
                    //        sbCtrl.RightClick += Control_RightClick;
                    //        sbCtrl.MoveUp += Control_MoveUp;
                    //        sbCtrl.MoveDown += Control_MoveDown;
                    //        sbCtrl.MoveLeft += Control_MoveLeft;
                    //        sbCtrl.MoveRight += Control_MoveRight;
                    //        sbCtrl.ByteDeleted += Control_ByteDeleted;
                    //        sbCtrl.EscapeKey += Control_EscapeKey;

                    //        sbCtrl.InternalChange = true;
                    //        sbCtrl.TBLCharacterTable = _TBLCharacterTable;
                    //        sbCtrl.TypeOfCharacterTable = TypeOfCharacterTable;

                    //        sbCtrl.Byte = (byte)_provider.ReadByte();
                    //        sbCtrl.ByteNext = (byte)_provider.ReadByte();

                    //        _provider.Position--;

                    //        //}
                    //        sbCtrl.InternalChange = false;

                    //        dataLineStack.Children.Add(sbCtrl);
                    //    }

                    //    StringDataStackPanel.Children.Add(dataLineStack);
                    //}
                    #endregion

                    #region 

                    //HexDataStackPanel.Children.Clear();

                    //foreach (Label infolabel in LinesInfoStackPanel.Children) {
                    //    StackPanel dataLineStack = new StackPanel();
                    //    dataLineStack.Height = _lineInfoHeight;
                    //    dataLineStack.Orientation = Orientation.Horizontal;

                    //    long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                    //    for (int i = 0; i < BytePerLine; i++) {
                    //        _provider.Position = position + i; //+ correction;

                    //        if (_provider.Position >= _provider.Length) {

                    //            break;
                    //        }

                    //        HexByteControl byteControl = new HexByteControl();

                    //        byteControl.BytePositionInFile = _provider.Position;
                    //        byteControl.ReadOnlyMode = ReadOnlyMode;
                    //        byteControl.MouseSelection += Control_MouseSelection;
                    //        byteControl.Click += Control_Click;
                    //        byteControl.RightClick += Control_RightClick;
                    //        byteControl.MoveNext += Control_MoveNext;
                    //        byteControl.MovePrevious += Control_MovePrevious;
                    //        //byteControl.ByteModified += Control_ByteModified;
                    //        byteControl.MoveUp += Control_MoveUp;
                    //        byteControl.MoveDown += Control_MoveDown;
                    //        byteControl.MoveLeft += Control_MoveLeft;
                    //        byteControl.MoveRight += Control_MoveRight;
                    //        byteControl.MovePageUp += Control_MovePageUp;
                    //        byteControl.MovePageDown += Control_MovePageDown;
                    //        byteControl.ByteDeleted += Control_ByteDeleted;
                    //        byteControl.EscapeKey += Control_EscapeKey;

                    //        byteControl.InternalChange = true;
                    //        byteControl.Byte = (byte)_provider.ReadByte();
                    //        byteControl.InternalChange = false;

                    //        dataLineStack.Children.Add(byteControl);
                    //    }

                    //    HexDataStackPanel.Children.Add(dataLineStack);
                    //}
                    #endregion

                    #region
                    //foreach (Label infolabel in LinesInfoStackPanel.Children) {
                    //    StackPanel dataLineStack = new StackPanel();
                    //    dataLineStack.Height = _lineInfoHeight;
                    //    dataLineStack.Orientation = Orientation.Horizontal;

                    //    long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                    //    for (int i = 0; i < BytePerLine; i++) {
                    //        _provider.Position = position + i;

                    //        if (_provider.Position >= _provider.Length)
                    //            break;

                    //        StringByteControl sbCtrl = new StringByteControl();

                    //        sbCtrl.BytePositionInFile = _provider.Position;
                    //        //sbCtrl.StringByteModified += Control_ByteModified;
                    //        sbCtrl.ReadOnlyMode = ReadOnlyMode;
                    //        sbCtrl.MoveNext += Control_MoveNext;
                    //        sbCtrl.MovePrevious += Control_MovePrevious;
                    //        sbCtrl.MouseSelection += Control_MouseSelection;
                    //        sbCtrl.Click += Control_Click;
                    //        sbCtrl.RightClick += Control_RightClick;
                    //        sbCtrl.MoveUp += Control_MoveUp;
                    //        sbCtrl.MoveDown += Control_MoveDown;
                    //        sbCtrl.MoveLeft += Control_MoveLeft;
                    //        sbCtrl.MoveRight += Control_MoveRight;
                    //        sbCtrl.ByteDeleted += Control_ByteDeleted;
                    //        sbCtrl.EscapeKey += Control_EscapeKey;

                    //        sbCtrl.InternalChange = true;
                    //        sbCtrl.TBLCharacterTable = _TBLCharacterTable;
                    //        sbCtrl.TypeOfCharacterTable = TypeOfCharacterTable;
                    //        sbCtrl.Byte = (byte)_provider.ReadByte();
                    //        sbCtrl.ByteNext = (byte)_provider.ReadByte();

                    //        _provider.Position--;

                    //        //}
                    //        sbCtrl.InternalChange = false;

                    //        dataLineStack.Children.Add(sbCtrl);
                    //    }

                    //    StringDataStackPanel.Children.Add(dataLineStack);
                    //}
                    //#endregion

                    //#region 

                    //HexDataStackPanel.Children.Clear();

                    //foreach (Label infolabel in LinesInfoStackPanel.Children) {
                    //    StackPanel dataLineStack = new StackPanel();
                    //    dataLineStack.Height = _lineInfoHeight;
                    //    dataLineStack.Orientation = Orientation.Horizontal;

                    //    long position = ByteConverters.DecimalLiteralToLong(infolabel.Content.ToString());

                    //    for (int i = 0; i < BytePerLine; i++) {
                    //        _provider.Position = position + i; //+ correction;

                    //        if (_provider.Position >= _provider.Length) {

                    //            break;
                    //        }

                    //        HexByteControl byteControl = new HexByteControl();

                    //        byteControl.BytePositionInFile = _provider.Position;
                    //        byteControl.ReadOnlyMode = ReadOnlyMode;
                    //        byteControl.MouseSelection += Control_MouseSelection;
                    //        byteControl.Click += Control_Click;
                    //        byteControl.RightClick += Control_RightClick;
                    //        byteControl.MoveNext += Control_MoveNext;
                    //        byteControl.MovePrevious += Control_MovePrevious;
                    //        //byteControl.ByteModified += Control_ByteModified;
                    //        byteControl.MoveUp += Control_MoveUp;
                    //        byteControl.MoveDown += Control_MoveDown;
                    //        byteControl.MoveLeft += Control_MoveLeft;
                    //        byteControl.MoveRight += Control_MoveRight;
                    //        byteControl.MovePageUp += Control_MovePageUp;
                    //        byteControl.MovePageDown += Control_MovePageDown;
                    //        byteControl.ByteDeleted += Control_ByteDeleted;
                    //        byteControl.EscapeKey += Control_EscapeKey;

                    //        byteControl.InternalChange = true;
                    //        byteControl.Byte = (byte)_provider.ReadByte();
                    //        byteControl.InternalChange = false;

                    //        dataLineStack.Children.Add(byteControl);
                    //    }

                    //    HexDataStackPanel.Children.Add(dataLineStack);
                    //}


                    #endregion
                    
                   
                }
                if(LinesInfoStackPanel.Children.Count == 0) {
                    return;
                }

                var enumrator = LinesInfoStackPanel.Children.GetEnumerator();
                enumrator.MoveNext();
                var firstInfoLabel = enumrator.Current as Label;
                var startPosition = ByteConverters.DecimalLiteralToLong(firstInfoLabel.Content.ToString());
                var sizeReadyToRead = LinesInfoStackPanel.Children.Count * BytePerLine + 1;
                _provider.Position = startPosition;
                var readSize = _provider.Read(buffer, 0, sizeReadyToRead);
                //sizeReadyToRead;

                var index = 0;

                #region
                var count = HexDataStackPanel.Children.Count;
                for (int stackIndex = 0; stackIndex < count; stackIndex++) {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children) {
                        byteControl.Action = ByteAction.Nothing;
                        byteControl.ReadOnlyMode = ReadOnlyMode;

                        byteControl.InternalChange = true;

                        if (index < readSize) {
                            //sbCtrl.Byte = (byte)_provider.ReadByte();
                            byteControl.Byte = buffer[index];

                            byteControl.BytePositionInFile = startPosition + index;
                            //sbCtrl.ByteNext = (byte)_provider.ReadByte();
                        } else {
                            byteControl.Byte = null;
                            byteControl.BytePositionInFile = -1;
                        }
                        byteControl.InternalChange = false;
                        index++;
                    }
                        
                }
                #endregion

                index = 0;

                #region
                for (int stackIndex = 0; stackIndex < count; stackIndex++) {
                    foreach (StringByteControl sbCtrl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children) {
                        sbCtrl.Action = ByteAction.Nothing;
                        sbCtrl.ReadOnlyMode = ReadOnlyMode;

                        sbCtrl.InternalChange = true;
                        sbCtrl.TBLCharacterTable = _TBLCharacterTable;
                        sbCtrl.TypeOfCharacterTable = TypeOfCharacterTable;

                        if (index < readSize) {
                            //sbCtrl.Byte = (byte)_provider.ReadByte();
                            sbCtrl.Byte = buffer[index];
                            sbCtrl.BytePositionInFile = startPosition + index;
                            if (index < readSize - 1) {
                                sbCtrl.ByteNext = buffer[index + 1];
                            }
                            else {
                                sbCtrl.ByteNext = null;
                            }
                        } else {
                            sbCtrl.Byte = null;
                            sbCtrl.ByteNext = null;
                            sbCtrl.BytePositionInFile = -1;
                        }
                        sbCtrl.InternalChange = false;
                        index++;
                    }
                }
                #endregion
                
            }
            else {
                buffer = null;
                TraverseDataControls(control => {
                    control.BytePositionInFile = -1;
                    control.Byte = null;
                    control.IsHighLight = false;
                    control.IsFocus = false;
                    control.IsSelected = false;
                });
                TraverseStringControls(control => {
                    control.BytePositionInFile = -1;
                    control.Byte = null;
                    control.ByteNext = null;
                    control.IsHighLight = false;
                    control.IsSelected = false;
                });
            }

        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateHexHeader()
        {
            HexHeaderStackPanel.Children.Clear();

            //if (ByteProvider.CheckIsOpen(_provider))
            //{
                for (int i = 0; i < BytePerLine; i++)
                {
                    TextBlock lineInfoTbl = new TextBlock();
                    lineInfoTbl.Height = _lineInfoHeight;
                    lineInfoTbl.Foreground = Brushes.Black;
                    lineInfoTbl.Width = 32;
                    lineInfoTbl.TextAlignment = TextAlignment.Center;
                    //lineInfoTbl.HorizontalAlignment = HorizontalAlignment.Center;
                    lineInfoTbl.VerticalAlignment = VerticalAlignment.Center;
                    lineInfoTbl.Text = ByteConverters.ByteToHex((byte)i).ToString();
                    lineInfoTbl.ToolTip = $"Column : {i.ToString()}";
                    //Create control
                    //Label LineInfoLabel = new Label();
                    //LineInfoLabel.Height = _lineInfoHeight;
                    //LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                    //LineInfoLabel.Foreground = Brushes.Gray;
                    //LineInfoLabel.Width = 32;
                    //LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                    //LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                    //LineInfoLabel.Content = ByteConverters.ByteToHex((byte)i);
                    //LineInfoLabel.ToolTip = $"Column : {i.ToString()}";

                    HexHeaderStackPanel.Children.Add(lineInfoTbl);
                }
            //}
        }

        /// <summary>
        /// Update the position info panel at left of the control
        /// </summary>
        public void UpdateLinesInfo()
        {
            LinesInfoStackPanel.Children.Clear();

            if (ByteProvider.CheckIsOpen(_provider))
            {
                for (int i = 0; i < GetMaxVisibleLine(); i++)
                {
                    long fds = GetMaxVisibleLine();
                    //LineInfo

                    long firstLineByte = ((long)VerticalScrollBar.Value + i) * BytePerLine;
                    string info = string.Format("{0:D8}", firstLineByte);
                        //"0x" + firstLineByte.ToString(ConstantReadOnly.HexLineInfoStringFormat, CultureInfo.InvariantCulture);

                    if (firstLineByte < _provider.Length)
                    {
                        //Create control
                        Label LineInfoLabel = new Label();
                        LineInfoLabel.Height = _lineInfoHeight;
                        LineInfoLabel.Padding = new Thickness(0, 0, 10, 0);
                        LineInfoLabel.Foreground = (SolidColorBrush) TryFindResource("LineInfoColor");
                        LineInfoLabel.MouseDown += LineInfoLabel_MouseDown;
                        LineInfoLabel.MouseMove += LineInfoLabel_MouseMove;
                        LineInfoLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                        LineInfoLabel.VerticalContentAlignment = VerticalAlignment.Center;
                        LineInfoLabel.Content = info;
                        LineInfoLabel.ToolTip = $"Byte : {firstLineByte.ToString()}";

                        LinesInfoStackPanel.Children.Add(LineInfoLabel);
                    }
                }
            }
        }

        #endregion Update/Refresh view methods

        #region First/Last visible byte methods
        /// <summary>
        /// Get first visible byte position in control
        /// </summary>
        /// <returns>Return -1 of no file open</returns>
        private long GetFirstVisibleBytePosition()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                int stackIndex = 0;
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        return byteControl.BytePositionInFile;

                    stackIndex++;
                }

                return -1;
            }
            else
                return -1;
        }

        /// <summary>
        /// Get last visible byte position in control
        /// </summary>
        /// <returns>Return -1 of no file open.</returns>
        private long GetLastVisibleBytePosition()
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                int stackIndex = 0;
                long byteposition = GetFirstVisibleBytePosition();
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                        byteposition++;

                    stackIndex++;
                }

                return byteposition;
            }
            else
                return -1;
        }
        #endregion First/Last visible byte methods

        #region Focus Methods

        /// <summary>
        /// Set focus on byte
        /// </summary>
        private void SetFocusHexDataPanel(long bytePositionInFile)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                if (bytePositionInFile >= _provider.Length)
                    return;

                int stackIndex = 0;
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (HexByteControl byteControl in ((StackPanel)HexDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile == bytePositionInFile)
                        {
                            byteControl.Focus();
                            return;
                        }
                    }

                    stackIndex++;
                }

                if (VerticalScrollBar.Value < VerticalScrollBar.Maximum)
                    VerticalScrollBar.Value++;

                SetFocusHexDataPanel(bytePositionInFile);
                //SetPosition(bytePositionInFile);
            }
        }

        /// <summary>
        /// Set focus on byte
        /// </summary>
        private void SetFocusStringDataPanel(long bytePositionInFile)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                if (bytePositionInFile >= _provider.Length)
                    return;

                int stackIndex = 0;
                foreach (Label infolabel in LinesInfoStackPanel.Children)
                {
                    foreach (StringByteControl byteControl in ((StackPanel)StringDataStackPanel.Children[stackIndex]).Children)
                    {
                        if (byteControl.BytePositionInFile == bytePositionInFile)
                        {
                            byteControl.Focus();
                            return;
                        }
                    }

                    stackIndex++;
                }

                if (VerticalScrollBar.Value < VerticalScrollBar.Maximum)
                    VerticalScrollBar.Value++;

                SetFocusStringDataPanel(bytePositionInFile);
                //SetPosition(bytePositionInFile);
            }
        }
        #endregion Focus Methods

        #region Find methods
        /// <summary>
        /// Find first occurence of string in stream. Search start as startPosition.
        /// </summary>        
        public long FindFirst(string text, long startPosition = 0)
        {
            return FindFirst(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find first occurence of byte[] in stream. Search start as startPosition.
        /// </summary>
        public long FindFirst(byte[] bytes, long startPosition = 0)
        {            
            if (ByteProvider.CheckIsOpen(_provider))
            {
                try
                {
                    var position = _provider.FindIndexOf(bytes, startPosition).First();
                    SetPosition(position, bytes.Length);
                    return position;
                }
                catch
                {
                    UnSelectAll();
                    return -1;
                }
            }
                        
            return -1;
        }

        /// <summary>
        /// Find next occurence of string in stream search start at SelectionStart.
        /// </summary>        
        public long FindNext(string text)
        {
            return FindNext(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find next occurence of byte[] in stream search start at SelectionStart.
        /// </summary>
        public long FindNext(byte[] bytes)
        {            
            if (ByteProvider.CheckIsOpen(_provider))
            {
                try
                {
                    var position = _provider.FindIndexOf(bytes, SelectionStart + 1).First();
                    SetPosition(position, bytes.Length);
                    return position;
                }
                catch
                {
                    UnSelectAll();
                    return -1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Find last occurence of string in stream search start at SelectionStart.
        /// </summary>        
        public long FindLast(string text)
        {
            return FindLast(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find first occurence of byte[] in stream.
        /// </summary>
        public long FindLast(byte[] bytes)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                try
                {
                    var position = _provider.FindIndexOf(bytes, SelectionStart + 1).Last();
                    SetPosition(position, bytes.Length);
                    return position;
                }
                catch
                {
                    UnSelectAll();
                    return -1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Find all occurence of string in stream.
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(string text)
        {
            return FindAll(ByteConverters.StringToByte(text));
        }

        /// <summary>
        /// Find all occurence of byte[] in stream.
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(byte[] bytes)
        {
            UnHighLightAll();

            if (ByteProvider.CheckIsOpen(_provider))
                return _provider.FindIndexOf(bytes, 0);
            
            return null;
        }

        /// <summary>
        /// Find all occurence of string in stream.
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(string text, bool highLight)
        {
            return FindAll(ByteConverters.StringToByte(text), highLight);
        }

        /// <summary>
        /// Find all occurence of string in stream. Highlight occurance in stream is MarcAll as true
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAll(byte[] bytes, bool highLight)
        {
            ClearScrollMarker(ScrollMarker.SearchHighLight);

            if (highLight)
            {
                var positions = FindAll(bytes);

                foreach (long position in positions)
                {
                    for (long i = position; i < position + bytes.Length; i++)
                        _markedPositionList.Add(i);

                    SetScrollMarker(position, ScrollMarker.SearchHighLight);
                }

                UnSelectAll();
                UpdateHighLightByte();

                //Sort list
                _markedPositionList.Sort();

                return positions;
            }
            else            
                return FindAll(bytes);            
        }

        /// <summary>
        /// Find all occurence of SelectionByteArray in stream. Highlight byte finded
        /// </summary>
        /// <returns>Return null if no occurence found</returns>
        public IEnumerable<long> FindAllSelection(bool highLight)
        {
            if (SelectionLength > 0)                            
                return FindAll(SelectionByteArray, highLight);            
            else
                return null;
        }
        #endregion Find methods

        #region Statusbar
        /// <summary>
        /// Update statusbar for somes property dont support dependency property
        /// </summary>
        private void UpdateStatusBar()
        {
            if (StatusBarVisibility == Visibility.Visible)
                if (ByteProvider.CheckIsOpen(_provider))
                {
                    bool MB = false;
                    long deletedBytesCount = _provider.ByteModifieds(ByteAction.Deleted).Count();

                    FileLengthLabel.Content = _provider.Length - deletedBytesCount;

                    //is mega bytes ?                    
                    double lenght = (_provider.Length - deletedBytesCount) / 1024;
                    if (lenght > 1024)
                    {
                        lenght = lenght / 1024;
                        MB = true;
                    }

                    FileLengthKBLabel.Content = Math.Round(lenght, 2) + (MB == true ? " MB" : " KB");
                }
                else
                {
                    FileLengthLabel.Content = 0;
                    FileLengthKBLabel.Content = 0;
                }
        }
        #endregion Statusbar

        #region Bookmark and other scrollmarker

        /// <summary>
        /// Get all bookmark are currently set 
        /// </summary>
        public IEnumerable<BookMark> BookMarks
        {
            get { return (IEnumerable<BookMark>)GetValue(BookMarksProperty); }
            internal set { SetValue(BookMarksProperty, value); }
        }

        public static readonly DependencyProperty BookMarksProperty =
            DependencyProperty.Register("BookMarks", typeof(IEnumerable<BookMark>), typeof(HexaEditor), 
                new FrameworkPropertyMetadata(new List<BookMark>()));
        
        /// <summary>
        /// Set bookmark at specified position
        /// </summary>
        /// <param name="position"></param>
        public void SetBookMark(long position)
        {
            SetScrollMarker(position, ScrollMarker.Bookmark);
        }

        /// <summary>
        /// Set bookmark at selection start
        /// </summary>
        public void SetBookMark()
        {
            SetScrollMarker(SelectionStart, ScrollMarker.Bookmark);
        }

        /// <summary>
        /// Set marker at position
        /// </summary>
        private void SetScrollMarker(long position, ScrollMarker marker)
        {
            Rectangle rect = new Rectangle();
            double topPosition = 0;
            double rightPosition = 0;

            //create bookmark
            var bookMark = new BookMark();
            bookMark.Marker = marker;
            bookMark.BytePositionInFile = position;

            //Remove selection start marker and set position
            if (marker == ScrollMarker.SelectionStart)
            {
                int i = 0;
                foreach (Rectangle ctrl in MarkerGrid.Children)
                {
                    if (((BookMark)ctrl.Tag).Marker == ScrollMarker.SelectionStart)
                    {
                        MarkerGrid.Children.RemoveAt(i);
                        break;
                    }
                    i++;
                }

                bookMark.BytePositionInFile = SelectionStart;
            }

            //Set position in scrollbar 
            topPosition = (GetLineNumber(bookMark.BytePositionInFile) * VerticalScrollBar.Track.TickHeight(GetMaxLine()) - 1);

            if (topPosition == double.NaN)
                topPosition = 0;

            //Check if position already exist and exit if exist
            if (marker != ScrollMarker.SelectionStart)
                foreach (Rectangle ctrl in MarkerGrid.Children)
                    if (ctrl.Margin.Top == topPosition && ((BookMark)ctrl.Tag).Marker == marker)
                        return;

            //Somes general properties
            rect.MouseDown += Rect_MouseDown;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.Tag = bookMark;
            rect.Width = 5;
            rect.Height = 3;

            var byteinfo = new ByteModified();
            byteinfo.BytePositionInFile = position;
            rect.DataContext = byteinfo;

            //Set somes properties for different marker
            switch (marker)
            {
                case ScrollMarker.Bookmark:
                    rect.ToolTip = TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)TryFindResource("BookMarkColor");
                    break;
                case ScrollMarker.SearchHighLight:
                    rect.ToolTip = TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)TryFindResource("SearchBookMarkColor");
                    rect.HorizontalAlignment = HorizontalAlignment.Center;
                    break;
                case ScrollMarker.SelectionStart:
                    rect.Fill = (SolidColorBrush)TryFindResource("SelectionStartBookMarkColor");
                    rect.Width = VerticalScrollBar.ActualWidth;
                    rect.Height = 2;
                    break;
                case ScrollMarker.ByteModified:
                    rect.ToolTip = TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)TryFindResource("ByteModifiedMarkColor");
                    rect.HorizontalAlignment = HorizontalAlignment.Right;
                    break;
                case ScrollMarker.ByteDeleted:
                    rect.ToolTip = TryFindResource("ScrollMarkerSearchToolTip");
                    rect.Fill = (SolidColorBrush)TryFindResource("ByteDeletedMarkColor");
                    rect.HorizontalAlignment = HorizontalAlignment.Right;
                    rightPosition = 4;
                    break;
            }

            try
            {
                rect.Margin = new Thickness(0, topPosition, rightPosition, 0);
            }
            catch { }

            //Add to grid
            MarkerGrid.Children.Add(rect);

            //Update bookmarks properties
            UpdateBookMarkProperties();
        }

        /// <summary>
        /// Update the bookmark properties are currently set
        /// </summary>
        private void UpdateBookMarkProperties()
        {
            List<BookMark> bmList = new List<BookMark>();
            foreach (Rectangle rc in MarkerGrid.Children)
            {
                BookMark bm = rc.Tag as BookMark;

                if (bm != null)
                    if (bm.Marker == ScrollMarker.Bookmark)
                        bmList.Add(bm);
            }
            BookMarks = bmList;
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;

            Debug.Print(rect.Tag.ToString());
            
            if (((BookMark)rect.Tag).Marker != ScrollMarker.SelectionStart)
                SetPosition(((BookMark)rect.Tag).BytePositionInFile, 1);
            else
                SetPosition(SelectionStart, 1);
        }

        /// <summary>
        /// Update all scroll marker position
        /// </summary>
        private void UpdateScrollMarkerPosition()
        {
            foreach (Rectangle rect in MarkerGrid.Children)            
                if (((BookMark)rect.Tag).Marker != ScrollMarker.SelectionStart)
                    rect.Margin = new Thickness(0, (GetLineNumber(((BookMark)rect.Tag).BytePositionInFile) * VerticalScrollBar.Track.TickHeight(GetMaxLine())) - rect.ActualHeight, 0, 0);            
        }

        /// <summary>
        /// Clear ScrollMarker
        /// </summary>
        public void ClearAllScrollMarker()
        {
            MarkerGrid.Children.Clear();
        }

        /// <summary>
        /// Clear ScrollMarker 
        /// </summary>
        public void ClearScrollMarker(ScrollMarker marker)
        {   
            for (int i = 0; i < MarkerGrid.Children.Count; i++)
            {
                BookMark mark = (BookMark)((Rectangle)MarkerGrid.Children[i]).Tag;

                if (mark.Marker == marker)
                {
                    MarkerGrid.Children.Remove(MarkerGrid.Children[i]);
                    i--;
                }
            }                
        }

        #endregion Bookmark and other scrollmarker

        #region Context menu

        private void Control_RightClick(object sender, EventArgs e)
        {
            ////position
            //StringByteControl sbCtrl = sender as StringByteControl;
            //HexByteControl ctrl = sender as HexByteControl;

            //if (sbCtrl != null)
            //    _rightClickBytePosition = sbCtrl.BytePositionInFile;
            //else if (ctrl != null)
            //    _rightClickBytePosition = ctrl.BytePositionInFile;

            ////update ctrl 
            ///*
            //CopyASCIICMenu.IsEnabled = false;
            //FindAllCMenu.IsEnabled = false;
            //CopyHexaCMenu.IsEnabled = false;
            //UndoCMenu.IsEnabled = false;
            //DeleteCMenu.IsEnabled = false;
            //*/
            ////BookMarkCMenu.IsEnabled = false;

            //if (SelectionLenght > 0)
            //{
            //    /*
            //    CopyASCIICMenu.IsEnabled = true;
            //    FindAllCMenu.IsEnabled = true;
            //    CopyHexaCMenu.IsEnabled = true;
            //    DeleteCMenu.IsEnabled = true;
            //    */
            //}
            
            //if (UndoCount > 0)
            //    UndoCMenu.IsEnabled = true;
            
            ////Show context menu
            //Focus();
            //CMenu.Visibility = Visibility.Visible;
        }

        private void FindAllCMenu_Click(object sender, RoutedEventArgs e)
        {
            FindAll(SelectionByteArray, true);
        }

        private void CopyHexaCMenu_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(CopyPasteMode.HexaString);
        }

        private void CopyASCIICMenu_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(CopyPasteMode.ASCIIString);
        }

        private void DeleteCMenu_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelection();
        }

        private void UndoCMenu_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }
                
        private void BookMarkCMenu_Click(object sender, RoutedEventArgs e)
        {
            SetBookMark(_rightClickBytePosition);
        }
                
        private void ClearBookMarkCMenu_Click(object sender, RoutedEventArgs e)
        {
            ClearScrollMarker(ScrollMarker.Bookmark);
        }

        private void PasteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (ByteProvider.CheckIsOpen(_provider))
            {
                _provider.PasteNotInsert(_rightClickBytePosition, Clipboard.GetText());

                RefreshView();
            }
        }

        #endregion Bookmark and other scrollmarker


    }

    
    
}
