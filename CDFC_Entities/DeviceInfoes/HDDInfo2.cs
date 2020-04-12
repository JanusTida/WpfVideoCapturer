using CDFCEntities.Structs;

namespace CDFCEntities.DeviceInfoes {
    public class HddInfo2 {
        public static HddInfo2 Create(HDDInfo2Struct st) {
            HddInfo2 hddInfo2 = new HddInfo2();
            hddInfo2.ID = st.ID;
            hddInfo2.szModelNumber = st.szModelNumber;
            hddInfo2.szSerialNumber = st.szSerialNumber;
            hddInfo2.szControllerNumber = st.szControllerNumber;
            return hddInfo2;
        }
        public int ID { get; set; }
        public string szModelNumber { get; set; }
        public string szSerialNumber { get; set; }
        public string szControllerNumber { get; set; }
    }
}
