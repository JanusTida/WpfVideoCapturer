using CDFCUIContracts.Abstracts;
using System;
using System.IO;

namespace WPFHexaEditor.Control.Abstracts {
    public abstract class HexEditorViewModel : BindableBaseTemp {
        private bool readOnlyMode;
        public bool ReadOnlyMode {
            get {
                if(Stream != null) {
                    return !Stream.CanWrite;
                }
                return false;
            }
        }

        private Stream stream;                      //所描述的流对象;
        public Stream Stream {
            get {
                return stream;
            }
            set {
                SetProperty(ref stream, value);
                OnPropertyChanged(nameof(ReadOnlyMode));
            }
        }

        private long selectionStart = -1L;           //选择起始位置;
        public long SelectionStart {
            get {
                return selectionStart;
            }
            set {
                SetProperty(ref selectionStart, value);
            }
        }

        private long selectionStop = -1L;                   //控制选定终止处;
        public long SelectionStop {
            get {
                return selectionStop;
            }
            set {
                SetProperty(ref selectionStop, value);
            }
        }

        private long position = 0;
        public long Position {                      //当前位置;
            get {
                return position;
            }
            set {
                SetProperty(ref position, value,notifyDiffrent:true);
            }
        }
        
        public void NotifyPosition(long position) {         //通知后台属性position已经变化;
            this.position = position;
        }
        

        public abstract event EventHandler<string> FindNextRequired; //要求查找下次满足条件的字符串;
        public abstract event EventHandler SubmitChangesRequired;
    }
}
