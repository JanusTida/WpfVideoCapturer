using CDFCEntities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Runtime.InteropServices;
using CDFCEntities.CScanMethods;
using System.Runtime.ExceptionServices;

namespace CDFCConverters.Converters {
    public class DateNumToDateStringConverter : IValueConverter {
        private static DateNumToDateStringConverter staticInstance;
        public static DateNumToDateStringConverter StaticInstance {
            get {
                return staticInstance ??
                    (staticInstance = new DateNumToDateStringConverter());
            }
        }

        private static DeviceTypeEnum DeviceType {
            get {
                return CDFCSetting.ScanSetting.DeviceType;
            }
        }
        //基本类型号;
        private static int? VersionID {
            get {
                return CDFCSetting.ScanSetting.VersionType?.ID;
            }
        }

        private static ScanMethod ScanMethod {
            get {
                return CDFCSetting.ScanSetting.ScanMethod;
            }
        }
        /// <summary>
        /// 恢复文件所需转换时间方法;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ConvertToStorageDateString(uint dateNum) {
            var dt = GetDateTimeByDeviceType(dateNum);
            if (dt.HasValue) {
                var dtVal = dt.Value;
                return string.Format("{0}-{1:D2}-{2:D2}-{3:D2}-{4:D2}-{5:D2}",
                    dtVal.Year, dtVal.Month, dtVal.Day, dtVal.Hour,
                    dtVal.Minute, dtVal.Second);
            }
            else {
                return "0000-00-00-00-00-00";
            }
        }

        #region HexNums各系统的年月日进制规则;
        /// <summary>
        /// 大华系统的年月日，分，进制数字规则；
        /// </summary>
        private static readonly uint[] DHHexNum = new uint[] {
            67108864,4194304,131072,4096,64,1
        };
        /// <summary>
        /// 海康系统的进制年月日，时分秒进制;
        /// </summary>
        private static readonly uint[] HKHexNum = new uint[] {
            67108864,4194304,131072,4096,64,1
        };
        //海康的初始时间(文件系统)
        private static DateTime hkIniDateTime = new DateTime(1970, 1, 1);
        /// <summary>
        /// 创泽系统的年月日，分，进制数字规则；
        /// </summary>
        private static readonly uint[] CZHexNum = new uint[] {
            67108864,4194304,131072,4096,64,1
        };
        /// <summary>
        /// 海视泰的初始时间;
        /// </summary>
        private static DateTime hstIniDateTime = new DateTime(1969, 12, 31);
        /// <summary>
        /// 汉邦的初始时间;
        /// </summary>
        private static DateTime? hbIniDateTime;
        private static DateTime HbIniDateTime {
            get {
                return (hbIniDateTime ??
                    (hbIniDateTime = new DateTime(1970, 1, 1))).Value;
            }
        }
        //3029529600 2000/1/1/00/00/00 MP4的起始时间参考值;
        #endregion

        #region 各系统的时间获得方法
        /// <summary>
        /// 获得大华的时间
        /// </summary>
        /// <param name="dateNum">从起始时间的偏移量</param>
        /// <returns>用短整型数组表示的年月日，时分秒</returns>
        private static DateTime? GetDHTime(uint dateNum) {
            short[] dateNums = new short[6];
            DateTime dt;

            for (byte index = 0;index < 6; index++) {
                var innerDateNum = dateNum;
                for(byte innerIndex = 0;innerIndex < index; innerIndex++) {
                    innerDateNum %= DHHexNum[innerIndex];
                }
                if(innerDateNum != 0) { 
                    dateNums[index] = (short)(innerDateNum / DHHexNum[index]);
                }
            }
            
            try {
                //大华起始时间为2000年初始时间;
                dt = new DateTime(dateNums[0] + 2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                return dt;
            }
            //若时间构造失败;
            catch {
                return null;
            }
        }
        /// <summary>
        /// 获得海康的时间;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime? GetHKTime(uint dateNum) {
            short[] dateNums = new short[6];
            DateTime dt;

            dateNum *= 4;
            for (byte index = 0; index < 6; index++) {
                var innerDateNum = dateNum;
                for (byte innerIndex = 0; innerIndex < index; innerIndex++) {
                    innerDateNum %= HKHexNum[innerIndex];
                }
                if (innerDateNum != 0) {
                    dateNums[index] = System.Convert.ToInt16(innerDateNum / HKHexNum[index]);
                }
            }
            
            try {
                dt = new DateTime(dateNums[0] + 2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                return dt;
            }
            //若时间构造失败;
            catch {
                return null;
            }
        }
        /// <summary>
        /// 获得海康系统的时间(文件系统)
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime? GetHKTime_FS(uint dateNum) {
            DateTime? dt = hkIniDateTime.AddSeconds(dateNum);
            return dt;
        }
        /// <summary>
        /// 获得创泽系统的时间
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime? GetCZTime(uint dateNum) {
            short[] dateNums = new short[6];
            DateTime dt;

            for (byte index = 0; index < 6; index++) {
                var innerDateNum = dateNum;
                for (byte innerIndex = 0; innerIndex < index; innerIndex++) {
                    innerDateNum %= CZHexNum[innerIndex];
                }
                if (innerDateNum != 0) {
                    dateNums[index] = System.Convert.ToInt16(innerDateNum / CZHexNum[index]);
                }
            }
            try { 
                dt = new DateTime(dateNums[0]+2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                return dt;
            }
            //若时间构造失败;
            catch {
                return null;
            }
        }
        /// <summary>
        /// 获得海视泰的时间;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime? GetHSTTime(uint dateNum) {
            DateTime? dt = hstIniDateTime.AddSeconds(dateNum);
            return dt;
        }

        /// <summary>
        /// 获得汉邦的时间;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime? GetHBTime(uint dateNum) {
            DateTime? dt = HbIniDateTime.AddSeconds(dateNum);
            return dt;
        }

        /// <summary>
        /// 获得星际的时间;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime xjIniDateTime = DateTime.Parse("1904/01/01 12:00:00 AM");
        private static DateTime? GetXJTime(uint dateNum) {
            return xjIniDateTime.AddSeconds(dateNum);
        }

        /// <summary>
        /// 获得MOV时间;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        private static DateTime? movInitTime = DateTime.Parse("2000/01/01");
        private static DateTime?GetCanonTime(uint dateNum) {
            return movInitTime.Value.AddSeconds(dateNum);
        }
        #endregion

        [HandleProcessCorruptedStateExceptions]
        private static DateTime? GetDateTimeByDeviceType(uint dateNum) {
            IntPtr dateNumsPtr = IntPtr.Zero;
            Action<ulong, IntPtr> ulongConverter = null;
            DateTime? dt = null;

            //根据设备类型不同，有不同的时间计算算法;
            switch (DeviceType) {
                case DeviceTypeEnum.DaHua:
                case DeviceTypeEnum.WFS:
                    dt = GetDHTime(dateNum);
                    break;
                case DeviceTypeEnum.AnLian:
                    ulongConverter = AnLianScanMethods.cdfc_object_date_converter;
                    break;
                case DeviceTypeEnum.HaiKang:
                    //converterAct = HKRecoveryMethods.cdfc_haikang_date_converter;
                    switch (ScanMethod) {
                        case ScanMethod.FileSystem:
                            dt = GetHKTime_FS(dateNum);
                            break;
                        default:
                            dt = GetHKTime(dateNum);
                            break;
                    }
                    break;

                case DeviceTypeEnum.HanBang:
                    dt = GetHBTime(dateNum);
                    break;
                case DeviceTypeEnum.XingKang:
                case DeviceTypeEnum.ZhongWei:
                case DeviceTypeEnum.H264:
                    ulongConverter = MP4ScanMethods.cdfc_object_date_converter;
                    break;
                case DeviceTypeEnum.HaiShiTai:
                    dt = GetHSTTime(dateNum);
                    break;
                case DeviceTypeEnum.ChuangZe:
                    dt = GetCZTime(dateNum);
                    break;
                case DeviceTypeEnum.Canon:
                case DeviceTypeEnum.MOV:
                    dt = GetCanonTime(dateNum);
                    break;
                case DeviceTypeEnum.Sony:
                    ulongConverter = SonyScanMethods.cdfc_object_date_converter;
                    break;
                case DeviceTypeEnum.Panasonic:
                    ulongConverter = PanasonicScanMethods.cdfc_object_date_converter;
                    break;
                case DeviceTypeEnum.HaiSi:
                    ulongConverter = HaiSiScanMethods.cdfc_object_date_converter;
                    break;
                case DeviceTypeEnum.JingYi:
                    //if(VersionID == 2) {
                        dt = GetXJTime(dateNum);
                    //}
                    //else {
                       // ulongConverter = MP4ScanMethods.cdfc_object_date_converter;
                    //}
                    break;
                 //善领的处理;
                case DeviceTypeEnum.ShanLing:
                    return null;
                case DeviceTypeEnum.YinTan:
                    dt = HbIniDateTime.AddSeconds(dateNum);
                    break;
                case DeviceTypeEnum.UnknownCar:
                    ulongConverter = UnknownCarScanMethods.cdfc_object_date_converter;
                    break;
            }

            //若无法自己得到接口
            if(dt == null) {
                try { 
                    int byteSize = Marshal.SizeOf(typeof(byte));
                    dateNumsPtr = Marshal.AllocHGlobal(6 * byteSize);
                    ulongConverter?.Invoke(dateNum, dateNumsPtr);
                    var dateNums = new short[6];
                    for (int index = 0; index < 6;index++) {
                        dateNums[index] = Marshal.ReadByte(dateNumsPtr + index * byteSize);
                    }
                    Marshal.FreeHGlobal(dateNumsPtr);
                    dt = new DateTime(dateNums[0] + 2000, dateNums[1], dateNums[2], dateNums[3], dateNums[4], dateNums[5]);
                    return dt;
                }
                catch(AccessViolationException ex) {
                    EventLogger.Logger.WriteLine("时间转换底层错误:"+ex.Message);
                    return null;
                }
                //若时间构造失败;
                catch(Exception ex) {
                    EventLogger.Logger.WriteLine("时间转换错误"+ex.Message);
                    return null;
                }
            }
            
            return dt;
        }

        //表示为空的字符串;
        private const string nullString = "----";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            uint dateNum = 0;
            //System.Convert.ToUInt32(value);
            if(UInt32.TryParse(value.ToString(), out dateNum) && dateNum != 0) {
                DateTime? dt = GetDateTimeByDeviceType(dateNum);
                if (dt.HasValue) {
                    DateTime dtValue = dt.Value;
                    return string.Format("{0}/{1:D2}/{2:D2}  {3:D2}:{4:D2}:{5:D2}", dtValue.Year, dtValue.Month, dtValue.Day, dtValue.Hour,
                        dtValue.Minute, dtValue.Second);
                }
            }
            return nullString;
        }

        /// <summary>
        /// 日期分类所需转换方法;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>
        public static DateTime? ConvertToNullableDate(uint dateNum) {
            if(dateNum == 0) {
                return null;
            }
            var dt = GetDateTimeByDeviceType(dateNum);
            return dt;
        }

        /// <summary>
        /// 日期分类所需转换方法;
        /// </summary>
        /// <param name="dateNum"></param>
        /// <returns></returns>

        public static string ConvertToDateString(uint dateNum) {
            var dt = GetDateTimeByDeviceType(dateNum);
            if (dt.HasValue) {
                var dtVal = dt.Value;
                return string.Format("{0}-{1:D2}-{2:D2}", dtVal.Year,dtVal.Month,dtVal.Day);
            }
            else {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
  
}
