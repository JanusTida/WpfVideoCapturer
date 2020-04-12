using CDFCCultures.Managers;
using CDFCEntities.Enums;
using System;
using static CDFCCultures.Managers.ManagerLocator;

namespace CDFCSetting {
    /// <summary>
    /// 品牌类型;
    /// </summary>
    public class DeviceType {
        public int ID { get; set; }
        private string _info;
        public string Info => _info??(_info = DeviceTypeEnum.GetBrandString());
        public DeviceTypeEnum DeviceTypeEnum { get; set; } = DeviceTypeEnum.Unknown;
    }

    /// <summary>
    /// 设备类型;
    /// </summary>
    public class VersionType {
        public int ID { get; set; }
        public string Info { get; set; }
        //是否是属于MP4的扫描范围;
        public bool IsMp4Class { get; set; }
        public DeviceTypeEnum DeviceType { get; private set; }
        public bool DateOk { get; private set; } = true;
        /// <summary>
        /// 基本版本类型的构造器;
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <param name="dateOk">时间是否可以操作</param>
        public VersionType(DeviceTypeEnum deviceType, bool dateOk = true) {
            this.DeviceType = deviceType;
            DateOk = dateOk;
        }
    }

    public static class ScanSetting {
        public static DeviceTypeEnum DeviceType { get; set; }
        public static ScanMethod ScanMethod { get; set; }
        
        //是否是MP4的扫描范围;
        private static bool isMP4Class;
        public static bool IsMP4Class {
            get {
                return isMP4Class;
            }
            set {
                isMP4Class = value;
            }
        }

        public static string StartingTime { get; set; }
        
        public static string ExtensionName { get; set; } = "dav";
        //扇区大小;
        public static int SectorSize { get; set; } = 512;
        public static VersionType VersionType {get;set;}
        public static DeviceType DeviceTypeInfo { get; set; }
    }

    public static class DeviceTypeHelper {
        public static string GetBrandString(this DeviceTypeEnum dType) {
            return FindResourceString(dType.ToString());
                //switch (dType) {
                //    case DeviceTypeEnum.AnLian:
                //        return "安联锐士";
                //    case DeviceTypeEnum.Canon:
                //        return "佳能";
                //    case DeviceTypeEnum.ChuangZe:
                //        return "创泽视讯";
                //    case DeviceTypeEnum.DaHua:
                //        return "大华监控机";
                //    case DeviceTypeEnum.HaiSi:
                //        return "海思监控机";
                //    case DeviceTypeEnum.HanBang:
                //        return "汉邦监控机";
                //    case DeviceTypeEnum.HaiKang:
                //        return "海康威视";
                //    case DeviceTypeEnum.HaiShiTai:
                //        return "海视泰监控机";
                //    case DeviceTypeEnum.ZhongWei:
                //        return "中维监控机";
                //    case DeviceTypeEnum.XingKang:
                //        return "兴康监控机";
                //    case DeviceTypeEnum.H264:
                //        return "H264监控机";
                //    case DeviceTypeEnum.RuiShi:
                //        return "瑞视监控机";
                //    case DeviceTypeEnum.Sony:
                //        return "索尼";
                //    case DeviceTypeEnum.Panasonic:
                //        return "松下";
                //    case DeviceTypeEnum.WFS:
                //        return "WFS监控机";
                //    case DeviceTypeEnum.GoPro:
                //        return "GoPro";
                //    case DeviceTypeEnum.LingDu:
                //        return "凌渡";
                //    case DeviceTypeEnum.JingYi:
                //        return "警翼执法记录仪";
                //    case DeviceTypeEnum.ShanLing:
                //        return "善领";
                //    case DeviceTypeEnum.XingJi:
                //        return "星际执法记录仪";
                //    case DeviceTypeEnum.UnknownCar:
                //        return "未知品牌1";
                //    case DeviceTypeEnum.WJCL:
                //        return "为佳长凌";
                //    case DeviceTypeEnum.YinTan:
                //        return "鹰潭";
                //    case DeviceTypeEnum.Unknown:
                //        return "未知设备";
                //    default:
                //        throw new NotImplementedException("当前设备不支持!");
                //}
           
        }
    }
}
