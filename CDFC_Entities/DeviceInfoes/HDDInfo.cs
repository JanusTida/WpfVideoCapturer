using CDFCEntities.Structs;
using CDFC.Util.PInvoke;
using System;

namespace CDFCEntities.DeviceInfoes{
    public class HddInfo {
        public static HddInfo Create(HDDInfoStruct st) {
            HddInfo hddInfo = new HddInfo();
            hddInfo.ID = st.ID;
            hddInfo.VendorID = st.VendorID;
            hddInfo.ProductID = st.ProductID;
            hddInfo.ProductRevision = st.ProductRevision;
            hddInfo.SerialNumber = st.SerialNumber;

            try {
                if (st.info != IntPtr.Zero) {
                    var hddInfo2Struct = st.info.GetStructure<HDDInfo2Struct>();
                    HddInfo2 hddInfo2 = HddInfo2.Create(hddInfo2Struct);
                    hddInfo.HddInfo2 = hddInfo2;
                }
                else {
                    return hddInfo;
                }
            }
            catch {
                return hddInfo;
            }
            return hddInfo;
        }

        public int ID { get; set; }
        public string VendorID { get; set; }
        public string ProductID { get; set; }
        public string ProductRevision { get; set; }
        public string SerialNumber { get; set; }
        public char Lable { get; set; }
        public HddInfo2 HddInfo2 { get; set; }
    }
}
