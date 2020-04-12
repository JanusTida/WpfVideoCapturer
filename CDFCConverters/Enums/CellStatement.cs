using System;

namespace CDFCConverter.Enums {
    [Flags]
    public enum CellStatement {
        None = 0,//无任何状态
        HasFile = 1,//存在文件
        Chosen = 2,//被选中(属于某个文件)
        Selected = 4,//被选中(属于被截取片段)
        Head = 8,//文件头(MP4)
        Tile = 16,//文件尾(MP4)
        Ticked = 32
    }
}
