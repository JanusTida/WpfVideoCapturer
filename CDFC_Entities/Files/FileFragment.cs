using CDFCEntities.Structs;

namespace CDFCEntities.Files {
    /// <summary>
    /// 文件碎片类型;
    /// </summary>
    public class FileFragment {
        public static FileFragment Create(FileFragmentStruct st) {
            FileFragment fileFragment = new FileFragment();
            fileFragment.StartAddress = (long)st.StartAddress;
            fileFragment.StartAddress1 = (long)st.StartAddress1;
            fileFragment.StartAddress2 = (long)st.StartAddress2;
            fileFragment.Size = (long)st.Size;
            fileFragment.ChannelNO = st.ChannelNO;
            fileFragment.StartDate = (uint)st.StartDate;
            fileFragment.EndDate = st.EndDate;
            //fileFragment.Next = st.Next;
            return fileFragment;
        }
        public long StartAddress {
            get {
                return startAddress;
            }
            set {

                startAddress = value;
            }
        }

        private long startAddress;
        
        public long StartAddress1 { get; set; }

        public long StartAddress2 { get; set; }
        public long Size { get; set; }
        public int ChannelNO { get; set; }
        public uint StartDate { get; set; }                  //文件开始时间
        public uint EndDate { get; set; }                    //文件结束时间
        public long EndAddress {
            get {
                return StartAddress + Size;
            }
        }
        public Video Video { get; private set; } //所属文件;
       // public IntPtr Next { get; set; }
    }
}
