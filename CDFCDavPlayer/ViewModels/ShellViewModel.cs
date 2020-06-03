using CDFCDavPlayer.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using CDFCDavPlayer.Models;
using System.Windows;
using EventLogger;
using System.Threading;
using CDFCMessageBoxes.MessageBoxes;
using CDFCDavPlayer.Helpers;
using System.IO;
using static CDFCUIContracts.Helpers.ApplicationHelper;
using Prism.Mvvm;
using Prism.Commands;
using System.Reflection;

namespace CDFCDavPlayer.ViewModels {
    [Export(typeof(IShellViewModel))]
    public partial class ShellViewModel: BindableBase,IShellViewModel {
        public ShellViewModel() {
            
        }

        /// <summary>
        /// 请求窗体聚焦;
        /// </summary>
        public event EventHandler ActivateRequest;

        /// <summary>
        /// 检查进程内存通知叠加;
        /// </summary>
        private void CheckSongNotified() {
            var args = StartUpHelper.ReadCommonStartUpArgs();
            if (args != null && args.Length != 0) {
                AppInvoke(() => {
                    AddSong(args[0]);
                    ActivateRequest?.Invoke(this, EventArgs.Empty);
                });
            }
        }

        private IPlayer _player;
        public IPlayer Player {
            get {
                return _player;
            }
            set {
                SetProperty(ref _player, value);
                RaisePropertyChanged(nameof(Title));
            }
        }

        private DelegateCommand _addSongCommand;
        public DelegateCommand AddSongCommand => _addSongCommand ??
            (_addSongCommand = new DelegateCommand(
                () => {
                    var dialog = new VistaOpenFileDialog();
                    dialog.Multiselect = false;
                    if(dialog.ShowDialog() == true) {
                        AddSong(dialog.FileName);
                    }
                }
            ));

        private void AddSong(string path) {
            try {
                //检查是否已经添加过此文件;
                var preSong = SongItems.FirstOrDefault(p => p.FullName == path);
                //若已经添加,则切换至对应文件;
                if (preSong != null) {
                    SelectedSong = preSong;
                    Play(SelectedSong.Player);
                    return;
                }


                var player = new DHPlayer(path);
                Play(player);

                var item = new SongItem(player, path);

                item.PreSource = Player.GetImageSource(TimeSpan.FromSeconds(2));
                item.SongLength = Player.TotalTimeSpan;
                SongItems.Add(item);
                SelectedSong = item;
                IsPlaying = true;
            }
            catch {
                CDFCMessageBox.Show("Error when constructing player!");
            }
        }

        private long priLevel = 0;
        /// <summary>
        /// 重新开始一次播放;
        /// </summary>
        /// <param name="player"></param>
        private void Play(IPlayer player) {
            Player?.Dispose();
            Player = player;
            player.Play();
            player.SetVolume((uint)Volume);
            var totalTs = player.TotalTimeSpan;
            if(totalTs != null) {
                MaxTimeSpan = totalTs.Value;
                PlayValue = 0;
            }

            var curLevel = ++priLevel;
            //部署升级进度条升级;
            ThreadPool.QueueUserWorkItem(cb => {
                while(curLevel == priLevel && Player != null) {
                    var curTs = Player.CurrentTimeSpan;
                    if(curTs != null && !IsProcessing) {
                        Application.Current?.Dispatcher.Invoke(() => {
                            _playValue = curTs.Value.TotalSeconds;
                            CurrentTimeSpan = TimeSpan.FromSeconds((long)curTs.Value.TotalSeconds);
                            MaxTimeSpan = Player.TotalTimeSpan??TimeSpan.Zero;
                            OnPropertyChanged(nameof(PlayValue));
                        });
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        public void RaiseOnLoad()
        {
            if (StartUpHelper.StartUpArgs != null)
            {
                var args = StartUpHelper.StartUpArgs.Args;
                foreach (var arg in args)
                {
                    try
                    {
                        if (File.Exists(arg))
                        {
                            AddSong(arg);
                        }
                    }
                    catch
                    {

                    }
                }
            }
            ThreadPool.QueueUserWorkItem(cb => {
                while (true)
                {
                    CheckSongNotified();
                    Thread.Sleep(1000);
                }
            });
        }

        public void RaiseOnDrop(IDataObject dataObject)
        {
            if(dataObject == null)
            {
                return;
            }

            try
            {
                var files = (string[])dataObject.GetData(DataFormats.FileDrop);
                foreach (var file in files)
                {
                    AddSong(file);
                }
                ActivateRequest?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private bool _isProcessing;
        public bool IsProcessing {
            get {
                return _isProcessing;
            }
            set {
                SetProperty(ref _isProcessing, value);
            }
        }

        private TimeSpan _maxTimeSpan;
        public TimeSpan MaxTimeSpan {
            get {
                return _maxTimeSpan;
            }
            set {
                SetProperty(ref _maxTimeSpan, value);
                RaisePropertyChanged(nameof(MaxTimeLength));
            }
        }
        public double MaxTimeLength => MaxTimeSpan.TotalSeconds;

        private TimeSpan _currentTimeSpan;
        public TimeSpan CurrentTimeSpan {
            get {
                return _currentTimeSpan;
            }
            set {
                SetProperty(ref _currentTimeSpan, value);
            }
        }
        
        private double _playValue;
        public double PlayValue {
            get {
                return _playValue;
            }
            set {
                SetProperty(ref _playValue, value);
                if(Player != null) {
                    Player.EscapeToTimeSpan(TimeSpan.FromSeconds(value));
                }
            }
        }

        private SongItem _selectedSong;
        public SongItem SelectedSong {
            get {
                return _selectedSong;
            }
            set {
                SetProperty(ref _selectedSong, value);
            }
        }

        public ObservableCollection<SongItem> SongItems { get; set; } = new ObservableCollection<SongItem>();

        private bool _isPlaying;
        public bool IsPlaying {
            get {
                return _isPlaying;
            }
            set {
                SetProperty(ref _isPlaying, value);
            }
        }

        private DelegateCommand _playOrPauseCommand;
        public DelegateCommand PlayOrPauseCommand => _playOrPauseCommand ??
            (_playOrPauseCommand = new DelegateCommand(
                () => {
                    if (IsPlaying) {
                        try {
                            if(Player?.Pause() == true) {
                                IsPlaying = false;
                            }
                        }
                        catch {

                        }
                    }
                    else {
                        try {
                            if(Player?.Resume() == true) {
                                IsPlaying = true;
                            }
                        }
                        catch(Exception ex) {
                            Logger.WriteLine($"{nameof(ShellViewModel)}->{nameof(PlayOrPauseCommand)}:{ex.Message}");
                            MessageBox.Show("Failed to Resume");
                        }
                    }
                }
            ));

        private DelegateCommand _removeSongCommand;
        public DelegateCommand RemoveSongCommand => _removeSongCommand ??
            (_removeSongCommand = new DelegateCommand(
                () => {
                    var removeItems = new List<SongItem>();
                    foreach (var item in SongItems.Where(p => p.IsChecked)) {
                        try {
                            //SongItems.Remove()
                            removeItems.Add(item);
                            item.Player.Dispose();
                            if(Player == item.Player) {
                                Player = null;
                                priLevel++;
                                MaxTimeSpan = TimeSpan.Zero;
                                CurrentTimeSpan = TimeSpan.Zero;
                            }
                        }
                        catch (Exception ex) {
                            Logger.WriteLine($"{nameof(ShellViewModel)}->{nameof(RemoveSongCommand)}:{ex.Message}");
                            CDFCMessageBox.Show(ex.Message);
                        }
                    }
                    removeItems.ForEach(p => SongItems.Remove(p));
                }
            ));

        private DelegateCommand _closingCommand;
        public DelegateCommand ClosingCommand => _closingCommand ??
            (_closingCommand = new DelegateCommand(
                () => {
                    foreach (var item in SongItems) {
                        try {
                            item.Player.Dispose();
                        }
                        catch(Exception ex) {
                            Logger.WriteLine($"{nameof(ShellViewModel)}->{nameof(ClosingCommand)}:{ex.Message}");
                        }
                    }
                    priLevel++;
                }
            ));

        private DelegateCommand<double?> _goForwardCommand;
        public DelegateCommand<double?> GoForwardCommand =>
            _goForwardCommand = (_goForwardCommand = new DelegateCommand<double?>(
                dis => {
                    if (Player == null) {
                        return;
                    }
                    if(dis == null) {
                        return;
                    }

                    var curTs = Player.CurrentTimeSpan;
                    if(curTs != null) {
                        //if (curTs.Value.Add(TimeSpan.FromSeconds(dis)) <= Player.TotalTimeSpan) {
                            var s = Player.EscapeToTimeSpan(curTs.Value.Add(TimeSpan.FromSeconds(dis.Value)));
                        //}
                    }
                    
                }));

        private DelegateCommand _choseSongCommand;
        public DelegateCommand ChoseSongCommand => _choseSongCommand ??
            (_choseSongCommand = new DelegateCommand(
                () => {
                    if(SelectedSong != null) {
                        Play(SelectedSong.Player);
                    }
                }
            ));

        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ??
            (_stopCommand = new DelegateCommand(
                () => {
                    Player?.Dispose();
                    Player = null;
                }
            ));

        //private DelegateCommand<DragEventArgs> _dropCommand;
        //public DelegateCommand<DragEventArgs> DropCommand =>
        //    _dropCommand??(_dropCommand = new DelegateCommand<DragEventArgs>(
        //        arg => {

        //        }
        //    ));
#if 流火
        private const string _softName = "流火视频播放器";
#else
        private const string _softName = "黑洞视频播放器";
#endif
        private string SoftName
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                var asmVersion = asm.GetName().Version;
                return $"{_softName}v{asmVersion.Major}.{asmVersion.Minor}";
            }
        }

        public string Title {
            get {
                if(Player != null)
                {
                    return $"{SoftName}-{Path.GetFileName(Player.FileName)}";
                }
                return SoftName;
            }
            
        }

    }

    public partial class ShellViewModel {
        private double _volume = 100;
        public double Volume {
            get {
                return _volume;
            }
            set {
                SetProperty(ref _volume, value);
                Player?.SetVolume((uint)value);
            }
        }

        private double _maxVolume = 100;
        public double MaxVolume {
            get {
                return _maxVolume;
            }
            set {
                SetProperty(ref _maxVolume, value);
            }
        }
        
    }

    public interface IShellViewModel {
        string Title { get; }

        void RaiseOnLoad();

        void RaiseOnDrop(IDataObject dataObject);

        event EventHandler ActivateRequest;
    }
}
