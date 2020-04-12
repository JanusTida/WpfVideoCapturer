using CDFCEntities.Structs;
using System;

namespace CDFCEntities.Files {
    /// <summary>
    /// Mov文件实体;
    /// </summary>
    public partial class MovVideo {
        public int Name { get; private set; }                         //无用 
        public ulong Size { get; set; }			//文件大小
        /// <summary>
        /// 修改日期  
        /// </summary>
	    public string FileData { get; set; } //这里没有结束日期,所以结束日期写---           
        public string FileType { get; private set; }                 //无用
        public ulong FileDataStart { get; private set; }	//无用
	    public ulong FileDataEnd { get; private set; }  //无用
        public ulong FileHeadStart { get; private set; }    //无用
        public ulong FileHeadEnd { get; private set; }	//无用
        public IntPtr VideoPtr { get; private set; }
    }

    public partial class MovVideo {
        public static MovVideo Create(IntPtr videoPtr,MovVideoStruct st) {
            MovVideo video = new MovVideo();
            video.Size = st.FileSize;
            video.FileData = st.FileData;
            //video.FileType = st.FileType;
            //video.FileDataStart = st.File_Data_Start;
            //video.FileDataEnd = st.File_Data_End;
            //video.FileHeadStart = st.File_Head_Start;
            //video.FileHeadEnd = st.File_Head_End;
            video.VideoPtr = videoPtr;
            return video;
        }
    }
}
