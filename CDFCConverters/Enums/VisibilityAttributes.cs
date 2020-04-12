using System;

namespace CDFCConverters.Enums {
    /// <summary>
    /// 转换可视的枚举;
    /// </summary>
    [Flags]
    public enum VisibilityAttributes {
        Normal = 0,
        Save = 1,//是否保留位置;
        Reverse = 2//是否反转;
    }
}
