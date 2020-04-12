using CDFCEntities.Structs;
namespace CDFCEntities.DeviceInfoes {
    public class CHS {
        public static CHS Create(CHSStruct st) {
            CHS chs = new CHS();
            chs.Cylinder = st.m_Cylinder;
            chs.HeadTrack = st.m_Head_Track;
            chs.TrackSector = st.m_Track_Sector;
            return chs;
        }
        public ulong Cylinder { get; set; }                   //柱面数
        public ulong HeadTrack { get; set; }              //每柱面磁道数
        public uint TrackSector { get; set; }
    }
}
