using CDllInvoker.StaticMethods;
using System;

namespace CDllInvoker.Entities {
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
