namespace CDFCVideoExactor.Interfaces {
    /// <summary>
    /// 扫描控制器接口;
    /// </summary>
    public interface IScanningController {
        bool Init();
        //开始扫描;
        void Stop();
        //终止扫描;
        void Start();
        //是否处于扫描中;
        bool IsScanning { get;  }
    }
}
