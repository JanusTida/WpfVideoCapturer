using CDFCCultures.Helpers;
using CDFCDavPlayer.Player;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CDFCDavPlayer.Models {
    //播放文件项目;
    public class SongItem : BindableBase {
        public SongItem(IPlayer player,string fullName) {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            Player = player;
            this.FullName = fullName;
        }
        public IPlayer Player { get; }

        /// <summary>
        /// 总时长;
        /// </summary>
        private TimeSpan? _totalLength;
        public TimeSpan? TotalLength {
            get {
                return _totalLength;
            }
            set {
                SetProperty(ref _totalLength, value);
            }
        }

        private bool _isChecked;
        public bool IsChecked {
            get {
                return _isChecked;
            }
            set {
                SetProperty(ref _isChecked, value);
            }
        }

        private ImageSource _preSource;
        public ImageSource PreSource {
            get {
                return _preSource;
            }
            set {
                SetProperty(ref _preSource, value);
            }
        }

        private string _songName;
        public string SongName {
            get {
                if(_songName == null && FullName != null) {
                    _songName = IOPathHelper.GetFileNameFromUrl(FullName);
                }
                
                return _songName;
            }
            set {
                SetProperty(ref _songName, value);
            }
        }

        public string FullName { get; }
        

        private TimeSpan? _songTimeLength;
        public TimeSpan? SongLength {
            get {
                return _songTimeLength;
            }
            set {
                SetProperty(ref _songTimeLength, value);
            }
        }

    }
}
