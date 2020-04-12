using CDFCEntities.Enums;
using CDFCEntities.Files;
using CDFCUIContracts.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoTester.Models {
    public class VideoRow : BindableBaseTemp {
        public static VideoRow Empty {
            get {
                VideoRow row = new VideoRow();
                row.IsEmpty = true;
                return row;
            }
        }

        //是否是空行;
        public bool IsEmpty { get; private set; }

        /// <summary>
        /// 内部使用的空构造方法;
        /// </summary>
        private VideoRow() {

        }

        //当前行是否被选中;
        private bool isSelected;
        public bool IsSelected {
            get {
                return isSelected;
            }
            set {
                isSelected = value;
                
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public readonly Video Video;

        public VideoIntegrity Integrity {
            get {
                return IsEmpty ? VideoIntegrity.Covered : Video.Integrity;
            }
        }
        public VideoRow(Video video) {
            Video = video;
        }

        /// <summary>
        /// 更新状态;
        /// </summary>
        public void UpdateState() {
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(EndDate));
            OnPropertyChanged(nameof(Integrity));
            //OnPropertyChanged(nameof(StartAddress));
        }

        //文件大小;
        public long Size {
            get {
                return IsEmpty ? 0 : Video.Size;
            }
        }

        /// <summary>
        /// 是否被复选;
        /// </summary>
        private bool isChecked;
        public bool IsChecked {
            get {
                return IsEmpty ? false : isChecked;
            }
            set {
                isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        //通道号;
        public int ChannelNO {
            get {
                return IsEmpty ? 0 : System.Convert.ToInt32(Video.ChannelNO);
            }
        }

        //起始时间;
        public uint StartDate {
            get {
                return IsEmpty ? 0 : Video.StartDate;
            }
        }

        //终止时间;
        public uint EndDate {
            get {
                return IsEmpty ? 0 : Video.EndDate;
            }
        }

        //起始位置;
        public long StartAddress {
            get {
                return IsEmpty ? 0 : Video.StartAddress;
            }
        }

        /// <summary>
        /// 是否已在碎片图表中显示了其位置;
        /// </summary>
        private bool hasFragShown;
        public bool HasFragShown {
            get {
                return IsEmpty ? false : hasFragShown;
            }
            set {
                hasFragShown = value;
                OnPropertyChanged(nameof(HasFragShown));
            }
        }

        /// <summary>
        /// 唯一ID;
        /// </summary>
        public int ItemID { get; private set; }

        /// <summary>
        ///临时文件暂存的位置;
        /// </summary>
        public string PreviewedPath { get; set; }

        /// <summary>
        /// 文件预览帧的位置;
        /// </summary>
        private List<string> previewedImgs;
        public List<string> PreviewedImgs {
            get {
                return previewedImgs ??
                    (previewedImgs = new List<string>());
            }
        }

        
        public string DateCategory { get; set; }
    }
}
